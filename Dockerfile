# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY src/FortressApi/FortressApi.csproj src/FortressApi/
RUN dotnet restore src/FortressApi/FortressApi.csproj
COPY . .
RUN dotnet publish src/FortressApi/FortressApi.csproj -c Release -o /out /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /out .
ENTRYPOINT ["dotnet", "FortressApi.dll"]
