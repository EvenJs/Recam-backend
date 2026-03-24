# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY Remp.API/Remp.API.csproj Remp.API/
COPY Remp.Service/Remp.Service.csproj Remp.Service/
COPY Remp.Repository/Remp.Repository.csproj Remp.Repository/
COPY Remp.DataAccess/Remp.DataAccess.csproj Remp.DataAccess/
COPY Remp.Models/Remp.Models.csproj Remp.Models/
COPY Remp.Common/Remp.Common.csproj Remp.Common/
COPY Remp.Tests/Remp.Tests.csproj Remp.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish Remp.API/Remp.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Remp.API.dll"]