# Multi-stage build for NexaCRM WebServer
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and source
COPY global.json ./
COPY NexaCrmSolution.sln ./
COPY src ./src
COPY tests ./tests

# Restore dependencies with roll-forward support from global.json
RUN dotnet restore "NexaCrmSolution.sln"

# Publish the server host
RUN dotnet publish "src/NexaCRM.WebServer/NexaCRM.WebServer.csproj" \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

# Configure container runtime
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "NexaCRM.WebServer.dll"]
