FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app
COPY blog-api/*.csproj ./
RUN dotnet restore

COPY blog-api/. ./
RUN dotnet publish -c Release -o build

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "blog-api.dll"]