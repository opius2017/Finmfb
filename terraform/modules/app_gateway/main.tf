# Azure Application Gateway Module
resource "azurerm_resource_group" "app_gateway" {
  name     = var.resource_group_name
  location = var.location
  tags     = var.tags
}

# Public IP for App Gateway
resource "azurerm_public_ip" "app_gateway_ip" {
  name                = "${var.app_gateway_name}-ip"
  resource_group_name = azurerm_resource_group.app_gateway.name
  location            = azurerm_resource_group.app_gateway.location
  allocation_method   = "Static"
  sku                 = "Standard"
  zones               = var.zones
  tags                = var.tags
}

# User Assigned Identity for App Gateway
resource "azurerm_user_assigned_identity" "app_gateway_identity" {
  name                = "${var.app_gateway_name}-identity"
  resource_group_name = azurerm_resource_group.app_gateway.name
  location            = azurerm_resource_group.app_gateway.location
  tags                = var.tags
}

# SSL certificate for App Gateway (if provided)
locals {
  has_ssl_cert       = var.ssl_certificate_name != null && var.ssl_certificate_path != null
  has_ssl_cert_data  = var.ssl_certificate_name != null && var.ssl_certificate_data != null
  has_key_vault_cert = var.key_vault_certificate_name != null && var.key_vault_id != null
}

