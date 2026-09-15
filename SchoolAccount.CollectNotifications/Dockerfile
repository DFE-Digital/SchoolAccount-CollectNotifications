FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SchoolAccount.CollectionNotifications/SchoolAccount.CollectionNotifications.csproj", "SchoolAccount.CollectionNotifications/"]
RUN dotnet restore "SchoolAccount.CollectionNotifications/SchoolAccount.CollectionNotifications.csproj"
COPY . .
WORKDIR "/src/SchoolAccount.CollectionNotifications"
RUN dotnet build "./SchoolAccount.CollectionNotifications.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SchoolAccount.CollectionNotifications.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchoolAccount.CollectionNotifications.dll"]
