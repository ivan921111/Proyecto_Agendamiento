FROM mcr.microsoft.com/mssql/server:2022-latest
LABEL author="iguzman"

# Configurar el entorno para que no sea interactivo, evitando que apt se bloquee
ENV DEBIAN_FRONTEND=noninteractive

# Instalar herramientas de línea de comandos de SQL Server
RUN apt-get update && apt-get install -y curl gnupg apt-transport-https && \
    curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /etc/apt/trusted.gpg.d/microsoft.gpg && \
    echo "deb [arch=amd64] https://packages.microsoft.com/ubuntu/22.04/prod jammy main" > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && ACCEPT_EULA=Y apt-get install -y mssql-tools18 && \
    # Limpiar la caché de apt para reducir el tamaño de la imagen
    apt-get clean && rm -rf /var/lib/apt/lists/*

COPY ./init/entrypoint.sh /
RUN chmod +x /entrypoint.sh
ENTRYPOINT ["/bin/bash", "/entrypoint.sh"]