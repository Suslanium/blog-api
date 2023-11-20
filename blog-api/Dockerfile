FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["blog-api/blog-api.csproj", "blog-api/"]
RUN dotnet restore "blog-api/blog-api.csproj"
COPY . .
WORKDIR "/src/blog-api"
RUN dotnet build "blog-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "blog-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "blog-api.dll"]
