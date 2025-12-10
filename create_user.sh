#!/bin/bash

echo "Creating RabbitMQ user..."
docker exec rabbitmq rabbitmqctl add_user guest guest
docker exec rabbitmq rabbitmqctl set_user_tags guest administrator
docker exec rabbitmq rabbitmqctl set_permissions guest ".*" ".*" ".*"
