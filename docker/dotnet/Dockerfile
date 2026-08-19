# Multi-stage .NET 8 Dockerfile for BCSMS Backend API
# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for optimized layer caching
COPY backend/dotnet/src/BCSMS.Domain/BCSMS.Domain.csproj backend/dotnet/src/BCSMS.Domain/
COPY backend/dotnet/src/BCSMS.Application/BCSMS.Application.csproj backend/dotnet/src/BCSMS.Application/
COPY backend/dotnet/src/BCSMS.Infrastructure/BCSMS.Infrastructure.csproj backend/dotnet/src/BCSMS.Infrastructure/
COPY backend/dotnet/src/BCSMS.API/BCSMS.API.csproj backend/dotnet/src/BCSMS.API/

# Restore dependencies
RUN dotnet restore backend/dotnet/src/BCSMS.API/BCSMS.API.csproj

# Copy source code
COPY backend/dotnet/src/ backend/dotnet/src/

# Build and publish release binaries
WORKDIR /src/backend/dotnet/src/BCSMS.API
RUN dotnet publish BCSMS.API.csproj -c Release -o /app/publish --no-restore

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published binaries from build stage
COPY --from=build /app/publish .

# Expose HTTP port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BCSMS.API.dll"]
