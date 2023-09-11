<!-- omit from toc -->
# Reddit API Project

Sample code for interacting with, tracking, and analyzing data from the [Reddit API](https://www.reddit.com/dev/api) developed as a proof of concept for a coding challenge.
<br/>

## Table of Contents

- [**1. Purpose**](#1-purpose)
- [**2. Practices**](#2-practices)
- [**3. Concessions**](#3-concessions)
- [**4. Primary Technologies and Packages**](#4-primary-technologies-and-packages)
- [**5. User Secrets**](#6-user-secrets-and-ssl-certs)
  - [**Reddit OAuth Keys**](#reddit-oauth-keys)
- [**7. Getting Started**](#7-getting-started)
  - [**Direct API Access**](#direct-api-access)
  - [**Swagger UI**](#swagger-ui)
- [**7. Conclusion**](#8-conclusion)

## **1. <u>Purpose</u>**

The purpose of this project was to create a [Reddit API](https://www.reddit.com/dev/api) project as a demonstration of my continued grasp of modern software development practices, architecture, principles, and ability to make use of API documentation. As with my first code challenge solution [Twitter Sampled Stream](https://github.com/TJ-Neal/twitter.sampled.stream), I wanted to extend myself and use the opportunity to go above and beyond so I added additional configurations and processes for the ability to monitor not only posts from the time the application starts, but from the entire paginated responses that Reddit will supply from the `new` sorted posts. Of course, there are some limitations. For instance, monitoring `/r/all` without limiting to `MonitorType.AfterStartOnly` posts or monitoring too many subreddits at a time could exceed the `rate limits` depending on the volume encountered.
<br/>

## **2. <u>Practices</u>**

The following practices for modern software development were used as much as possible:
<br/>

- SOLID Principles
- Clean Code Architecture
- Microservice Architecture
- .NET Minimal API
- Repository Pattern
- Event-Driven Architecture
- Structured Logging
- Unit Testing
<br/>

## **3. <u>Concessions</u>**
There are concessions that I made due to the nature of the project, the availability of the previous code challenge solution, and desire for brevity.
<br/>

1. Unit Tests - Though I did include some unit tests to demonstrate my grasp of the subject and use of `Xunit` and `Dependency Injection` in unit testing, they are by no means intended to be exhaustive, are not indicative of `Test Driven Development` (or a lack of appreciation for the practice), and are admittedly quite anemic. My apologies. 
\* `Moq` was not used again due to recent events surrounding the package and authors.
2. Since the original [Twitter Sampled Stream](https://github.com/TJ-Neal/twitter.sampled.stream) project covered `Docker`, `Kafka`, `React`, and `Faster`, I omitted them in this solution for brevity and a shorter turnaround time.
<br/>

## **4. <u>Primary Technologies and Packages</u>**

- [Windows 11](https://www.microsoft.com/en-us/windows?wa=wsignin1.0) - Host/Development OS
- [.NET Core 7](https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core?view=aspnetcore-7.0#recommended-learning-path) - Application framework
- [C# 11](https://learn.microsoft.com/en-us/dotnet/csharp/) - Primary programming language
- [Reddit API](https://www.reddit.com/dev/api) - Data source API
- [MediatR](https://github.com/jbogard/MediatR) - Event-Driven pub/sub mediator
- [Serilog](https://serilog.net/) - .NET Logging implementations
- [Swagger/OpenAPI](https://swagger.io/) - API explorer/UI tool
- [XUnit](https://github.com/xunit/visualstudio.xunit) - Unit tesing framework for .NET
<br/>

## **5. <u>User Secrets</u>**

### **Reddit OAuth Keys**
<br/>

To access the Reddit API you will need to have valid OAuth keys placed into the user secrets of the `RedditReader` and/or `RedditClient.Tests` project.

Open `Manage User Secrets` for the project from the context menu
<br/>

And the following keys and edit the values to match your Reddit application keys:
<br/>

```json
{
    "Credentials": {
        "ClientId": "<PRIVATE_REDDIT_CLIENT_ID>",
        "ClientSecret": "<PRIVATE_REDDIT_CLIENT_SECRET>",
        "DeviceId": "<PRIVATE_DEVICE_ID>"
    }
}
```

<br/>
The `ClientId` and `ClientSecret` are retrieved from your Reddit developer account application page as outlined [here](https://github.com/reddit-archive/reddit/wiki/OAuth2). The `DeviceId` is any unique identifier between 20 and 30 characters.

<br/>

## **6. <u>Getting Started</u>**

To get started, clone this repository to a location on your local machine.
<br/>

The easiest way to execute the solution is to use `Visual Studio 2022` and launching both the API and Reader projects. To do so, load the solution in `Visual Studio 2002` after cloning the repository then open the startup configuration dropdown. From there, select the `Configure Startup Projects...` open. In the dialog, select `Multiple startup projects` and set `Neal.Reddit.API.Simple` and `Neal.Reddit.Infrastructure.Reader` to `Start`. Click `Ok` and then click `Start`. Both the API and Reader console application will start and a browser window should load with the `Swagger UI` for interacting with the API.
<br/>

### **Direct API Access**

The API is also accessible via the direct links to each operation. For instance, the count of posts can be see in the Simple API via http://localhost:4001/api/posts/count.
<br/>

### **Swagger UI**

Most importantly, the API has Swagger UI enabled, giving an interface to interact with the API endpoints. Swagger UI can be reached at the following location:

Simple API: https://localhost:4001/swagger<br/>
<br/>

## **7. <u>Conclusion</u>**

I truly enjoyed creating this updated project for Reddit. Interacting with rate limits and semaphores is not an every day exercise for me. Personally, I am not a fan of the Reddit API and felt that some processes could certainly have been simplified had their API been designed differently. That in itself is a learning experience and tool for pro and con takeaways.

I also believe that this solution can handle a large volume of posts from Reddit; however, due to their rate limitations, exercising it to the maximum quickly reached their limits. That being said, I do think that when used reasonably this solution makes maximum use of the given rate limits and can handle multiple subreddits concurrently.