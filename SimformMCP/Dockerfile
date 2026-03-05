FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SimformMCP/SimformMCP.csproj SimformMCP/
RUN dotnet restore SimformMCP/SimformMCP.csproj

COPY . .
WORKDIR /src/SimformMCP
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SimformMCP.dll"]