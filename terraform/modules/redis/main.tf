# Redis Cache Module
resource "azurerm_resource_group" "redis" {
  name     = var.resource_group_name
  location = var.location
  tags     = var.tags
}

resource "azurerm_redis_cache" "redis" {
  name                = var.redis_name
  location            = azurerm_resource_group.redis.location
  resource_group_name = azurerm_resource_group.redis.name
  capacity            = var.capacity
  family              = var.family
  sku_name            = var.sku_name
  enable_non_ssl_port = var.enable_non_ssl_port
  minimum_tls_version = var.minimum_tls_version
  
  redis_configuration {
    maxmemory_reserved              = var.maxmemory_reserved
    maxmemory_delta                 = var.maxmemory_delta
    maxmemory_policy                = var.maxmemory_policy
    maxfragmentationmemory_reserved = var.maxfragmentationmemory_reserved
  }

  # Premium tier specific configurations
  dynamic "patch_schedule" {
    for_each = var.sku_name == "Premium" && length(var.patch_schedule) > 0 ? var.patch_schedule : []
    content {
      day_of_week    = patch_schedule.value.day_of_week
      start_hour_utc = patch_schedule.value.start_hour_utc
    }
  }

  dynamic "redis_configuration" {
    for_each = var.sku_name == "Premium" ? [1] : []
    content {
      aof_backup_enabled              = var.aof_backup_enabled
      aof_storage_connection_string_0 = var.aof_storage_connection_string_0
      aof_storage_connection_string_1 = var.aof_storage_connection_string_1
    }
  }

  # Private endpoint configuration
  dynamic "private_endpoint" {
    for_each = var.private_endpoint_enabled && var.sku_name == "Premium" ? [1] : []
    content {
      subnet_id = var.private_endpoint_subnet_id
    }
  }

  tags = var.tags
}

# Create firewall rules if specified
resource "azurerm_redis_firewall_rule" "firewall_rules" {
  for_each = var.firewall_rules

  name                = each.key
  redis_cache_name    = azurerm_redis_cache.redis.name
  resource_group_name = azurerm_resource_group.redis.name
  start_ip            = each.value.start_ip
  end_ip              = each.value.end_ip
}

output "id" {
  value = azurerm_redis_cache.redis.id
}

output "hostname" {
  value = azurerm_redis_cache.redis.hostname
}

output "ssl_port" {
  value = azurerm_redis_cache.redis.ssl_port
}

output "non_ssl_port" {
  value = azurerm_redis_cache.redis.enable_non_ssl_port ? azurerm_redis_cache.redis.port : null
}

output "primary_access_key" {
  value     = azurerm_redis_cache.redis.primary_access_key
  sensitive = true
}

output "secondary_access_key" {
  value     = azurerm_redis_cache.redis.secondary_access_key
  sensitive = true
}

output "connection_string" {
  value     = "${azurerm_redis_cache.redis.hostname}:${azurerm_redis_cache.redis.ssl_port},password=${azurerm_redis_cache.redis.primary_access_key}"
  sensitive = true
}