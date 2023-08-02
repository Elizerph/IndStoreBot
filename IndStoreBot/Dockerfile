#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["IndStoreBot/IndStoreBot.csproj", "IndStoreBot/"]
RUN dotnet restore "IndStoreBot/IndStoreBot.csproj"
COPY . .
WORKDIR "/src/IndStoreBot"
RUN dotnet build "IndStoreBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IndStoreBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IndStoreBot.dll"]