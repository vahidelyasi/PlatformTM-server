FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY PlatformTM.sln ./
COPY PlatformTM.API/PlatformTM.API.csproj PlatformTM.API/
COPY PlatformTM.Core/PlatformTM.Core.csproj PlatformTM.Core/
COPY PlatformTM.Services/PlatformTM.Services.csproj PlatformTM.Services/
COPY PlatformTM.Data/PlatformTM.Data.csproj PlatformTM.Data/
RUN dotnet restore -nowarn:msb3202,nu1503
COPY . .
WORKDIR /src/PlatformTM.API
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY dbinit /app/app_data
ENTRYPOINT ["dotnet", "PlatformTM.API.dll"]
