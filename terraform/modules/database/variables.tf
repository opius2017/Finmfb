variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
}

variable "server_name" {
  description = "The name of the SQL Server"
  type        = string
}

variable "server_version" {
  description = "The version of the SQL Server"
  type        = string
  default     = "12.0"
}

variable "administrator_login" {
  description = "The administrator login for the SQL Server"
  type        = string
}

variable "administrator_login_password" {
  description = "The administrator login password for the SQL Server"
  type        = string
  sensitive   = true
}

variable "database_name" {
  description = "The name of the SQL Database"
  type        = string
}

variable "collation" {
  description = "The collation of the database"
  type        = string
  default     = "SQL_Latin1_General_CP1_CI_AS"
}

variable "license_type" {
  description = "The license type for the database"
  type        = string
  default     = "LicenseIncluded"
}

variable "sku_name" {
  description = "The SKU name for the database"
  type        = string
  default     = "S1"
}

variable "zone_redundant" {
  description = "Whether to enable zone redundancy for the database"
  type        = bool
  default     = false
}

variable "read_scale" {
  description = "Whether to enable read scale for the database"
  type        = bool
  default     = false
}

variable "max_size_gb" {
  description = "The maximum size of the database in GB"
  type        = number
  default     = 32
}

variable "backup_retention_days" {
  description = "The number of days to retain short-term backups"
  type        = number
  default     = 7
}

variable "long_term_retention" {
  description = "Configuration for long-term retention policy"
  type = object({
    weekly_retention  = string
    monthly_retention = string
    yearly_retention  = string
    week_of_year      = number
  })
  default = {
    weekly_retention  = "P4W"
    monthly_retention = "P12M"
    yearly_retention  = "P5Y"
    week_of_year      = 1
  }
}

variable "public_network_access_enabled" {
  description = "Whether public network access is enabled for the SQL Server"
  type        = bool
  default     = false
}

variable "private_endpoint_enabled" {
  description = "Whether to create a private endpoint for the SQL Server"
  type        = bool
  default     = true
}

variable "private_endpoint_subnet_id" {
  description = "The ID of the subnet for the private endpoint"
  type        = string
  default     = null
}

variable "firewall_rules" {
  description = "Map of firewall rules to create"
  type = map(object({
    start_ip_address = string
    end_ip_address   = string
  }))
  default = {}
}

variable "vnet_rules" {
  description = "Map of virtual network rules to create"
  type = map(object({
    subnet_id = string
  }))
  default = {}
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}