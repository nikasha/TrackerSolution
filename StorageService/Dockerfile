FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copia csproj y restaura las dependencias
COPY RabbitMqClientLib/*.csproj ./RabbitMqClientLib/
RUN dotnet restore RabbitMqClientLib/RabbitMqClientLib.csproj

COPY StorageService/*.csproj ./StorageService/
RUN dotnet restore StorageService/StorageService.csproj

# Copia el resto de los archivos y directorios
COPY RabbitMqClientLib/ ./RabbitMqClientLib/
COPY StorageService/ ./StorageService/

# Publica la aplicación
RUN dotnet publish StorageService/StorageService.csproj -c Release -o out

# Etapa de imagen de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "StorageService.dll"]