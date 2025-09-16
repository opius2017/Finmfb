# Azure Container Registry (ACR) Module
resource "azurerm_resource_group" "acr" {
  name     = var.resource_group_name
  location = var.location
  tags     = var.tags
}

resource "azurerm_container_registry" "acr" {
  name                = var.acr_name
  resource_group_name = azurerm_resource_group.acr.name
  location            = azurerm_resource_group.acr.location
  sku                 = var.sku
  admin_enabled       = var.admin_enabled
  
  # Enable zone redundancy for Premium SKU
  dynamic "georeplications" {
    for_each = var.sku == "Premium" && length(var.georeplications) > 0 ? var.georeplications : []
    content {
      location                = georeplications.value.location
      zone_redundancy_enabled = georeplications.value.zone_redundancy_enabled
      tags                    = var.tags
    }
  }

  # Enable encryption
  dynamic "encryption" {
    for_each = var.encryption_enabled ? [1] : []
    content {
      enabled            = true
      key_vault_key_id   = var.key_vault_key_id
      identity_client_id = var.identity_client_id
    }
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

output "id" {
  value = azurerm_container_registry.acr.id
}

output "login_server" {
  value = azurerm_container_registry.acr.login_server
}

output "admin_username" {
  value     = var.admin_enabled ? azurerm_container_registry.acr.admin_username : null
  sensitive = true
}

output "admin_password" {
  value     = var.admin_enabled ? azurerm_container_registry.acr.admin_password : null
  sensitive = true
}

output "identity" {
  value = azurerm_container_registry.acr.identity
}