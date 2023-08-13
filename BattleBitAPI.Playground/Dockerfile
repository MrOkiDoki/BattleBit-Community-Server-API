FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CommunityServerAPI.Playground/CommunityServerAPI.Playground.csproj", "CommunityServerAPI.Playground/"]
RUN dotnet restore "CommunityServerAPI.Playground/CommunityServerAPI.Playground.csproj"
COPY . .
WORKDIR "/src/CommunityServerAPI.Playground"
RUN dotnet build "CommunityServerAPI.Playground.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CommunityServerAPI.Playground.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommunityServerAPI.Playground.dll"]
