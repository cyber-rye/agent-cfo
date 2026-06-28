FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files and restore
COPY src/AgentCfo.Core/AgentCfo.Core.csproj src/AgentCfo.Core/
COPY src/AgentCfo.Application/AgentCfo.Application.csproj src/AgentCfo.Application/
COPY src/AgentCfo.Infrastructure/AgentCfo.Infrastructure.csproj src/AgentCfo.Infrastructure/
COPY src/AgentCfo.Api/AgentCfo.Api.csproj src/AgentCfo.Api/
RUN dotnet restore src/AgentCfo.Api/AgentCfo.Api.csproj

# Copy source and build
COPY src/ src/
RUN dotnet publish src/AgentCfo.Api/AgentCfo.Api.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AgentCfo.Api.dll"]
