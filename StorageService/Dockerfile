FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY RabbitMqClientLib/*.csproj ./RabbitMqClientLib/
RUN dotnet restore RabbitMqClientLib/RabbitMqClientLib.csproj

COPY StorageService/*.csproj ./StorageService/
RUN dotnet restore StorageService/StorageService.csproj

COPY RabbitMqClientLib/ ./RabbitMqClientLib/
COPY StorageService/ ./StorageService/

RUN dotnet publish StorageService/StorageService.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "StorageService.dll"]
