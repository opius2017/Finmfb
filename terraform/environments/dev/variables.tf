variable "prefix" {
  description = "The prefix to use for all resources in this environment"
  type        = string
  default     = "finmfb"
}

variable "env" {
  description = "The environment name"
  type        = string
  default     = "dev"
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
  default     = "eastus"
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default = {
    Environment = "Development"
    Project     = "FinMFB"
    Terraform   = "true"
  }
}

# Network variables
variable "vnet_address_space" {
  description = "The address space of the virtual network"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

variable "subnets" {
  description = "Map of subnet names to their configuration"
  type = map(object({
    address_prefixes                          = list(string)
    service_endpoints                         = list(string)
    private_endpoint_network_policies_enabled = bool
    delegation                                = map(list(string))
  }))
  default = {
    aks = {
      address_prefixes                          = ["10.0.0.0/22"]
      service_endpoints                         = ["Microsoft.Sql", "Microsoft.Storage", "Microsoft.KeyVault", "Microsoft.ContainerRegistry"]
      private_endpoint_network_policies_enabled = true
      delegation                                = {}
    }
    app-gateway = {
      address_prefixes                          = ["10.0.4.0/24"]
      service_endpoints                         = []
      private_endpoint_network_policies_enabled = true
      delegation                                = {}
    }
    db = {
      address_prefixes                          = ["10.0.5.0/24"]
      service_endpoints                         = ["Microsoft.Sql"]
      private_endpoint_network_policies_enabled = false
      delegation                                = {}
    }
    private-endpoints = {
      address_prefixes                          = ["10.0.6.0/24"]
      service_endpoints                         = []
      private_endpoint_network_policies_enabled = false
      delegation                                = {}
    }
  }
}

# AKS variables
variable "kubernetes_version" {
  description = "The Kubernetes version to use"
  type        = string
  default     = "1.25.6"
}

variable "default_node_pool_count" {
  description = "The number of nodes in the default node pool"
  type        = number
  default     = 2
}

variable "default_node_pool_vm_size" {
  description = "The VM size for the default node pool"
  type        = string
  default     = "Standard_D2s_v3"
}

variable "additional_node_pools" {
  description = "Map of additional node pools to create"
  type = map(object({
    vm_size             = string
    node_count          = number
    enable_auto_scaling = bool
    min_count           = number
    max_count           = number
    max_pods            = number
    node_taints         = list(string)
    node_labels         = map(string)
  }))
  default = {
    userpool = {
      vm_size             = "Standard_D4s_v3"
      node_count          = 2
      enable_auto_scaling = true
      min_count           = 2
      max_count           = 5
      max_pods            = 30
      node_taints         = []
      node_labels = {
        "nodepool-type" = "user"
        "environment"   = "dev"
      }
    }
  }
}

variable "service_cidr" {
  description = "The CIDR block for Kubernetes services"
  type        = string
  default     = "10.1.0.0/16"
}

variable "dns_service_ip" {
  description = "The IP address for the Kubernetes DNS service"
  type        = string
  default     = "10.1.0.10"
}

variable "docker_bridge_cidr" {
  description = "The CIDR block for the Docker bridge"
  type        = string
  default     = "172.17.0.1/16"
}

variable "admin_group_object_ids" {
  description = "List of AD group object IDs with admin access to the AKS cluster"
  type        = list(string)
  default     = []
}

# ACR variables
variable "acr_georeplication_locations" {
  description = "List of Azure locations where the ACR should be geo-replicated"
  type        = list(string)
  default     = []
}

# Redis Cache variables
variable "redis_firewall_rules" {
  description = "Map of firewall rules for the Redis Cache"
  type = map(object({
    start_ip = string
    end_ip   = string
  }))
  default = {
    azure_portal = {
      start_ip = "0.0.0.0"
      end_ip   = "0.0.0.0"
    }
    development = {
      start_ip = "0.0.0.0"
      end_ip   = "0.0.0.0"
    }
  }
}

variable "redis_patch_schedule" {
  description = "List of Redis patch schedules"
  type = list(object({
    day_of_week    = string
    start_hour_utc = number
  }))
  default = [
    {
      day_of_week    = "Sunday"
      start_hour_utc = 2
    }
  ]
}

# Application Gateway variables
variable "app_gateway_backend_pools" {
  description = "Map of backend address pools for the Application Gateway"
  type = map(object({
    ip_addresses = list(string)
    fqdns        = list(string)
  }))
  default = {
    api-backend = {
      ip_addresses = []
      fqdns        = ["api.finmfb.local"]
    }
    web-backend = {
      ip_addresses = []
      fqdns        = ["web.finmfb.local"]
    }
  }
}

variable "app_gateway_backend_http_settings" {
  description = "Map of backend HTTP settings for the Application Gateway"
  type = map(object({
    cookie_based_affinity               = string
    affinity_cookie_name                = string
    path                                = string
    port                                = number
    protocol                            = string
    request_timeout                     = number
    probe_name                          = string
    host_name                           = string
    pick_host_name_from_backend_address = bool
    trusted_root_certificate_names      = list(string)
  }))
  default = {
    api-settings = {
      cookie_based_affinity               = "Disabled"
      affinity_cookie_name                = null
      path                                = "/"
      port                                = 80
      protocol                            = "Http"
      request_timeout                     = 30
      probe_name                          = null
      host_name                           = "api.finmfb.local"
      pick_host_name_from_backend_address = false
      trusted_root_certificate_names      = []
    }
    web-settings = {
      cookie_based_affinity               = "Disabled"
      affinity_cookie_name                = null
      path                                = "/"
      port                                = 80
      protocol                            = "Http"
      request_timeout                     = 30
      probe_name                          = null
      host_name                           = "web.finmfb.local"
      pick_host_name_from_backend_address = false
      trusted_root_certificate_names      = []
    }
  }
}

variable "app_gateway_http_listeners" {
  description = "Map of HTTP listeners for the Application Gateway"
  type = map(object({
    frontend_ip_configuration_name = string
    frontend_port                  = number
    protocol                       = string
    ssl_certificate_name           = string
    host_name                      = string
    host_names                     = list(string)
    require_sni                    = bool
  }))
  default = {
    api-listener = {
      frontend_ip_configuration_name = "finmfb-dev-appgw-fe-ip-public"
      frontend_port                  = 80
      protocol                       = "Http"
      ssl_certificate_name           = null
      host_name                      = "api.finmfb.local"
      host_names                     = null
      require_sni                    = false
    }
    web-listener = {
      frontend_ip_configuration_name = "finmfb-dev-appgw-fe-ip-public"
      frontend_port                  = 80
      protocol                       = "Http"
      ssl_certificate_name           = null
      host_name                      = "web.finmfb.local"
      host_names                     = null
      require_sni                    = false
    }
  }
}

variable "app_gateway_routing_rules" {
  description = "Map of request routing rules for the Application Gateway"
  type = map(object({
    rule_type                  = string
    http_listener_name         = string
    backend_address_pool_name  = string
    backend_http_settings_name = string
    url_path_map_name          = string
    redirect_configuration_name = string
    priority                   = number
  }))
  default = {
    api-rule = {
      rule_type                  = "Basic"
      http_listener_name         = "api-listener"
      backend_address_pool_name  = "api-backend"
      backend_http_settings_name = "api-settings"
      url_path_map_name          = null
      redirect_configuration_name = null
      priority                   = 100
    }
    web-rule = {
      rule_type                  = "Basic"
      http_listener_name         = "web-listener"
      backend_address_pool_name  = "web-backend"
      backend_http_settings_name = "web-settings"
      url_path_map_name          = null
      redirect_configuration_name = null
      priority                   = 101
    }
  }
}

variable "app_gateway_waf_configuration" {
  description = "WAF configuration for the Application Gateway"
  type = object({
    enabled                  = bool
    firewall_mode            = string
    rule_set_type            = string
    rule_set_version         = string
    file_upload_limit_mb     = number
    request_body_check       = bool
    max_request_body_size_kb = number
    disabled_rule_groups     = list(object({
      rule_group_name = string
      rules           = list(string)
    }))
    exclusions              = list(object({
      match_variable          = string
      selector                = string
      selector_match_operator = string
    }))
  })
  default = {
    enabled                  = true
    firewall_mode            = "Prevention"
    rule_set_type            = "OWASP"
    rule_set_version         = "3.2"
    file_upload_limit_mb     = 100
    request_body_check       = true
    max_request_body_size_kb = 128
    disabled_rule_groups     = []
    exclusions               = []
  }
}

# Database variables
variable "db_admin_username" {
  description = "The admin username for the PostgreSQL server"
  type        = string
  default     = "finmfbadmin"
  sensitive   = true
}

variable "db_admin_password" {
  description = "The admin password for the PostgreSQL server"
  type        = string
  sensitive   = true
}

variable "databases" {
  description = "List of databases to create in the PostgreSQL server"
  type        = list(string)
  default     = ["finmfb"]
}

variable "db_firewall_rules" {
  description = "Map of firewall rules for the PostgreSQL server"
  type = map(object({
    start_ip_address = string
    end_ip_address   = string
  }))
  default = {
    azure_services = {
      start_ip_address = "0.0.0.0"
      end_ip_address   = "0.0.0.0"
    }
    development = {
      start_ip_address = "0.0.0.0"
      end_ip_address   = "0.0.0.0"
    }
  }
}

# Key Vault variables
variable "key_vault_ip_rules" {
  description = "List of IP addresses or CIDR blocks for the Key Vault firewall"
  type        = list(string)
  default     = ["0.0.0.0/0"]
}