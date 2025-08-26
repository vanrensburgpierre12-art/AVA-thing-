FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SimDevicePlatform.Api/SimDevicePlatform.Api.csproj", "SimDevicePlatform.Api/"]
COPY ["SimDevicePlatform.Core/SimDevicePlatform.Core.csproj", "SimDevicePlatform.Core/"]
COPY ["SimDevicePlatform.Infrastructure/SimDevicePlatform.Infrastructure.csproj", "SimDevicePlatform.Infrastructure/"]
COPY ["SimDevicePlatform.Processing/SimDevicePlatform.Processing.csproj", "SimDevicePlatform.Processing/"]
RUN dotnet restore "SimDevicePlatform.Api/SimDevicePlatform.Api.csproj"
COPY . .
WORKDIR "/src/SimDevicePlatform.Api"
RUN dotnet build "SimDevicePlatform.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimDevicePlatform.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimDevicePlatform.Api.dll"]