# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy Directory.Build.props first (build optimization)
COPY services/games/Directory.Build.props ./

# Copy shared common projects (required for ProjectReferences)
COPY ["shared/common/src/TC.CloudGames.Messaging/TC.CloudGames.Messaging.csproj", "shared/common/src/TC.CloudGames.Messaging/"]
COPY ["shared/common/src/TC.CloudGames.SharedKernel/TC.CloudGames.SharedKernel.csproj", "shared/common/src/TC.CloudGames.SharedKernel/"]
COPY ["shared/common/src/TC.CloudGames.Contracts/TC.CloudGames.Contracts.csproj", "shared/common/src/TC.CloudGames.Contracts/"]

# Copy project files maintaining directory structure (required for ProjectReferences)
COPY ["services/games/src/Adapters/Inbound/TC.CloudGames.Games.Api/TC.CloudGames.Games.Api.csproj", "services/games/src/Adapters/Inbound/TC.CloudGames.Games.Api/"]
COPY ["services/games/src/Core/TC.CloudGames.Games.Application/TC.CloudGames.Games.Application.csproj", "services/games/src/Core/TC.CloudGames.Games.Application/"]
COPY ["services/games/src/Core/TC.CloudGames.Games.Domain/TC.CloudGames.Games.Domain.csproj", "services/games/src/Core/TC.CloudGames.Games.Domain/"]
COPY ["services/games/src/Adapters/Outbound/TC.CloudGames.Games.Infrastructure/TC.CloudGames.Games.Infrastructure.csproj", "services/games/src/Adapters/Outbound/TC.CloudGames.Games.Infrastructure/"]

# Restore packages
RUN dotnet restore "services/games/src/Adapters/Inbound/TC.CloudGames.Games.Api/TC.CloudGames.Games.Api.csproj"

# Copy source code maintaining structure
COPY . .

# Build the application
WORKDIR "/src/services/games/src/Adapters/Inbound/TC.CloudGames.Games.Api"
RUN dotnet build "TC.CloudGames.Games.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TC.CloudGames.Games.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TC.CloudGames.Games.Api.dll"]