output "id" {
  description = "The ID of the Application Gateway"
  value       = azurerm_application_gateway.app_gateway.id
}

output "name" {
  description = "The name of the Application Gateway"
  value       = azurerm_application_gateway.app_gateway.name
}

output "public_ip_address" {
  description = "The public IP address of the Application Gateway"
  value       = azurerm_public_ip.app_gateway_ip.ip_address
}

output "public_ip_id" {
  description = "The ID of the public IP address of the Application Gateway"
  value       = azurerm_public_ip.app_gateway_ip.id
}

output "backend_address_pool_ids" {
  description = "Map of backend address pool names to their IDs"
  value = {
    for k, v in azurerm_application_gateway.app_gateway.backend_address_pool : v.name => v.id
  }
}

output "backend_http_settings_ids" {
  description = "Map of backend HTTP settings names to their IDs"
  value = {
    for k, v in azurerm_application_gateway.app_gateway.backend_http_settings : v.name => v.id
  }
}

output "http_listener_ids" {
  description = "Map of HTTP listener names to their IDs"
  value = {
    for k, v in azurerm_application_gateway.app_gateway.http_listener : v.name => v.id
  }
}

output "identity_id" {
  description = "The ID of the User Assigned Identity for the Application Gateway"
  value       = azurerm_user_assigned_identity.app_gateway_identity.id
}

output "identity_principal_id" {
  description = "The Principal ID of the User Assigned Identity for the Application Gateway"
  value       = azurerm_user_assigned_identity.app_gateway_identity.principal_id
}

output "frontend_ip_configuration_ids" {
  description = "Map of frontend IP configuration names to their IDs"
  value = {
    for k, v in azurerm_application_gateway.app_gateway.frontend_ip_configuration : v.name => v.id
  }
}

output "frontend_port_ids" {
  description = "Map of frontend port names to their IDs"
  value = {
    for k, v in azurerm_application_gateway.app_gateway.frontend_port : v.name => v.id
  }
}

output "resource_group_name" {
  description = "The name of the resource group containing the Application Gateway"
  value       = azurerm_resource_group.app_gateway.name
}

output "resource_group_id" {
  description = "The ID of the resource group containing the Application Gateway"
  value       = azurerm_resource_group.app_gateway.id
}