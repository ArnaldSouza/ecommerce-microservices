#!/bin/bash
set -e

echo "Criando imagens Docker..."

# Build das imagens
docker-compose build --no-cache

echo "Subindo serviços..."

# Subir apenas infraestrutura primeiro
docker-compose up -d sqlserver rabbitmq

echo "Aguardando inicialização da infraestrutura..."
sleep 30

# Subir os serviços
docker-compose up -d estoque-service
sleep 15

docker-compose up -d vendas-service  
sleep 15

docker-compose up -d api-gateway

echo "Todos os serviços foram iniciados!"

echo "Status dos serviços:"
docker-compose ps

echo "URLs disponíveis:"
echo "  - API Gateway: http://localhost:5000/swagger"
echo "  - Estoque Service: http://localhost:5002/swagger" 
echo "  - Vendas Service: http://localhost:5004/swagger"
echo "  - RabbitMQ Management: http://localhost:15672 (admin/admin123)"

echo "Para ver logs: docker-compose logs -f [service-name]"
echo "Para parar tudo: docker-compose down"