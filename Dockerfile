# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first for better caching
COPY ["Server/LolStatsTracker.API.csproj", "Server/"]
COPY ["Shared/LolStatsTracker.Shared.csproj", "Shared/"]
RUN dotnet restore "Server/LolStatsTracker.API.csproj"

# Copy source code
COPY Server/ Server/
COPY Shared/ Shared/

# Build and publish
WORKDIR "/src/Server"
RUN dotnet build "LolStatsTracker.API.csproj" -c Release -o /app/build
RUN dotnet publish "LolStatsTracker.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" --uid 1000 appuser

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "LolStatsTracker.API.dll"]
