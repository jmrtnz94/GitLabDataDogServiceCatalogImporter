FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DataDogServiceCatalog/DataDogServiceCatalog.csproj", "DataDogServiceCatalog/"]
RUN dotnet restore "DataDogServiceCatalog/DataDogServiceCatalog.csproj"
COPY . .
WORKDIR "/src/DataDogServiceCatalog"
RUN dotnet build "DataDogServiceCatalog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataDogServiceCatalog.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataDogServiceCatalog.dll"]
