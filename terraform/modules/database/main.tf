# Database Module
resource "azurerm_resource_group" "db" {
  name     = var.resource_group_name
  location = var.location
  tags     = var.tags
}

resource "azurerm_mssql_server" "server" {
  name                         = var.server_name
  resource_group_name          = azurerm_resource_group.db.name
  location                     = azurerm_resource_group.db.location
  version                      = var.server_version
  administrator_login          = var.administrator_login
  administrator_login_password = var.administrator_login_password
  minimum_tls_version          = "1.2"
  public_network_access_enabled = var.public_network_access_enabled

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Create SQL Database
resource "azurerm_mssql_database" "database" {
  name                = var.database_name
  server_id           = azurerm_mssql_server.server.id
  collation           = var.collation
  license_type        = var.license_type
  sku_name            = var.sku_name
  zone_redundant      = var.zone_redundant
  read_scale          = var.read_scale
  max_size_gb         = var.max_size_gb
  
  short_term_retention_policy {
    retention_days = var.backup_retention_days
  }

  long_term_retention_policy {
    weekly_retention  = var.long_term_retention.weekly_retention
    monthly_retention = var.long_term_retention.monthly_retention
    yearly_retention  = var.long_term_retention.yearly_retention
    week_of_year      = var.long_term_retention.week_of_year
  }

  tags = var.tags
}

# Configure private endpoint if requested
resource "azurerm_private_endpoint" "private_endpoint" {
  count               = var.private_endpoint_enabled ? 1 : 0
  
  name                = "${var.server_name}-private-endpoint"
  location            = azurerm_resource_group.db.location
  resource_group_name = azurerm_resource_group.db.name
  subnet_id           = var.private_endpoint_subnet_id

  private_service_connection {
    name                           = "${var.server_name}-private-connection"
    private_connection_resource_id = azurerm_mssql_server.server.id
    subresource_names              = ["sqlServer"]
    is_manual_connection           = false
  }

  tags = var.tags
}

# Configure firewall rules if public access is enabled
resource "azurerm_mssql_firewall_rule" "firewall_rules" {
  for_each = var.public_network_access_enabled ? var.firewall_rules : {}
  
  name             = each.key
  server_id        = azurerm_mssql_server.server.id
  start_ip_address = each.value.start_ip_address
  end_ip_address   = each.value.end_ip_address
}

# Configure virtual network rules if public access is enabled
resource "azurerm_mssql_virtual_network_rule" "vnet_rules" {
  for_each = var.public_network_access_enabled ? var.vnet_rules : {}
  
  name      = each.key
  server_id = azurerm_mssql_server.server.id
  subnet_id = each.value.subnet_id
}

output "server_id" {
  value = azurerm_mssql_server.server.id
}

output "server_name" {
  value = azurerm_mssql_server.server.name
}

output "server_fqdn" {
  value = azurerm_mssql_server.server.fully_qualified_domain_name
}

output "database_id" {
  value = azurerm_mssql_database.database.id
}

output "database_name" {
  value = azurerm_mssql_database.database.name
}

output "connection_string" {
  value     = "Server=${azurerm_mssql_server.server.fully_qualified_domain_name};Database=${azurerm_mssql_database.database.name};User Id=${var.administrator_login};Password=${var.administrator_login_password};TrustServerCertificate=True;"
  sensitive = true
}