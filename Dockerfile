# Imagen única para compilación y ejecución
FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

# Instalar Python, pip y virtualenv
RUN apt-get update && \
    apt-get install -y python3 python3-pip python3-venv && \
    python3 -m venv /opt/venv && \
    /opt/venv/bin/pip install --upgrade pip && \
    /opt/venv/bin/pip install substack-api==1.0.2 && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# Agregar el entorno virtual al PATH
ENV PATH="/opt/venv/bin:$PATH"

# Copiar el proyecto
COPY . ./

# Restaurar dependencias y publicar
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Copiar el script Python manualmente al output
COPY PythonScripts/substack_reader.py /app/out/PythonScripts/substack_reader.py

WORKDIR /app/out

# Render inyecta automáticamente el puerto
ENV APP_NET_CORE proyectoTienda.dll

# Ejecutar la app
CMD ["dotnet", "proyectoTienda.dll"]

