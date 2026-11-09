# Build context = repository root:
#   docker build -f src/ClearPay.Notifications/Dockerfile -t clearpay-notifications .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Project files first so `dotnet restore` is cached until a csproj changes.
COPY src/ClearPay.Contracts/ClearPay.Contracts.csproj src/ClearPay.Contracts/
COPY src/ClearPay.Messaging/ClearPay.Messaging.csproj src/ClearPay.Messaging/
COPY src/ClearPay.Notifications/ClearPay.Notifications.csproj src/ClearPay.Notifications/
RUN dotnet restore src/ClearPay.Notifications/ClearPay.Notifications.csproj
COPY src/ClearPay.Contracts/ src/ClearPay.Contracts/
COPY src/ClearPay.Messaging/ src/ClearPay.Messaging/
COPY src/ClearPay.Notifications/ src/ClearPay.Notifications/
RUN dotnet publish src/ClearPay.Notifications/ClearPay.Notifications.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["dotnet", "ClearPay.Notifications.dll"]
