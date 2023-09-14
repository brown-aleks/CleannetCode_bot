FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CleannetCodeBot/CleannetCodeBot.csproj", "CleannetCodeBot/"]
RUN dotnet restore "CleannetCodeBot/CleannetCodeBot.csproj"
COPY . .
WORKDIR "/src/CleannetCodeBot"
RUN dotnet build "CleannetCodeBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CleannetCodeBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleannetCodeBot.dll"]
