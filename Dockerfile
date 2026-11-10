FROM node:22-alpine AS frontend-build
WORKDIR /src/Vezza.Web
COPY Vezza.Web/package.json Vezza.Web/package-lock.json ./
RUN npm ci
COPY Vezza.Web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY Vezza.sln ./
COPY Vezza.Api/Vezza.Api.csproj Vezza.Api/
COPY Vezza.Domain/Vezza.Domain.csproj Vezza.Domain/
COPY Vezza.Infra/Vezza.Infra.csproj Vezza.Infra/
COPY Vezza.Shared/Vezza.Shared.csproj Vezza.Shared/
RUN dotnet restore Vezza.Api/Vezza.Api.csproj
COPY . ./
RUN dotnet publish Vezza.Api/Vezza.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /src/Vezza.Web/dist ./wwwroot
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "Vezza.Api.dll"]
