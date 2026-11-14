ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0
ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled

FROM ${SDK_IMAGE} AS build
WORKDIR /source
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/Unwalked.Domain/Unwalked.Domain.csproj src/Unwalked.Domain/
COPY src/Unwalked.Application/Unwalked.Application.csproj src/Unwalked.Application/
COPY src/Unwalked.Infrastructure/Unwalked.Infrastructure.csproj src/Unwalked.Infrastructure/
COPY src/Unwalked.Api/Unwalked.Api.csproj src/Unwalked.Api/
COPY src/Unwalked.Tools/Unwalked.Tools.csproj src/Unwalked.Tools/
RUN dotnet restore src/Unwalked.Api/Unwalked.Api.csproj \
 && dotnet restore src/Unwalked.Tools/Unwalked.Tools.csproj
COPY src/ src/
RUN dotnet publish src/Unwalked.Api/Unwalked.Api.csproj -c Release --no-restore -o /out/api /p:UseAppHost=false \
 && dotnet publish src/Unwalked.Tools/Unwalked.Tools.csproj -c Release --no-restore -o /out/tools /p:UseAppHost=false

FROM ${RUNTIME_IMAGE} AS tools
WORKDIR /app
COPY --from=build /out/tools .
USER $APP_UID
ENTRYPOINT ["dotnet", "Unwalked.Tools.dll"]

FROM ${RUNTIME_IMAGE} AS api
WORKDIR /app
COPY --from=build /out/api .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Unwalked.Api.dll"]
