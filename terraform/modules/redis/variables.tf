variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
}

variable "redis_name" {
  description = "The name of the Redis Cache"
  type        = string
}

variable "capacity" {
  description = "The size of the Redis Cache"
  type        = number
  default     = 1
  validation {
    condition     = contains([0, 1, 2, 3, 4, 5, 6], var.capacity)
    error_message = "The capacity must be between 0 and 6."
  }
}

variable "family" {
  description = "The SKU family to use"
  type        = string
  default     = "C"
  validation {
    condition     = contains(["C", "P"], var.family)
    error_message = "The family must be either C or P."
  }
}

variable "sku_name" {
  description = "The SKU of Redis to use"
  type        = string
  default     = "Standard"
  validation {
    condition     = contains(["Basic", "Standard", "Premium"], var.sku_name)
    error_message = "The SKU must be one of: Basic, Standard, Premium."
  }
}

variable "enable_non_ssl_port" {
  description = "Enable the non-SSL port (6379)"
  type        = bool
  default     = false
}

variable "minimum_tls_version" {
  description = "The minimum TLS version"
  type        = string
  default     = "1.2"
}

variable "maxmemory_reserved" {
  description = "The amount of memory in MB reserved for non-cache usage"
  type        = number
  default     = 50
}

variable "maxmemory_delta" {
  description = "The max-memory delta for the Redis instance"
  type        = number
  default     = 50
}

variable "maxmemory_policy" {
  description = "The max-memory policy of the Redis instance"
  type        = string
  default     = "volatile-lru"
}

variable "maxfragmentationmemory_reserved" {
  description = "The amount of memory in MB reserved for fragmentation"
  type        = number
  default     = 50
}

variable "patch_schedule" {
  description = "List of patch schedules (Premium tier only)"
  type = list(object({
    day_of_week    = string
    start_hour_utc = number
  }))
  default = []
}

variable "aof_backup_enabled" {
  description = "Enable AOF backup (Premium tier only)"
  type        = bool
  default     = false
}

variable "aof_storage_connection_string_0" {
  description = "First storage account connection string for AOF backup (Premium tier only)"
  type        = string
  default     = null
  sensitive   = true
}

variable "aof_storage_connection_string_1" {
  description = "Second storage account connection string for AOF backup (Premium tier only)"
  type        = string
  default     = null
  sensitive   = true
}

variable "private_endpoint_enabled" {
  description = "Whether to create a private endpoint for Redis Cache (Premium tier only)"
  type        = bool
  default     = false
}

variable "private_endpoint_subnet_id" {
  description = "The ID of the subnet for the private endpoint"
  type        = string
  default     = null
}

variable "firewall_rules" {
  description = "Map of firewall rules to create"
  type = map(object({
    start_ip = string
    end_ip   = string
  }))
  default = {}
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}