FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "Directory.Packages.props", "./"]
COPY ["src/SchoolAccount.CollectNotifications/SchoolAccount.CollectNotifications.csproj", "src/SchoolAccount.CollectNotifications/"]
RUN dotnet restore "src/SchoolAccount.CollectNotifications/SchoolAccount.CollectNotifications.csproj"
COPY . .
WORKDIR "/src/src/SchoolAccount.CollectNotifications"
RUN dotnet build "./SchoolAccount.CollectNotifications.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SchoolAccount.CollectNotifications.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchoolAccount.CollectNotifications.dll"]
