# 1. Start with the official .NET SDK image to BUILD your app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish
# 2. Start fresh with the much smaller .NET Runtime image to RUN your app
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FirstApi.dll"]