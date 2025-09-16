provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
    key_vault {
      purge_soft_delete_on_destroy          = true
      recover_soft_deleted_key_vaults       = true
      purge_soft_deleted_secrets_on_destroy = true
    }
  }
}

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
  backend "azurerm" {
    resource_group_name  = "finmfb-terraform-state"
    storage_account_name = "finmfbterraformstate"
    container_name       = "terraform-state"
    key                  = "dev.terraform.tfstate"
  }
}

# Resource Group for the entire environment
resource "azurerm_resource_group" "main" {
  name     = "${var.prefix}-${var.env}-rg"
  location = var.location
  tags     = var.tags
}

# Virtual Network
module "network" {
  source              = "../../modules/network"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  vnet_name           = "${var.prefix}-${var.env}-vnet"
  vnet_address_space  = var.vnet_address_space
  subnets             = var.subnets
  tags                = var.tags
}

# Azure Kubernetes Service
module "aks" {
  source                    = "../../modules/aks"
  resource_group_name       = azurerm_resource_group.main.name
  location                  = var.location
  cluster_name              = "${var.prefix}-${var.env}-aks"
  kubernetes_version        = var.kubernetes_version
  dns_prefix                = "${var.prefix}${var.env}"
  default_node_pool_name    = "default"
  default_node_pool_count   = var.default_node_pool_count
  default_node_pool_vm_size = var.default_node_pool_vm_size
  default_node_pool_subnet_id = module.network.subnet_ids["aks"]
  additional_node_pools     = var.additional_node_pools
  network_plugin            = "azure"
  network_policy            = "calico"
  service_cidr              = var.service_cidr
  dns_service_ip            = var.dns_service_ip
  docker_bridge_cidr        = var.docker_bridge_cidr
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id
  admin_group_object_ids    = var.admin_group_object_ids
  tags                      = var.tags
}

# Azure Container Registry
module "acr" {
  source                     = "../../modules/acr"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = var.location
  acr_name                   = "${replace(var.prefix, "-", "")}${var.env}acr"
  sku                        = "Premium"
  admin_enabled              = false
  georeplication_locations   = var.env == "prod" ? var.acr_georeplication_locations : []
  tags                       = var.tags
}

# Grant AKS access to ACR
resource "azurerm_role_assignment" "aks_acr_pull" {
  scope                = module.acr.id
  role_definition_name = "AcrPull"
  principal_id         = module.aks.kubelet_identity_object_id
}

# Redis Cache for distributed caching
module "redis" {
  source              = "../../modules/redis"
  resource_group_name = "${var.prefix}-${var.env}-redis-rg"
  location            = var.location
  redis_name          = "${var.prefix}-${var.env}-redis"
  capacity            = var.env == "prod" ? 2 : 1
  family              = var.env == "prod" ? "P" : "C"
  sku_name            = var.env == "prod" ? "Premium" : "Standard"
  enable_non_ssl_port = false
  firewall_rules      = var.redis_firewall_rules
  patch_schedule      = var.env == "prod" ? var.redis_patch_schedule : []
  tags                = var.tags
}

# Application Gateway for API Gateway
module "app_gateway" {
  source               = "../../modules/app_gateway"
  resource_group_name  = "${var.prefix}-${var.env}-appgw-rg"
  location             = var.location
  app_gateway_name     = "${var.prefix}-${var.env}-appgw"
  sku_name             = var.env == "prod" ? "WAF_v2" : "Standard_v2"
  sku_tier             = var.env == "prod" ? "WAF_v2" : "Standard_v2"
  capacity             = var.env == "prod" ? 2 : 1
  subnet_id            = module.network.subnet_ids["app-gateway"]
  frontend_ports       = [80, 443]
  backend_address_pools = var.app_gateway_backend_pools
  backend_http_settings = var.app_gateway_backend_http_settings
  http_listeners       = var.app_gateway_http_listeners
  request_routing_rules = var.app_gateway_routing_rules
  waf_configuration    = var.env == "prod" ? var.app_gateway_waf_configuration : null
  tags                 = var.tags
}

# Monitoring resources
module "monitoring" {
  source                     = "../../modules/monitoring"
  resource_group_name        = "${var.prefix}-${var.env}-monitoring-rg"
  location                   = var.location
  log_analytics_workspace_name = "${var.prefix}-${var.env}-law"
  log_analytics_sku          = "PerGB2018"
  log_retention_in_days      = var.env == "prod" ? 30 : 7
  app_insights_name          = "${var.prefix}-${var.env}-appinsights"
  tags                       = var.tags
}

# Azure Database for PostgreSQL
module "database" {
  source                   = "../../modules/database"
  resource_group_name      = "${var.prefix}-${var.env}-db-rg"
  location                 = var.location
  server_name              = "${var.prefix}-${var.env}-pgsql"
  sku_name                 = var.env == "prod" ? "GP_Gen5_4" : "GP_Gen5_2"
  storage_mb               = var.env == "prod" ? 102400 : 51200
  backup_retention_days    = var.env == "prod" ? 35 : 7
  geo_redundant_backup     = var.env == "prod" ? true : false
  administrator_login      = var.db_admin_username
  administrator_password   = var.db_admin_password
  server_version           = "11"
  ssl_enforcement_enabled  = true
  databases                = var.databases
  vnet_rules               = {
    aks = {
      subnet_id = module.network.subnet_ids["aks"]
    }
  }
  firewall_rules           = var.db_firewall_rules
  tags                     = var.tags
}

# Key Vault for secrets management
module "key_vault" {
  source                   = "../../modules/key_vault"
  resource_group_name      = "${var.prefix}-${var.env}-kv-rg"
  location                 = var.location
  key_vault_name           = "${var.prefix}-${var.env}-kv"
  sku_name                 = "standard"
  soft_delete_retention_days = 7
  purge_protection_enabled = var.env == "prod" ? true : false
  
  network_acls = {
    default_action = "Deny"
    bypass         = "AzureServices"
    ip_rules       = var.key_vault_ip_rules
    virtual_network_subnet_ids = [
      module.network.subnet_ids["aks"]
    ]
  }
  
  secrets = {
    "Redis--ConnectionString" = module.redis.primary_connection_string
    "Database--ConnectionString" = "Host=${module.database.fqdn};Database=${var.databases[0]};Username=${var.db_admin_username}@${module.database.fqdn};Password=${var.db_admin_password};SslMode=Require;"
  }
  
  access_policies = {
    aks = {
      tenant_id = data.azurerm_client_config.current.tenant_id
      object_id = module.aks.kubelet_identity_object_id
      secret_permissions = ["Get", "List"]
      key_permissions    = []
      certificate_permissions = []
    }
  }
  
  tags = var.tags
}

# Add Application Insights connection string to Key Vault
resource "azurerm_key_vault_secret" "app_insights_connection_string" {
  name         = "ApplicationInsights--ConnectionString"
  value        = module.monitoring.app_insights_connection_string
  key_vault_id = module.key_vault.id
}

# Add Application Insights instrumentation key to Key Vault
resource "azurerm_key_vault_secret" "app_insights_instrumentation_key" {
  name         = "ApplicationInsights--InstrumentationKey"
  value        = module.monitoring.app_insights_instrumentation_key
  key_vault_id = module.key_vault.id
}

# Data Sources
data "azurerm_client_config" "current" {}