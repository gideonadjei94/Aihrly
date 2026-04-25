# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY src/Aihrly.Api/Aihrly.Api.csproj ./src/Aihrly.Api/
RUN dotnet restore src/Aihrly.Api/Aihrly.Api.csproj

COPY src/ ./src/

# Removing --no-restore so publish re-uses the restored packages correctly
RUN dotnet publish src/Aihrly.Api/Aihrly.Api.csproj -c Release -o /publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Aihrly.Api.dll"]