using FASTER.core;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Application.Interfaces.RedditRepository;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;
using System.Text.Json;

namespace Neal.Reddit.Infrastructure.Faster.Repository.Services.Repository;

/// <summary>
/// Represents a repository for <see cref="Link"/> using the Microsoft.FASTER (KV) Library consisting of a hybrid in-memory and disk based log with checkpoints.
/// The hybrid log is stored in the temporary path of the executing environment.
/// To change the storage location, the temporary path must be set according to your OS requirements.
/// </summary>
public sealed class FasterPostRepository : IPostRepository
{
// Justification - Initializers used in constructor
#pragma warning disable CS8618

    #region Fields

    #region Post Store

    private static FasterKV<string, string> postsStore;

    private static AsyncPool<ClientSession<string, string, string, string, Empty, IFunctions<string, string, string, string, Empty>>> postsSessionPool;

    private static IDevice postsObjectLog;

    private static IDevice postsLog;

    #endregion Post Store

    #region Upvote Store

    private static FasterKV<string, int> upvotesStore;

    private static AsyncPool<ClientSession<string, int, int, int, Empty, IFunctions<string, int, int, int, Empty>>> upvotesSessionPool;

    private static IDevice upvotesObjectLog;

    private static IDevice upvotesLog;

    #endregion Upvote Store

    #region Author Store

    private static FasterKV<string, int> authorsStore;

    private static AsyncPool<ClientSession<string, int, int, int, Empty, IFunctions<string, int, int, int, Empty>>> authorsSessionPool;

    private static IDevice authorsObjectLog;

    private static IDevice authorsLog;

    #endregion Author Store

    private static Thread commitThread;

    private readonly ILogger<FasterPostRepository> logger;

    private static readonly CancellationTokenSource cancellationTokenSource = new();

    #endregion Fields

#pragma warning restore CS8618

    public FasterPostRepository(ILogger<FasterPostRepository> logger)
    {
        // TODO: Create a FasterKV configuration model so this can be configured to be project specific
        string rootPath = Path.Combine(Path.GetTempPath(), "Logs", "Faster");
        this.logger = logger;

        InstantiatePostsStore(rootPath);
        InstantiateAuthorsStore(rootPath);

        // Ensure a commit thread is running once records have been added at least once
        if (commitThread is null)
        {
            commitThread = new Thread(new ThreadStart(() => this.CommitThread(cancellationTokenSource.Token)));
            commitThread.Start();
        }
    }

    public void Dispose()
    {
        // Cancel background commit thread
        cancellationTokenSource.Cancel();

        // Dispose of pools and stores - ensures graceful shutdown
        postsSessionPool.Dispose();
        postsStore.Dispose();
        postsSessionPool.Dispose();
        authorsStore.Dispose();
    }

    public async Task AddPostsAsync(IEnumerable<Link> posts)
    {
        var postsSession = GetPostsSession();
        var authorsSession = GetAuthorsSession();

        try
        {
            foreach (var post in posts)
            {
                await postsSession.RMWAsync(new string(post.Name), JsonSerializer.Serialize(post));

                if (post.AuthorId is null
                    || !string.IsNullOrWhiteSpace(post.AuthorId))
                {
                    continue;
                }

                await authorsSession.RMWAsync(post.AuthorId, 1);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogCritical(ExceptionMessages.GenericException, ex.Message);
        }

        postsSessionPool.Return(postsSession);
        authorsSessionPool.Return(authorsSession);
    }

    public Task<List<Link>> GetAllPostsAsync(Pagination pagination)
    {
        int recordSize = postsStore.Log.FixedRecordSize;
        long headAddress = postsStore.Log.HeadAddress;
        long pageBytes = pagination.PageSize * recordSize;
        long pageStart = headAddress + ((pagination.Page - 1) * pageBytes);
        long pageEnd = pageStart + pageBytes;
        var postsSession = GetPostsSession();
        var output = new List<Link>();

        if (pageStart > postsStore.Log.TailAddress)
        {
            return Task.FromResult(output);
        }

        if (pageEnd > postsStore.Log.TailAddress)
        {
            pageEnd = postsStore.Log.TailAddress;
        }

        var iterator = postsSession.Iterate(pageEnd);

        while (iterator.GetNext(out var recordInfo, out string key, out string value))
        {
            if (iterator.CurrentAddress < pageStart)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<Link>(value);

                    if (dto is not null)
                    {
                        output.Add(dto);
                    }
                }
                catch (Exception)
                {
                    this.logger.LogCritical(ExceptionMessages.SerializationError, nameof(Link));
                }
            }
        }

