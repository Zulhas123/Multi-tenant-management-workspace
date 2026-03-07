FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ProjectManagementSaaS.sln", "./"]
COPY ["src/ProjectManagementSaaS.Api/ProjectManagementSaaS.Api.csproj", "src/ProjectManagementSaaS.Api/"]
COPY ["src/ProjectManagementSaaS.Application/ProjectManagementSaaS.Application.csproj", "src/ProjectManagementSaaS.Application/"]
COPY ["src/ProjectManagementSaaS.Domain/ProjectManagementSaaS.Domain.csproj", "src/ProjectManagementSaaS.Domain/"]
COPY ["src/ProjectManagementSaaS.Infrastructure/ProjectManagementSaaS.Infrastructure.csproj", "src/ProjectManagementSaaS.Infrastructure/"]
COPY ["tests/ProjectManagementSaaS.Application.Tests/ProjectManagementSaaS.Application.Tests.csproj", "tests/ProjectManagementSaaS.Application.Tests/"]
RUN dotnet restore "ProjectManagementSaaS.sln"

COPY . .
RUN dotnet publish "src/ProjectManagementSaaS.Api/ProjectManagementSaaS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ProjectManagementSaaS.Api.dll"]
