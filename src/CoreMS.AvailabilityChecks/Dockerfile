# base
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_HTTPS_PORTS=8081
ENV ASPNETCORE_URLS "http://+:8080"

# build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CoreMS.AvailabilityChecks/CoreMS.AvailabilityChecks.csproj", "src/CoreMS.AvailabilityChecks/"]
RUN dotnet restore "./src/CoreMS.AvailabilityChecks/CoreMS.AvailabilityChecks.csproj"
COPY . .
WORKDIR "/src/src/CoreMS.AvailabilityChecks"
RUN dotnet build "./CoreMS.AvailabilityChecks.csproj" -c $BUILD_CONFIGURATION -o /app/build

# publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoreMS.AvailabilityChecks.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreMS.AvailabilityChecks.dll"]