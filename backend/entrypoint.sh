#!/bin/bash
set -e

SA_PASSWORD="Ivan123**"
SQLCMD=/opt/mssql-tools18/bin/sqlcmd

# Esperar a que SQL Server acepte conexiones

echo "Esperando a que SQL Server acepte conexiones..."
until $SQLCMD -S db -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; do
  echo "SQL Server no está listo todavía. Esperando..."
  sleep 3
done

echo "SQL Server está listo. Ejecutando script de inicialización..."
$SQLCMD -S db -U sa -P "$SA_PASSWORD" -C -i /app/script.sql

echo "Inicialización SQL completada. Iniciando la aplicación."
exec dotnet Scheduling.Api.dll