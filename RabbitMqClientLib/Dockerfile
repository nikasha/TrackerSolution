FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /RabbitMqClientLib

RUN rabbitmq-plugins enable --offline rabbitmq_shovel rabbitmq_shovel_management

COPY ["RabbitMqClientLib.csproj", "./"]
RUN dotnet restore "RabbitMqClientLib.csproj"

COPY . .

RUN dotnet publish "RabbitMqClientLib.csproj" -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0

COPY --from=build /out /out
