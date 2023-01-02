
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Auth.Api/Auth.Api.csproj", "src/Auth.Api/"]
COPY ["src/Auth.Data/Auth.Data.csproj", "src/Auth.Data/"]
COPY ["src/Auth.Services/Auth.Services.csproj", "src/Auth.Services/"]
RUN dotnet restore "src/Auth.Api/Auth.Api.csproj"
COPY . .
WORKDIR "/src/src/Auth.Api"
RUN dotnet build "Auth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Auth.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.Api.dll"]