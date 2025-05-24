# Etapa 1: Compilación .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiar y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto de archivos y publicar en Release
COPY . ./
RUN dotnet publish -c Release -o out

# Etapa 2: Imagen final con .NET runtime + Python
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Instalar Python 3 y pip
RUN apt-get update && \
    apt-get install -y python3 python3-pip && \
    pip3 install substack-api && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copiar archivos publicados por .NET
COPY --from=build-env /app/out ./

# Copiar script de Python
COPY PythonScripts/substack_reader.py ./PythonScripts/substack_reader.py

# Render inyecta automáticamente la variable $PORT
ENV APP_NET_CORE proyectoTienda.dll
CMD ASPNETCORE_URLS=http://*:$PORT dotnet $APP_NET_CORE
