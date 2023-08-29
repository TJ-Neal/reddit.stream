#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Infrastructure/Neal.Reddit.Infrastructure.Reader/Neal.Reddit.Infrastructure.Reader.csproj", "Infrastructure/Neal.Reddit.Infrastructure.Reader/"]
RUN dotnet restore "Infrastructure/Neal.Reddit.Infrastructure.Reader/Neal.Reddit.Infrastructure.Reader.csproj"
COPY . .
WORKDIR "/src/Infrastructure/Neal.Reddit.Infrastructure.Reader"
RUN dotnet build "Neal.Reddit.Infrastructure.Reader.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Neal.Reddit.Infrastructure.Reader.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Neal.Reddit.Infrastructure.Reader.dll"]