FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CleannetCode_bot/CleannetCode_bot.csproj", "CleannetCode_bot/"]
RUN dotnet restore "CleannetCode_bot/CleannetCode_bot.csproj"
COPY . .
WORKDIR "/src/CleannetCode_bot"
RUN dotnet build "CleannetCode_bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CleannetCode_bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleannetCode_bot.dll"]
