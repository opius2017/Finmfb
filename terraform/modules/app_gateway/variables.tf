variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
}

variable "app_gateway_name" {
  description = "The name of the Application Gateway"
  type        = string
}

variable "sku_name" {
  description = "The name of the SKU for the Application Gateway"
  type        = string
  default     = "Standard_v2"
}

variable "sku_tier" {
  description = "The tier of the SKU for the Application Gateway"
  type        = string
  default     = "Standard_v2"
}

variable "capacity" {
  description = "The capacity of the Application Gateway"
  type        = number
  default     = 2
}

variable "zones" {
  description = "A list of availability zones to deploy the Application Gateway to"
  type        = list(string)
  default     = ["1", "2", "3"]
}

variable "enable_http2" {
  description = "Whether to enable HTTP2 on the Application Gateway"
  type        = bool
  default     = true
}

variable "subnet_id" {
  description = "The ID of the subnet for the Application Gateway"
  type        = string
}

variable "private_ip_address" {
  description = "The private IP address for the Application Gateway"
  type        = string
  default     = null
}

variable "frontend_ports" {
  description = "A list of frontend ports to create"
  type        = list(number)
  default     = [80, 443]
}

variable "ssl_certificate_name" {
  description = "The name of the SSL certificate"
  type        = string
  default     = null
}

variable "ssl_certificate_path" {
  description = "The path to the SSL certificate file"
  type        = string
  default     = null
}

variable "ssl_certificate_data" {
  description = "The base64-encoded SSL certificate data"
  type        = string
  default     = null
  sensitive   = true
}

variable "ssl_certificate_password" {
  description = "The password for the SSL certificate"
  type        = string
  default     = null
  sensitive   = true
}

variable "key_vault_id" {
  description = "The ID of the Key Vault containing the SSL certificate"
  type        = string
  default     = null
}

variable "key_vault_certificate_name" {
  description = "The name of the certificate in Key Vault"
  type        = string
  default     = null
}

variable "backend_address_pools" {
  description = "Map of backend address pools to create"
  type = map(object({
    ip_addresses = list(string)
    fqdns        = list(string)
  }))
}

variable "backend_http_settings" {
  description = "Map of backend HTTP settings to create"
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
}

variable "http_listeners" {
  description = "Map of HTTP listeners to create"
  type = map(object({
    frontend_ip_configuration_name = string
    frontend_port                  = number
    protocol                       = string
    ssl_certificate_name           = string
    host_name                      = string
    host_names                     = list(string)
    require_sni                    = bool
  }))
}

variable "request_routing_rules" {
  description = "Map of request routing rules to create"
  type = map(object({
    rule_type                  = string
    http_listener_name         = string
    backend_address_pool_name  = string
    backend_http_settings_name = string
    url_path_map_name          = string
    redirect_configuration_name = string
    priority                   = number
  }))
}

variable "url_path_maps" {
  description = "Map of URL path maps to create"
  type = map(object({
    default_backend_address_pool_name  = string
    default_backend_http_settings_name = string
    default_redirect_configuration_name = string
    path_rules = map(object({
      paths                      = list(string)
      backend_address_pool_name  = string
      backend_http_settings_name = string
      redirect_configuration_name = string
    }))
  }))
  default = {}
}

variable "health_probes" {
  description = "Map of health probes to create"
  type = map(object({
    protocol                                  = string
    path                                      = string
    host                                      = string
    interval                                  = number
    timeout                                   = number
    unhealthy_threshold                       = number
    pick_host_name_from_backend_http_settings = bool
    match = object({
      body         = string
      status_codes = list(string)
    })
  }))
  default = {}
}

variable "redirect_configurations" {
  description = "Map of redirect configurations to create"
  type = map(object({
    redirect_type        = string
    target_listener_name = string
    target_url           = string
    include_path         = bool
    include_query_string = bool
  }))
  default = {}
}

variable "waf_configuration" {
  description = "WAF configuration for the Application Gateway"
  type = object({
    enabled                  = bool
    firewall_mode            = string
    rule_set_type            = string
    rule_set_version         = string
    file_upload_limit_mb     = number
    request_body_check       = bool
    max_request_body_size_kb = number
    disabled_rule_groups = list(object({
      rule_group_name = string
      rules           = list(string)
    }))
    exclusions = list(object({
      match_variable          = string
      selector                = string
      selector_match_operator = string
    }))
  })
  default = null
}

variable "autoscale_configuration" {
  description = "Autoscale configuration for the Application Gateway"
  type = object({
    min_capacity = number
    max_capacity = number
  })
  default = null
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}