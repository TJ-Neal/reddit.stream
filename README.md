<!-- omit from toc -->
# Reddit API Project
<br/>

Sample code for interacting with, tracking, and analysizing data from the [Reddit API](https://www.reddit.com/dev/api) developed as a proof of concept for a coding challenge.
<br/><br/>

## Table of Contents

- [**1. Purpose**](#1-purpose)
- [**2. Practices**](#2-practices)
- [**3. Concessions**](#3-concessions)
- [**4. Primary Technologies and Packages**](#4-primary-technologies-and-packages)
- [**5. Prerequisites**](#5-prerequisites)
  - [**5.1 Web API, Reddit Reader, Kafka, and Kafka Reader**](#51-web-api-reddit-reader-kafka-and-kafka-reader)
  - [**5.2 React Front-end UI**](#52-react-front-end-ui)
- [**6. Structure**](#6-structure)
- [**7. User Secrets and `SSL` Certs**](#7-user-secrets-and-ssl-certs)
  - [**Reddit OAuth Keys**](#reddit-oauth-keys)
  - [**Self-signed (dev) `SSL` Certs**](#self-signed-dev-ssl-certs)
- [**8. Getting Started**](#8-getting-started)
  - [**Docker**](#docker)
  - [**Health Checks**](#health-checks)
  - [**Direct API Access**](#direct-api-access)
  - [**Swagger UI**](#swagger-ui)
  - [**Sample React Application**](#sample-react-application)
- [**9. Clean-up**](#9-clean-up)
- [**10. Conclusion**](#10-conclusion)

<br/>

## **1. <u>Purpose</u>**
<br/>

The purpose of this project was to use the code from my [Twitter Sampled Stream](https://github.com/TJ-Neal/twitter.sampled.stream) project and create a [Reddit API](https://www.reddit.com/dev/api) project version as a demonstration of my continued grasp of modern software development practices, architecture, principles, and ability to make use of API documentation. As with the first project, I wanted to extend myself and use the opportunity to go above and beyond so I added additional configurations and processes for monitoring Reddit posts from subreddits using any of the sorting functions, as well as, the ability to monitor not only posts from the time the applications starts, but from the entire paginated responsees that Reddit will supply. of course, there are some limitations. For instance, monitoring `/r/all` without limiting to start up posts could be difficult depending on the volume encountered or monitoring too many subreddits at a time can lead to too many requests to be able to retrieve all the posts within the `rate limits` allowed.
<br/><br/>

## **2. <u>Practices</u>**
<br/>

The following practices for modern software development were used as much as possible:
<br/>

- SOLID Principles
- Clean Code Architecture
- Microservice Architecture
- API Gateway
- Containers and orchestration
- .NET Minimal API
- Repository Pattern
- Event-Driven Architecture
- Structured Logging
- Unit Testing
<br/><br/>

## **3. <u>Concessions</u>**
<br/>
There are a number of concessions that I made due to the nature of the project and the scope that I decided to undertake to demonstrate the technologies.
<br/><br/>

1. Unit Tests - Though I did include some unit tests to demonstrate my grasp of the subject and use of `Xunit` and `Dependency Injection` in unit testing, they are by no means intended to be exhaustive, are not indicitive of `Test Driven Development` (or a lack of appreciation for the practice) and are admittedly quite anemic. My appologies. 
\* `Moq` was not used again due to recent events surrounding the package and authors.
2. Security - I would never normally include any sort of secrets in a repository; however, I did make one exception in this case in order to test and demonstrate `SSL` certificates in `Docker` containers. These are only `dev-certs` and the `GUID` used as a password conveys no risk.
<br/><br/>

## **4. <u>Primary Technologies and Packages</u>**
<br/>

- [Windows 11](https://www.microsoft.com/en-us/windows?wa=wsignin1.0) - Host/Development OS
- [WSL 2](https://learn.microsoft.com/en-us/windows/wsl/install) - Windows Subsystem for Lixus, used to host Linux containers on Windows OS
- [.NET Core 6](https://learn.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core?view=aspnetcore-6.0#recommended-learning-path) - Application framework
- [C# 10](https://learn.microsoft.com/en-us/dotnet/csharp/) - Primary programming language
- [Reddit API](https://www.reddit.com/dev/api) - Data source API
- [Docker](https://www.docker.com) - Container hosting platform
  - [Docker Compose](https://docs.docker.com/compose/) - Container orchestration utility
- [Kafka](https://kafka.apache.org/) - Message bus platform
  - [.NET Kafka Client](https://github.com/confluentinc/confluent-kafka-dotnet/) - Message bus platform client
- [MediatR](https://github.com/jbogard/MediatR) - Event-Driven pub/sub mediator
- [React](https://reactjs.org) - Front-end Web UI framework
  - [Typescript](https://www.typescriptlang.org) - Primary UI programming language
  - [MUI](https://mui.com) - Material Design UI component library
- [Serilog](https://serilog.net/) - .NET Logging implementations
- [Swagger/OpenAPI](https://swagger.io/) - API explorer/UI tool
- [XUnit](https://github.com/xunit/visualstudio.xunit) - Unit tesing framework for .NET
<br/><br/>

## **5. <u>Prerequisites</u>**
<br/>

### **5.1 Web API, Reddit Reader, Kafka, and Kafka Reader**
<br/>

The preferred way to run the APIs and Readers is to launch them in their Docker containers. This requires that you have `Visual Studio 2022` (or any `.NET 6` and `Docker` compatible IDE), and assuming you are working on a Windows OS, `Docker Desktop` installed and running with `WSL 2` (comes with `Docker Desktop`) but may need to be updated. Free versions are avaiable for each of these.

- [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
<br/><br/>

### **5.2 React Front-end UI**
<br/>

In order to run the React Front-end, Node and npm need to be installed. Both are freely available. I also highly recommend using Node Version Manager for this process, as it makes the process super simple.

- [Node Version Manager](https://github.com/nvm-sh/nvm)
  - [For Windows](https://github.com/coreybutler/nvm-windows)
- [NodeJS](https://nodejs.org/en/) (For reference only, should be installed through NVM)
<br/><br/>

## **6. <u>Structure</u>**
<br/>
Here is a very basic diagram of the three implementations:
<br/><br/>

<div style="max-width: 50%">

![Basic Design Diagram][basic_design_diagram]

</div>
<br/><br/>

## **7. <u>User Secrets and `SSL` Certs</u>**
<br/>

### **Reddit OAuth Keys**
<br/>

To access the Reddit API you will need to have valid OAuth keys placed into the user secrets of the `RedditReader` and/or `RedditClient.Tests` project.

Open `Manage User Secrets` for the project from the context menu:
<br/><br/>

<span style="padding-left: 3em">![][user_secrets_image]</span>
<br/><br/>

And use the following keys to edit the values to match your Twitter access tokens:
<br/><br/>

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
The `ClientId` and `ClientSecret` are retrieved from your Reddit developer account application page as outlined [here](https://github.com/reddit-archive/reddit/wiki/OAuth2). The `DeviceId` is just any unique identifier between 20 and 30 characters.

<br/><br/>

### **Self-signed (dev) `SSL` Certs**
<br/>

The `Docker` build process does include a `dev-cert SSL` certificate for each `API` to use for testing; however, this is for testing purposes only and would be replaced in production configurations.

The `dotnet` commandline tool provides a tool for creating self-signed certificates for developers to use on their local machines. I have included this functionality in the `Docker` file process so that the certificates are included in the images that are built. ___**This is not secure and uses a password that is plain text in all the docker files.**___ This is only for development and will not function on external systems. Additionally, because the certification is generated within the `Docker` image context, the certificate cannot be automatically trusted on the host machine, though it can be trusted manually or per browser session. 
<br/><br/>

```yaml
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=f9ea0a74-d7b3-49ae-b18b-25630bfbea10
.
.
.
RUN dotnet dev-certs https -ep /app/https/aspnetapp.pfx -p f9ea0a74-d7b3-49ae-b18b-25630bfbea10
```
<br/>

Alternatively, the `Docker` images could be made to point to a user's existing certificates, but I wanted the process to be as automated as possible with limited impact on the host machines.

When working with the application, you can ignore the `SSL` and use `HTTP` since I have not enforced `HTTPS` purposely. Alternatively, you can accept the certificate in your browser or add it to your trusted certificate store.
<br/><br/>

## **8. <u>Getting Started</u>**
<br/>

I have included all the necessary files and configurations for operating the application locally using `Docker Desktop` and `WSL` via the `Docker Compose` project. This excludes the sample React User Interface, which must be built and started manually. Instructions are included in the UI folder. ___**The yml and Dockerfiles are not configured for production - there is no real security implemented.**___ 
<br/><br/>

To get started, clone this repository to a location on your local machine.
<br/><br/>

### **Docker**
<br/>

The simplist way (i.e. "The Happy Path") to run the sample is to use the `Docker Compose` tool. This can be accessed either from the `Powershell` terminal or by using the `Developer Powershell` terminal built into Visual Studio.

From the terminal and within the `src` folder, use the following two commands to build and launch the `Docker` containers (they can be combined, but for ease of demonstration I have seperated them here).
<br/><br/>

```powershell
docker compose --profile all build --parallel --no-cache
docker compose --profile all up --detach
```
<br/>

You should see something similar to the following if your environment is properly set up with the latest `Docker Desktop`, `WSL 2`, `Visual Studio 2022`, and `.NET 6 SDK` as described in [**Primary Technologies and Packages**](#3-primary-technologies-and-packages). If you encounter an error accessing your local file system from within the containers, I found this [article](https://appuals.com/an-error-occurred-mounting-one-of-your-file-systems/) extremely helpful for addressing `WSL` as the cause.
<br/><br/><br/>

![][docker_desktop_image]
<br/><br/>

Assuming that everything is up and running now, you should see traffic between the containers as the `RedditReader` process pulls posts from the Reddit API and sends them to each of the producers (`Kafka` and `Simple` are enabled by default). This can be seen via the running logs within each respective container. Additionally, I have provided a number of ways to interact with the APIs to gain insights into their functioning.
<br/><br/>

### **Health Checks**

Each `API` has a very basic health check thread implemented that will respond with "Healthy" if it is responsive. They can be reached using the following URLs. (each `SSL` version requires accepting the `dev-cert` in your browser)

Simple API: http://localhost:4000/health, https://localhost:4001/health<br/>
Kafka Reddit API: http://localhost:4200/health, https://localhost:4201/health
<br/><br/>

### **Direct API Access**

Each API is also accessible via the direct links to each operation. For instance, the count of posts can be see in the Simple API via http://localhost:4000/api/posts/count
<br/><br/>

### **Swagger UI**

Most importantly, each API has Swagger UI enabled when running in `Docker`, giving an interface to interact with the API endpoints. Swagger UI can be reached at the following locations:

Simple API: http://localhost:4000/swagger<br/>
Kafka Reddit API: http://localhost:4200/swagger
<br/><br/>

### **Sample React Application**

Finally, there is a sample React JS UI application included to view all of the APIs together, showing the total number of posts processed by each and the top 10 posts and authors captured. See the `README` included in the UI folder for instructrion on building and running the application.
<br/><br/>

![][react_sample_page]
<br/><br/>

## **9. <u>Clean-up</u>**
<br/>

When the solution is run within `Docker Desktop` the persisted data and logs are store in the users temporary storage in a folder called `Containers`. On Windows, this is in the %TEMP% directory. When run outside of `Docker Desktop`, some files will be written to the `c:/Temp` directory. Simply delete these folders when you no longer need them or wish to reset the data.
<br/><br/>

## **10. <u>Conclusion</u>**
<br/>

I truly enjoyed creating this updated project for Reddit. Interacting with rate limits and semaphores is not an every day exercise for me. Personally, I am not a fan of the Reddit API and felt that some processes could certainly have been simplified had their API been designed differently. That in itself is a learning experience and tool for pro and con takeaways.

I also believe that this solution can handle a large volume of posts from Reddit; however, due to their rate limitations, exercising it to the maximum quickly reached their limits. That being said, I do think that when used reasonably this solution makes maximum use of the given rate limits and can handle multiple subreddits concurrently.

[user_secrets_image]: content/visual_studio_project_user_secrets.png
[docker_desktop_image]: content/Docker_Desktop_Running_All_Containers.png
[react_sample_page]: content/React_Sample_Page.png
[basic_design_diagram]: content/Basic_Design_Diagram.svg