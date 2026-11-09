# Build context = repository root:
#   docker build -f src/ClearPay.Reconciliation/Dockerfile -t clearpay-reconciliation .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/ClearPay.Contracts/ClearPay.Contracts.csproj src/ClearPay.Contracts/
COPY src/ClearPay.Messaging/ClearPay.Messaging.csproj src/ClearPay.Messaging/
COPY src/ClearPay.Reconciliation/ClearPay.Reconciliation.csproj src/ClearPay.Reconciliation/
RUN dotnet restore src/ClearPay.Reconciliation/ClearPay.Reconciliation.csproj
COPY src/ClearPay.Contracts/ src/ClearPay.Contracts/
COPY src/ClearPay.Messaging/ src/ClearPay.Messaging/
COPY src/ClearPay.Reconciliation/ src/ClearPay.Reconciliation/
RUN dotnet publish src/ClearPay.Reconciliation/ClearPay.Reconciliation.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
# Settlement CSV files (mock bank drops) live outside the app folder so the non-root user can write them.
RUN mkdir -p /data/settlement-files && chown -R $APP_UID /data
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080 \
    Reconciliation__SettlementDirectory=/data/settlement-files
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ClearPay.Reconciliation.dll"]
