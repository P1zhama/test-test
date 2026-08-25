FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/Project.Domain/Project.Domain.csproj src/Project.Domain/
COPY src/Project.Application/Project.Application.csproj src/Project.Application/
COPY src/Project.Infrastructure/Project.Infrastructure.csproj src/Project.Infrastructure/
COPY src/Project.Api/Project.Api.csproj src/Project.Api/
RUN dotnet restore src/Project.Api/Project.Api.csproj

COPY src/ src/
RUN dotnet publish src/Project.Api/Project.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "Project.Api.dll"]
