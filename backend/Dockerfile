FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./Scheduling.Api/Scheduling.Api.csproj ./Scheduling.Api/
RUN dotnet restore ./Scheduling.Api/Scheduling.Api.csproj

COPY . .
RUN dotnet publish ./Scheduling.Api/Scheduling.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl gnupg2 apt-transport-https ca-certificates unixodbc-dev netcat-openbsd \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > /usr/share/keyrings/microsoft.gpg \
    && echo "deb [signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y --no-install-recommends msodbcsql18 mssql-tools18 \
    && ln -s /opt/mssql-tools18/bin/sqlcmd /usr/bin/sqlcmd \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY ./script.sql .
COPY ./entrypoint.sh .
RUN chmod +x ./entrypoint.sh

EXPOSE 80
ENTRYPOINT ["./entrypoint.sh"]