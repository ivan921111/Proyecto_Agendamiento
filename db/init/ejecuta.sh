#!/bin/bash

echo "Esperando a SQL Server..."
sleep 30

echo "Ejecutando script SQL..."

/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Ivan123** -C -i /docker-entrypoint-initdb.d/script.sql 