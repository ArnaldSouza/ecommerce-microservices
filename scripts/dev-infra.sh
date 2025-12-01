#!/bin/bash
set -e

echo "Iniciando infraestrutura para desenvolvimento..."

# Subir apenas SQL Server e RabbitMQ
docker-compose -f docker-compose. dev.yml up -d

echo "Aguardando inicialização..."
sleep 20

echo "Infraestrutura pronta!"
echo "Status:"
docker-compose -f docker-compose. dev.yml ps

echo "  Serviços disponíveis:"
echo "  - SQL Server: localhost:1433 (sa/SqlServer2022! )"
echo "  - RabbitMQ: localhost:5672 (admin/admin123)"
echo "  - RabbitMQ Management: http://localhost:15672"

echo "Agora você pode rodar os serviços . NET localmente:"
echo "  cd src/EstoqueService/EstoqueService && dotnet run"
echo "  cd src/VendasService/VendasService && dotnet run"