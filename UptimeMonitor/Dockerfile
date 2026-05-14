FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["UptimeMonitor.csproj", "./"]
RUN dotnet restore "UptimeMonitor.csproj"

COPY . .
RUN dotnet publish "UptimeMonitor.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "UptimeMonitor.dll"]