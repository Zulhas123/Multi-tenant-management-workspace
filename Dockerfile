FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["MultiTenantWorkspace.sln", "./"]
COPY ["src/MultiTenantWorkspace.Web/MultiTenantWorkspace.Web.csproj", "src/MultiTenantWorkspace.Web/"]
COPY ["src/MultiTenantWorkspace.Application/MultiTenantWorkspace.Application.csproj", "src/MultiTenantWorkspace.Application/"]
COPY ["src/MultiTenantWorkspace.Domain/MultiTenantWorkspace.Domain.csproj", "src/MultiTenantWorkspace.Domain/"]
COPY ["src/MultiTenantWorkspace.Infrastructure/MultiTenantWorkspace.Infrastructure.csproj", "src/MultiTenantWorkspace.Infrastructure/"]
COPY ["src/MultiTenantWorkspace.Shared/MultiTenantWorkspace.Shared.csproj", "src/MultiTenantWorkspace.Shared/"]
COPY ["tests/MultiTenantWorkspace.Application.Tests/MultiTenantWorkspace.Application.Tests.csproj", "tests/MultiTenantWorkspace.Application.Tests/"]
COPY ["tests/MultiTenantWorkspace.Infrastructure.Tests/MultiTenantWorkspace.Infrastructure.Tests.csproj", "tests/MultiTenantWorkspace.Infrastructure.Tests/"]
RUN dotnet restore "MultiTenantWorkspace.sln"

COPY . .
RUN dotnet publish "src/MultiTenantWorkspace.Web/MultiTenantWorkspace.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "MultiTenantWorkspace.Web.dll"]
