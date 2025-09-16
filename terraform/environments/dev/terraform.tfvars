prefix    = "finmfb"
env       = "dev"
location  = "eastus"

tags = {
  Environment = "Development"
  Project     = "FinMFB"
  Terraform   = "true"
  Owner       = "DevOps"
}

# Network configuration
vnet_address_space = ["10.0.0.0/16"]

# AKS configuration
kubernetes_version       = "1.25.6"
default_node_pool_count  = 2
default_node_pool_vm_size = "Standard_D2s_v3"

additional_node_pools = {
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

service_cidr       = "10.1.0.0/16"
dns_service_ip     = "10.1.0.10"
docker_bridge_cidr = "172.17.0.1/16"

# Database configuration
db_admin_username = "finmfbadmin"
# db_admin_password must be provided via environment variable or command line
# Avoid storing sensitive values in this file
# TF_VAR_db_admin_password="YourPasswordHere"

databases = ["finmfb"]

# Redis configuration
redis_firewall_rules = {
  azure_services = {
    start_ip = "0.0.0.0"
    end_ip   = "0.0.0.0"
  }
  development = {
    start_ip = "0.0.0.0" # This should be your trusted IP in production
    end_ip   = "0.0.0.0" # This should be your trusted IP in production
  }
}

# Application Gateway configuration for API Gateway pattern
app_gateway_backend_pools = {
  api-backend = {
    ip_addresses = []
    fqdns        = ["api.finmfb.local"] # This will be updated dynamically in production
  }
  web-backend = {
    ip_addresses = []
    fqdns        = ["web.finmfb.local"] # This will be updated dynamically in production
  }
}

# Key Vault configuration
key_vault_ip_rules = ["0.0.0.0/0"] # This should be restricted to trusted IPs in production