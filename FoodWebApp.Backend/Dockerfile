FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FoodWebApp.Backend/FoodWebApp.Backend.csproj", "FoodWebApp.Backend/"]
RUN dotnet restore "FoodWebApp.Backend/FoodWebApp.Backend.csproj"
COPY . .
WORKDIR "/src/FoodWebApp.Backend"
RUN dotnet build "FoodWebApp.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FoodWebApp.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FoodWebApp.Backend.dll"]
