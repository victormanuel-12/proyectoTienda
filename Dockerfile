FROM mcr.microsoft.com/dotnet/sdk:8.0

# Instalar dependencias necesarias para compilar Python
RUN apt-get update && apt-get install -y \
    wget build-essential libssl-dev zlib1g-dev libbz2-dev \
    libreadline-dev libsqlite3-dev curl llvm libncursesw5-dev xz-utils \
    tk-dev libxml2-dev libxmlsec1-dev libffi-dev liblzma-dev git && \
    rm -rf /var/lib/apt/lists/*

# Descargar y compilar Python 3.13.3
RUN cd /usr/src && \
    wget https://www.python.org/ftp/python/3.13.3/Python-3.13.3.tgz && \
    tar xvf Python-3.13.3.tgz && \
    cd Python-3.13.3 && \
    ./configure --enable-optimizations && \
    make -j $(nproc) && \
    make altinstall

# Crear entorno virtual con Python 3.13 y activar pip
RUN python3.13 -m venv /opt/venv
ENV PATH="/opt/venv/bin:$PATH"

# Instalar substack-api versión compatible
RUN pip install --upgrade pip && pip install substack-api==1.0.2

# Establecer directorio de trabajo
WORKDIR /app

# Copiar todo el proyecto
COPY . .

# Restaurar y publicar la aplicación .NET
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Cambiar a directorio de publicación
WORKDIR /app/out

# Iniciar la app
CMD ["dotnet", "proyectoTienda.dll"]
