#!/bin/bash

echo "Limpando containers e volumes..."

# Parar todos os serviços
docker-compose down

# Remover volumes
read -p "Remover volumes (todos os dados serão perdidos)?  [y/N]: " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    docker-compose down -v
    docker volume prune -f
    echo "Volumes removidos!"
fi

# Remover imagens não utilizadas
docker image prune -f

echo "Limpeza concluída!"