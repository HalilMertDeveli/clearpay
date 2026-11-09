# Build context = repository root:
#   docker build -f src/ClearPay.Web/Dockerfile -t clearpay-web .
# Used by docker-compose.services.yml. Azure App Service deploys with `dotnet publish` (docs/DEPLOY.md), not this image.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/ClearPay.Domain/ClearPay.Domain.csproj src/ClearPay.Domain/
COPY src/ClearPay.Application/ClearPay.Application.csproj src/ClearPay.Application/
COPY src/ClearPay.Contracts/ClearPay.Contracts.csproj src/ClearPay.Contracts/
COPY src/ClearPay.Messaging/ClearPay.Messaging.csproj src/ClearPay.Messaging/
COPY src/ClearPay.Infrastructure/ClearPay.Infrastructure.csproj src/ClearPay.Infrastructure/
COPY src/ClearPay.Web/ClearPay.Web.csproj src/ClearPay.Web/
RUN dotnet restore src/ClearPay.Web/ClearPay.Web.csproj
COPY src/ src/
RUN dotnet publish src/ClearPay.Web/ClearPay.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ClearPay.Web.dll"]
