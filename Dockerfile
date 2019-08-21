FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Tringo.WebApp/Tringo.WebApp.csproj", "Tringo.WebApp/"]
RUN dotnet restore "Tringo.WebApp/Tringo.WebApp.csproj"
COPY . .
RUN dotnet restore

ENV TestArtifacts=/app/FlightsSearchService.Tests/TestArtifacts
RUN dotnet test "Tringo.WebApp.Tests/Tringo.WebApp.Tests.csproj"

WORKDIR "/src/Tringo.WebApp"
RUN dotnet build "Tringo.WebApp.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Tringo.WebApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Tringo.WebApp.dll"]
