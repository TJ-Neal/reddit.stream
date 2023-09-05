using FASTER.core;
using Microsoft.Extensions.Logging;
using Neal.Reddit.Application.Constants.Messages;
using Neal.Reddit.Application.Interfaces.RedditRepository;
using Neal.Reddit.Core.Entities.Configuration;
using Neal.Reddit.Core.Entities.Reddit;
using System.Text.Json;

namespace Neal.Reddit.Infrastructure.Faster.Repository.Services.Repository;

/// <summary>
/// Represents a repository for <see cref="DataBase"/> using the Microsoft.FASTER (KV) Library consisting of a hybrid in-memory and disk based log with checkpoints.
/// The hybrid log is stored in the temporary path of the executing environment.
/// To change the storage location, the temporary path must be set according to your OS requirements.
/// </summary>
public sealed class FasterRecordRepository : IRedditRepository
{
// Justification - Initializers used in constructor
#pragma warning disable CS8618

    #region Fields

    #region Record Store

    private static FasterKV<string, string> redditRecordStore;

    private static AsyncPool<ClientSession<string, string, string, string, Empty, IFunctions<string, string, string, string, Empty>>> recordSessionPool;

    private static IDevice redditRecordObjectLog;

    private static IDevice redditRecordLog;

    #endregion Record Store

    #region Author Store

    private static FasterKV<string, int> authorsStore;

    private static AsyncPool<ClientSession<string, int, int, int, Empty, IFunctions<string, int, int, int, Empty>>> authorsSessionPool;

    private static IDevice authorsObjectLog;

    private static IDevice authorsLog;

    private static readonly CancellationTokenSource cancellationTokenSource = new();

    #endregion Author Store

    private static Thread commitThread;

    private readonly ILogger<FasterRecordRepository> logger;

    #endregion Fields

#pragma warning restore CS8618

    public FasterRecordRepository(ILogger<FasterRecordRepository> logger)
    {
        // TODO: Create a FasterKV configuration model so this can be configured to be project specific
        string rootPath = Path.Combine(Path.GetTempPath(), "Logs", "Faster");
        this.logger = logger;

        InstantiateRecordsStore(rootPath);
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
        authorsSessionPool.Dispose();
        authorsStore.Dispose();
        recordSessionPool.Dispose();
        redditRecordStore.Dispose();
    }

    public async Task AddRecordsAsync(IEnumerable<DataBase> records)
    {
        var recordSession = GetRecordsSession();
        var authorsSession = GetAuthorsSession();

        try
        {
            foreach (var record in records)
            {
                await recordSession.RMWAsync(new string(record.Name), JsonSerializer.Serialize(record));

                if (record.AuthorId is null
                    || !string.IsNullOrWhiteSpace(record.AuthorId))
                {
                    continue;
                }

                await authorsSession.RMWAsync(record.AuthorId, 1);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogCritical(ExceptionMessages.GenericException, ex.Message);
        }

        recordSessionPool.Return(recordSession);
        authorsSessionPool.Return(authorsSession);
    }

    public Task<List<DataBase>> GetAllRecordsAsync(Pagination pagination)
    {
        int recordSize = redditRecordStore.Log.FixedRecordSize;
        long headAddress = redditRecordStore.Log.HeadAddress;
        long pageBytes = pagination.PageSize * recordSize;
        long pageStart = headAddress + ((pagination.Page - 1) * pageBytes);
        long pageEnd = pageStart + pageBytes;
        var recordsSession = GetRecordsSession();
        var output = new List<DataBase>();

        if (pageStart > redditRecordStore.Log.TailAddress)
        {
            return Task.FromResult(output);
        }

        if (pageEnd > redditRecordStore.Log.TailAddress)
        {
            pageEnd = redditRecordStore.Log.TailAddress;
        }

        var iterator = recordsSession.Iterate(pageEnd);

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
                    var dto = JsonSerializer.Deserialize<DataBase>(value);

                    if (dto is not null)
                    {
                        output.Add(dto);
                    }
                }
                catch (Exception)
                {
                    this.logger.LogCritical(ExceptionMessages.SerializationError, nameof(DataBase));
                }
            }
        }

        recordSessionPool.Return(recordsSession);

        return Task.FromResult(output);
    }

    public Task<long> GetCountAsync()
    {
        long logCount = (redditRecordStore.Log.TailAddress - redditRecordStore.Log.HeadAddress) / redditRecordStore.Log.FixedRecordSize;
        long memoryCount = (redditRecordStore.ReadCache.TailAddress - redditRecordStore.ReadCache.HeadAddress) / redditRecordStore.ReadCache.FixedRecordSize;

        return Task.FromResult(logCount + memoryCount);
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

            redditRecordStore
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
                nameof(FasterRecordRepository),
                redditRecordStore.EntryCount,
                authorsStore.EntryCount);
        }
    }

    #region Initialization

    private static void InstantiateRecordsStore(string rootPath)
    {
        string recordsPath = Path.Combine(rootPath, "records");
        var recordsFunctions = new SimpleFunctions<string, string>((a, b) => b);

        redditRecordObjectLog = Devices.CreateLogDevice(Path.Combine(recordsPath, "_obj.log"), recoverDevice: true);
        redditRecordLog = Devices.CreateLogDevice(Path.Combine(recordsPath, ".log"), recoverDevice: true);

        var settings = new FasterKVSettings<string, string>(recordsPath, deleteDirOnDispose: false)
        {
            IndexSize = 1L << 20,
            TryRecoverLatest = true,
            LogDevice = redditRecordLog,
            ObjectLogDevice = redditRecordObjectLog,
            CheckpointDir = Path.Combine(recordsPath, "CheckPoints"),
            ReadCacheEnabled = true
        };

        redditRecordStore = new FasterKV<string, string>(settings);
        recordSessionPool = new AsyncPool<ClientSession<string, string, string, string, Empty, IFunctions<string, string, string, string, Empty>>>(
            settings.LogDevice.ThrottleLimit,
            () => redditRecordStore.For(recordsFunctions).NewSession<IFunctions<string, string, string, string, Empty>>());
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
            LogDevice = authorsObjectLog,
            ObjectLogDevice = authorsLog,
            CheckpointDir = Path.Combine(authorsPath, "CheckPoints")
        };

        authorsStore = new FasterKV<string, int>(settings);
        authorsSessionPool = new AsyncPool<ClientSession<string, int, int, int, Empty, IFunctions<string, int, int, int, Empty>>>(
            settings.LogDevice.ThrottleLimit,
            () => authorsStore.For(authorsFunctions).NewSession<IFunctions<string, int, int, int, Empty>>());
    }

    #endregion Initialization

    #region Sessions

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

    private static ClientSession<string, string, string, string, Empty, IFunctions<string, string, string, string, Empty>> GetRecordsSession()
    {
        if (!recordSessionPool.TryGet(out var recordsSession))
        {
            recordsSession = recordSessionPool
                .GetAsync()
                .AsTask()
                .GetAwaiter()
                .GetResult();
        }

        return recordsSession;
    }

    #endregion Sessions

    #endregion Private Methods
}