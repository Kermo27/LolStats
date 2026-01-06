# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first for better caching
COPY ["Client/LolStatsTracker.csproj", "Client/"]
COPY ["Shared/LolStatsTracker.Shared.csproj", "Shared/"]
RUN dotnet restore "Client/LolStatsTracker.csproj"

# Copy source code
COPY Client/ Client/
COPY Shared/ Shared/

# Build and publish
WORKDIR "/src/Client"
RUN dotnet publish "LolStatsTracker.csproj" -c Release -o /app/publish

# Runtime stage - use nginx to serve static files
FROM nginx:alpine AS runtime

# Copy published Blazor app to nginx
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

# Copy custom nginx config for SPA routing
COPY Client/nginx.conf /etc/nginx/nginx.conf

# Expose port 80
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
