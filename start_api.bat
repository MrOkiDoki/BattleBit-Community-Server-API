git fetch

git pull

dotnet restore "CommunityServerAPI.csproj"

dotnet build "CommunityServerAPI.csproj" --no-restore -c Release -o /app


dotnet publish "CommunityServerAPI.csproj" -c Release -o /app

dotnet run