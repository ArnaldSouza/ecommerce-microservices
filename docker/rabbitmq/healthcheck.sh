#!/bin/bash
set -e

# Verificar se RabbitMQ está respondendo
rabbitmq-diagnostics -q ping
rabbitmq-diagnostics -q check_running
rabbitmq-diagnostics -q check_local_alarms

echo "RabbitMQ está saudável"