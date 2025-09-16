output "id" {
  description = "The ID of the Redis Cache"
  value       = azurerm_redis_cache.redis.id
}

output "name" {
  description = "The name of the Redis Cache"
  value       = azurerm_redis_cache.redis.name
}

output "hostname" {
  description = "The hostname of the Redis Cache"
  value       = azurerm_redis_cache.redis.hostname
}

output "ssl_port" {
  description = "The SSL port of the Redis Cache"
  value       = azurerm_redis_cache.redis.ssl_port
}

output "non_ssl_port" {
  description = "The non-SSL port of the Redis Cache (if enabled)"
  value       = var.enable_non_ssl_port ? azurerm_redis_cache.redis.port : null
}

output "primary_access_key" {
  description = "The primary access key of the Redis Cache"
  value       = azurerm_redis_cache.redis.primary_access_key
  sensitive   = true
}

output "secondary_access_key" {
  description = "The secondary access key of the Redis Cache"
  value       = azurerm_redis_cache.redis.secondary_access_key
  sensitive   = true
}

output "primary_connection_string" {
  description = "The primary connection string to the Redis Cache"
  value       = "${azurerm_redis_cache.redis.hostname}:${azurerm_redis_cache.redis.ssl_port},password=${azurerm_redis_cache.redis.primary_access_key},ssl=True,abortConnect=False"
  sensitive   = true
}

output "secondary_connection_string" {
  description = "The secondary connection string to the Redis Cache"
  value       = "${azurerm_redis_cache.redis.hostname}:${azurerm_redis_cache.redis.ssl_port},password=${azurerm_redis_cache.redis.secondary_access_key},ssl=True,abortConnect=False"
  sensitive   = true
}

output "resource_group_name" {
  description = "The name of the resource group containing the Redis Cache"
  value       = azurerm_resource_group.redis.name
}

output "resource_group_id" {
  description = "The ID of the resource group containing the Redis Cache"
  value       = azurerm_resource_group.redis.id
}