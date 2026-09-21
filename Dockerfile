FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    Dexus__DisableHttpsRedirection=true \
    DOTNET_EnableDiagnostics=0 \
    COMPlus_EnableDiagnostics=0

RUN mkdir -p \
        /app/RequestManager/Manifest \
        /home/app/.aspnet/DataProtection-Keys \
    && chown -R app:app \
        /app/RequestManager \
        /home/app/.aspnet \
    && chmod 700 \
        /app/RequestManager/Manifest \
        /home/app/.aspnet/DataProtection-Keys


VOLUME ["/app/RequestManager/Manifest", "/home/app/.aspnet/DataProtection-Keys"]

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Dexus/Dexus/Dexus.csproj
RUN dotnet publish Dexus/Dexus/Dexus.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Dexus.dll"]