# Application Gateway
resource "azurerm_application_gateway" "app_gateway" {
  name                = var.app_gateway_name
  resource_group_name = azurerm_resource_group.app_gateway.name
  location            = azurerm_resource_group.app_gateway.location
  zones               = var.zones
  enable_http2        = var.enable_http2
  tags                = var.tags

  sku {
    name     = var.sku_name
    tier     = var.sku_tier
    capacity = var.capacity
  }

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.app_gateway_identity.id]
  }

  gateway_ip_configuration {
    name      = "${var.app_gateway_name}-ip-config"
    subnet_id = var.subnet_id
  }

  frontend_ip_configuration {
    name                 = "${var.app_gateway_name}-fe-ip-public"
    public_ip_address_id = azurerm_public_ip.app_gateway_ip.id
  }

  # Add a private frontend IP if provided
  dynamic "frontend_ip_configuration" {
    for_each = var.private_ip_address != null ? [1] : []
    content {
      name                          = "${var.app_gateway_name}-fe-ip-private"
      subnet_id                     = var.subnet_id
      private_ip_address            = var.private_ip_address
      private_ip_address_allocation = "Static"
    }
  }

  # Frontend ports
  dynamic "frontend_port" {
    for_each = var.frontend_ports
    content {
      name = "${var.app_gateway_name}-fe-port-${frontend_port.value}"
      port = frontend_port.value
    }
  }

  # SSL Certificate from file
  dynamic "ssl_certificate" {
    for_each = local.has_ssl_cert ? [1] : []
    content {
      name     = var.ssl_certificate_name
      data     = filebase64(var.ssl_certificate_path)
      password = var.ssl_certificate_password
    }
  }

  # SSL Certificate from variable
  dynamic "ssl_certificate" {
    for_each = local.has_ssl_cert_data ? [1] : []
    content {
      name     = var.ssl_certificate_name
      data     = var.ssl_certificate_data
      password = var.ssl_certificate_password
    }
  }

  # SSL Certificate from Key Vault
  dynamic "ssl_certificate" {
    for_each = local.has_key_vault_cert ? [1] : []
    content {
      name                = var.key_vault_certificate_name
      key_vault_secret_id = "${var.key_vault_id}/secrets/${var.key_vault_certificate_name}"
    }
  }

  # Backend Address Pools
  dynamic "backend_address_pool" {
    for_each = var.backend_address_pools
    content {
      name         = backend_address_pool.key
      ip_addresses = backend_address_pool.value.ip_addresses
      fqdns        = backend_address_pool.value.fqdns
    }
  }

  # Backend HTTP settings
  dynamic "backend_http_settings" {
    for_each = var.backend_http_settings
    content {
      name                                = backend_http_settings.key
      cookie_based_affinity               = backend_http_settings.value.cookie_based_affinity
      affinity_cookie_name                = backend_http_settings.value.affinity_cookie_name
      path                                = backend_http_settings.value.path
      port                                = backend_http_settings.value.port
      protocol                            = backend_http_settings.value.protocol
      request_timeout                     = backend_http_settings.value.request_timeout
      probe_name                          = backend_http_settings.value.probe_name
      host_name                           = backend_http_settings.value.host_name
      pick_host_name_from_backend_address = backend_http_settings.value.pick_host_name_from_backend_address
      trusted_root_certificate_names      = backend_http_settings.value.trusted_root_certificate_names
    }
  }

  # HTTP Listeners
  dynamic "http_listener" {
    for_each = var.http_listeners
    content {
      name                           = http_listener.key
      frontend_ip_configuration_name = http_listener.value.frontend_ip_configuration_name
      frontend_port_name             = "${var.app_gateway_name}-fe-port-${http_listener.value.frontend_port}"
      protocol                       = http_listener.value.protocol
      ssl_certificate_name           = http_listener.value.ssl_certificate_name
      host_name                      = http_listener.value.host_name
      host_names                     = http_listener.value.host_names
      require_sni                    = http_listener.value.require_sni
    }
  }

  # Request routing rules
  dynamic "request_routing_rule" {
    for_each = var.request_routing_rules
    content {
      name                       = request_routing_rule.key
      rule_type                  = request_routing_rule.value.rule_type
      http_listener_name         = request_routing_rule.value.http_listener_name
      backend_address_pool_name  = request_routing_rule.value.backend_address_pool_name
      backend_http_settings_name = request_routing_rule.value.backend_http_settings_name
      url_path_map_name          = request_routing_rule.value.url_path_map_name
      redirect_configuration_name = request_routing_rule.value.redirect_configuration_name
      priority                   = request_routing_rule.value.priority
    }
  }

  # URL path maps
  dynamic "url_path_map" {
    for_each = var.url_path_maps
    content {
      name                               = url_path_map.key
      default_backend_address_pool_name  = url_path_map.value.default_backend_address_pool_name
      default_backend_http_settings_name = url_path_map.value.default_backend_http_settings_name
      default_redirect_configuration_name = url_path_map.value.default_redirect_configuration_name

      dynamic "path_rule" {
        for_each = url_path_map.value.path_rules
        content {
          name                       = path_rule.key
          paths                      = path_rule.value.paths
          backend_address_pool_name  = path_rule.value.backend_address_pool_name
          backend_http_settings_name = path_rule.value.backend_http_settings_name
          redirect_configuration_name = path_rule.value.redirect_configuration_name
        }
      }
    }
  }

  # Health probes
  dynamic "probe" {
    for_each = var.health_probes
    content {
      name                                      = probe.key
      protocol                                  = probe.value.protocol
      path                                      = probe.value.path
      host                                      = probe.value.host
      interval                                  = probe.value.interval
      timeout                                   = probe.value.timeout
      unhealthy_threshold                       = probe.value.unhealthy_threshold
      pick_host_name_from_backend_http_settings = probe.value.pick_host_name_from_backend_http_settings
      match {
        body        = probe.value.match.body
        status_code = probe.value.match.status_codes
      }
    }
  }

  # Redirect configurations
  dynamic "redirect_configuration" {
    for_each = var.redirect_configurations
    content {
      name                 = redirect_configuration.key
      redirect_type        = redirect_configuration.value.redirect_type
      target_listener_name = redirect_configuration.value.target_listener_name
      target_url           = redirect_configuration.value.target_url
      include_path         = redirect_configuration.value.include_path
      include_query_string = redirect_configuration.value.include_query_string
    }
  }

  # Web Application Firewall configuration
  dynamic "waf_configuration" {
    for_each = var.sku_tier == "WAF" || var.sku_tier == "WAF_v2" ? [1] : []
    content {
      enabled                  = var.waf_configuration.enabled
      firewall_mode            = var.waf_configuration.firewall_mode
      rule_set_type            = var.waf_configuration.rule_set_type
      rule_set_version         = var.waf_configuration.rule_set_version
      file_upload_limit_mb     = var.waf_configuration.file_upload_limit_mb
      request_body_check       = var.waf_configuration.request_body_check
      max_request_body_size_kb = var.waf_configuration.max_request_body_size_kb
      
      dynamic "disabled_rule_group" {
        for_each = var.waf_configuration.disabled_rule_groups
        content {
          rule_group_name = disabled_rule_group.value.rule_group_name
          rules           = disabled_rule_group.value.rules
        }
      }
      
      dynamic "exclusion" {
        for_each = var.waf_configuration.exclusions
        content {
          match_variable          = exclusion.value.match_variable
          selector                = exclusion.value.selector
          selector_match_operator = exclusion.value.selector_match_operator
        }
      }
    }
  }

  # Autoscale configuration
  dynamic "autoscale_configuration" {
    for_each = var.autoscale_configuration != null ? [1] : []
    content {
      min_capacity = var.autoscale_configuration.min_capacity
      max_capacity = var.autoscale_configuration.max_capacity
    }
  }
}

output "id" {
  value = azurerm_application_gateway.app_gateway.id
}

output "name" {
  value = azurerm_application_gateway.app_gateway.name
}

output "public_ip_address" {
  value = azurerm_public_ip.app_gateway_ip.ip_address
}

output "backend_address_pool_ids" {
  value = {
    for k, v in azurerm_application_gateway.app_gateway.backend_address_pool : v.name => v.id
  }
}

output "identity_id" {
  value = azurerm_user_assigned_identity.app_gateway_identity.id
}

output "identity_principal_id" {
  value = azurerm_user_assigned_identity.app_gateway_identity.principal_id
}