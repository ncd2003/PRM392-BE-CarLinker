#!/bin/bash

# Quick Database Query Helper
# Usage: ./query-db.sh "SELECT * FROM Garage"

CONTAINER_NAME="carlinker-mssql"
SA_PASSWORD="CarLinker@2025"
DATABASE="CarLinker"

if [ -z "$1" ]; then
    echo "Usage: $0 \"SQL_QUERY\""
    echo "Example: $0 \"SELECT * FROM Garage\""
    exit 1
fi

docker exec -i $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U SA \
    -P "$SA_PASSWORD" \
    -C \
    -d $DATABASE \
    -Q "$1"
