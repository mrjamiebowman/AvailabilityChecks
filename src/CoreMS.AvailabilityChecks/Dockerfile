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
COPY ["src/MrJB.AvailabilityChecks/MrJB.AvailabilityChecks.csproj", "src/MrJB.AvailabilityChecks/"]
RUN dotnet restore "./src/MrJB.AvailabilityChecks/MrJB.AvailabilityChecks.csproj"
COPY . .
WORKDIR "/src/src/MrJB.AvailabilityChecks"
RUN dotnet build "./MrJB.AvailabilityChecks.csproj" -c $BUILD_CONFIGURATION -o /app/build

# publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MrJB.AvailabilityChecks.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MrJB.AvailabilityChecks.dll"]