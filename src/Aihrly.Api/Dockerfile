# Build stage — uses the full SDK to compile and publish
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files first so Docker can cache the restore layer
COPY Aihrly.sln .
COPY src/Aihrly.Api/Aihrly.Api.csproj ./src/Aihrly.Api/
RUN dotnet restore

# Copy everything else and publish in Release mode
COPY src/ ./src/
RUN dotnet publish src/Aihrly.Api/Aihrly.Api.csproj -c Release -o /publish --no-restore

# Runtime stage — smaller image, no SDK overhead
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Aihrly.Api.dll"]
