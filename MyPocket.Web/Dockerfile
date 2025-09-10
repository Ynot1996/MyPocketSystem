# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images efficiently.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyPocket.Web/MyPocket.Web.csproj", "MyPocket.Web/"]
COPY ["MyPocket.Core/MyPocket.Core.csproj", "MyPocket.Core/"]
COPY ["MyPocket.Services/MyPocket.Services.csproj", "MyPocket.Services/"]
COPY ["MyPocket.DataAccess/MyPocket.DataAccess.csproj", "MyPocket.DataAccess/"]
COPY ["MyPocket.Shared/MyPocket.Shared.csproj", "MyPocket.Shared/"]
RUN dotnet restore "MyPocket.Web/MyPocket.Web.csproj"
COPY . .
WORKDIR "/src/MyPocket.Web"
RUN dotnet publish "MyPocket.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyPocket.Web.dll"]
