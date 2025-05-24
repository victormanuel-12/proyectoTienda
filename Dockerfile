# Imagen base con .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Establecer directorio de trabajo
WORKDIR /app

# Instalar dependencias mínimas necesarias para compilar Python
RUN apt-get update && apt-get install -y \
    wget build-essential libssl-dev zlib1g-dev \
    libbz2-dev libreadline-dev xz-utils \
    libffi-dev curl && \
    rm -rf /var/lib/apt/lists/*

# Descargar e instalar Python 3.13.3
RUN cd /usr/src && \
    wget https://www.python.org/ftp/python/3.13.3/Python-3.13.3.tgz && \
    tar xvf Python-3.13.3.tgz && \
    cd Python-3.13.3 && \
    ./configure --enable-optimizations && \
    make -j$(nproc) && \
    make altinstall

# Crear entorno virtual y activar pip
RUN python3.13 -m venv /opt/venv
ENV PATH="/opt/venv/bin:$PATH"

# Instalar substack-api
RUN pip install --upgrade pip && pip install substack-api==1.0.2

# Copiar el código
COPY . ./

# Restaurar y compilar .NET
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Copiar el script Python manualmente al output
COPY PythonScripts/substack_reader.py /app/out/PythonScripts/substack_reader.py

# Establecer ruta de trabajo
WORKDIR /app/out

# Ejecutar la aplicación
CMD ["dotnet", "proyectoTienda.dll"]