        postsSessionPool.Return(postsSession);

        return Task.FromResult(output);
    }

    public Task<long> GetCountAsync()
    {
        long logCount = (postsStore.Log.TailAddress - postsStore.Log.HeadAddress) / postsStore.Log.FixedRecordSize;
        long memoryCount = (postsStore.ReadCache.TailAddress - postsStore.ReadCache.HeadAddress) / postsStore.ReadCache.FixedRecordSize;

        return Task.FromResult(logCount + memoryCount);
    }

    public Task<IEnumerable<KeyValuePair<string, int>>> GetTopPosts(int top = 10)
    {
        var upvotesSession = GetUpvotesSession();
        var posts = new Dictionary<string, int>();
        var iterator = upvotesSession.Iterate();

        while (iterator.GetNext(out var recordInfo, out string key, out int value))
        {
            if (!string.IsNullOrEmpty(key)
                && value > 0)
            {
                posts.Add(key, value);
            }
        }

        var results = posts
            .ToList()
            .OrderByDescending(post => post.Value)
            .ThenBy(post => post.Key)
            .Take(top)
            .Select(post => new KeyValuePair<string, int>(post.Key, post.Value));

        upvotesSessionPool.Return(upvotesSession);

        return Task.FromResult(results);
    }

    public Task<IEnumerable<KeyValuePair<string, int>>> GetTopAuthors(int top = 10)
    {
        var authorsSession = GetAuthorsSession();
        var authors = new Dictionary<string, int>();
        var iterator = authorsSession.Iterate();

        while (iterator.GetNext(out var recordInfo, out string key, out int value))
        {
            if (!string.IsNullOrEmpty(key)
                && value > 0)
            {
                authors.Add(key, value);
            }
        }

        var results = authors
            .ToList()
            .OrderByDescending(author => author.Value)
            .ThenBy(author => author.Key)
            .Take(top)
            .Select(author => new KeyValuePair<string, int>(author.Key, author.Value));

        authorsSessionPool.Return(authorsSession);

        return Task.FromResult(results);
    }

    #region Private Methods

    private void CommitThread(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Thread.Sleep(TimeSpan.FromSeconds(30));

            postsStore
                .TakeHybridLogCheckpointAsync(CheckpointType.Snapshot)
                .AsTask()
                .GetAwaiter()
                .GetResult();

            authorsStore
                .TakeHybridLogCheckpointAsync(CheckpointType.Snapshot)
                .AsTask()
                .GetAwaiter()
                .GetResult();

            this.logger.LogInformation(
                ApplicationStatusMessages.RecordCount,
                nameof(FasterPostRepository),
                postsStore.EntryCount,
                authorsStore.EntryCount);
        }
    }

    #region Initialization

    private static void InstantiatePostsStore(string rootPath)
    {
        string postsPath = Path.Combine(rootPath, "posts");
        var postsFunctions = new SimpleFunctions<string, string>((a, b) => b);

        postsObjectLog = Devices.CreateLogDevice(Path.Combine(postsPath, "_obj.log"), recoverDevice: true);
        postsLog = Devices.CreateLogDevice(Path.Combine(postsPath, ".log"), recoverDevice: true);

        var settings = new FasterKVSettings<string, string>(postsPath, deleteDirOnDispose: false)
        {
            IndexSize = 1L << 20,
            TryRecoverLatest = true,
            LogDevice = postsLog,
            ObjectLogDevice = postsObjectLog,
            CheckpointDir = Path.Combine(postsPath, "CheckPoints"),
            ReadCacheEnabled = true
        };

        postsStore = new FasterKV<string, string>(settings);
        postsSessionPool = new AsyncPool<ClientSession<string, string, string, string, Empty, IFunctions<string, string, string, string, Empty>>>(
            settings.LogDevice.ThrottleLimit,
            () => postsStore.For(postsFunctions).NewSession<IFunctions<string, string, string, string, Empty>>());
    }

    private static void InstantiateUpvotesStore(string rootPath)
    {
        string upvotesPath = Path.Combine(rootPath, "upvotes");
        var upvotesFunctions = new SimpleFunctions<string, int>((a, b) => a + b);

        upvotesObjectLog = Devices.CreateLogDevice(Path.Combine(upvotesPath, "_obj.log"), recoverDevice: true);
        upvotesLog = Devices.CreateLogDevice(Path.Combine(upvotesPath, ".log"), recoverDevice: true);

        var settings = new FasterKVSettings<string, int>(upvotesPath, deleteDirOnDispose: false)
        {
            IndexSize = 1L << 20,
            TryRecoverLatest = true,
            LogDevice = upvotesLog,
            ObjectLogDevice = upvotesObjectLog,
            CheckpointDir = Path.Combine(upvotesPath, "CheckPoints")
        };

        upvotesStore = new FasterKV<string, int>(settings);
        upvotesSessionPool = new AsyncPool<ClientSession<string, int, int, int, Empty, IFunctions<string, int, int, int, Empty>>>(
            settings.LogDevice.ThrottleLimit,
            () => upvotesStore.For(upvotesFunctions).NewSession<IFunctions<string, int, int, int, Empty>>());
    }

    private static void InstantiateAuthorsStore(string rootPath)
    {
        string authorsPath = Path.Combine(rootPath, "authors");
        var authorsFunctions = new SimpleFunctions<string, int>((a, b) => a + b);

        authorsObjectLog = Devices.CreateLogDevice(Path.Combine(authorsPath, "_obj.log"), recoverDevice: true);
        authorsLog = Devices.CreateLogDevice(Path.Combine(authorsPath, ".log"), recoverDevice: true);

        var settings = new FasterKVSettings<string, int>(authorsPath, deleteDirOnDispose: false)
        {
            IndexSize = 1L << 20,
            TryRecoverLatest = true,
            LogDevice = authorsLog,
            ObjectLogDevice = authorsObjectLog,
            CheckpointDir = Path.Combine(authorsPath, "CheckPoints")
        };

        authorsStore = new FasterKV<string, int>(settings);
        authorsSessionPool = new AsyncPool<ClientSession<string, int, int, int, Empty, IFunctions<string, int, int, int, Empty>>>(
            settings.LogDevice.ThrottleLimit,
            () => authorsStore.For(authorsFunctions).NewSession<IFunctions<string, int, int, int, Empty>>());
    }

    #endregion Initialization

    #region Sessions

    private static ClientSession<string, string, string, string, Empty, IFunctions<string, string, string, string, Empty>> GetPostsSession()
    {
        if (!postsSessionPool.TryGet(out var postsSession))
        {
            postsSession = postsSessionPool
                .GetAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();
        }

        return postsSession;
    }

    private static ClientSession<string, int, int, int, Empty, IFunctions<string, int, int, int, Empty>> GetUpvotesSession()
    {
        if (!upvotesSessionPool.TryGet(out var upvotesSession))
        {
            upvotesSession = upvotesSessionPool
                .GetAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();
        }

        return upvotesSession;
    }

    private static ClientSession<string, int, int, int, Empty, IFunctions<string, int, int, int, Empty>> GetAuthorsSession()
    {
        if (!authorsSessionPool.TryGet(out var authorsSession))
        {
            authorsSession = authorsSessionPool
                .GetAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();
        }

        return authorsSession;
    }

    #endregion Sessions

    #endregion Private Methods
}