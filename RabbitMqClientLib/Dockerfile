FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /RabbitMqClientLib

# Copia el archivo csproj y restaura las dependencias
COPY ["RabbitMqClientLib.csproj", "./"]
RUN dotnet restore "RabbitMqClientLib.csproj"

# Copia el resto de los archivos del proyecto
COPY . .

# Publica la librería a un directorio de salida
RUN dotnet publish "RabbitMqClientLib.csproj" -c Release -o /out

# La imagen final, basada en el runtime de .NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Copia los archivos publicados desde el build al directorio de trabajo
COPY --from=build /out /out
