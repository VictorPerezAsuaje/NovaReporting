# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/playwright/dotnet:v1.53.0-noble AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Remove unused dependencies from playwright
RUN rm -rf \
    # Won't be using any other than Chromium
    /ms-playwright/firefox \
    /ms-playwright/webkit

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NovaPdf.Reporting.WebClient/NovaPdf.Reporting.WebClient.csproj", "NovaPdf.Reporting.WebClient/"]
COPY ["NovaPdf.Reporting.Core/NovaPdf.Reporting.Core.csproj", "NovaPdf.Reporting.Core/"]
COPY ["NovaPdf.Reporting.Razor/NovaPdf.Reporting.Razor.csproj", "NovaPdf.Reporting.Razor/"]
RUN dotnet restore "./NovaPdf.Reporting.WebClient/NovaPdf.Reporting.WebClient.csproj" /p:EnableDefaultContentItems=false
COPY . .
WORKDIR "/src/NovaPdf.Reporting.WebClient"
RUN dotnet build "./NovaPdf.Reporting.WebClient.csproj" -c $BUILD_CONFIGURATION -o /app/build 


# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NovaPdf.Reporting.WebClient.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false 

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NovaPdf.Reporting.WebClient.dll"]