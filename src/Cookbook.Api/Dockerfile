FROM node:22 AS frontend
WORKDIR /app/frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
RUN npx ng build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["Cookbook.sln", "./"]
COPY ["src/Cookbook.Api/Cookbook.Api.csproj", "src/Cookbook.Api/"]
COPY ["src/Cookbook.Application/Cookbook.Application.csproj", "src/Cookbook.Application/"]
COPY ["src/Cookbook.Domain/Cookbook.Domain.csproj", "src/Cookbook.Domain/"]
COPY ["src/Cookbook.Infrastructure/Cookbook.Infrastructure.csproj", "src/Cookbook.Infrastructure/"]
RUN dotnet restore "src/Cookbook.Api/Cookbook.Api.csproj"

COPY . .
RUN dotnet publish "src/Cookbook.Api/Cookbook.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=frontend /app/src/Cookbook.Api/wwwroot ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Cookbook.Api.dll"]

