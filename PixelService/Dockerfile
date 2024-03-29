FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY RabbitMqClientLib/*.csproj ./RabbitMqClientLib/
RUN dotnet restore RabbitMqClientLib/RabbitMqClientLib.csproj

COPY PixelService/*.csproj ./PixelService/
RUN dotnet restore PixelService/PixelService.csproj

COPY RabbitMqClientLib/ ./RabbitMqClientLib/
COPY PixelService ./PixelService/
RUN dotnet publish PixelService/PixelService.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "PixelService.dll"]
