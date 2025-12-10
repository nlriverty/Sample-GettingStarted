#!/bin/bash

echo "Deleting RabbitMQ user..."
docker exec rabbitmq rabbitmqctl delete_user guest
