variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
}

variable "cluster_name" {
  description = "The name of the AKS cluster"
  type        = string
}

variable "dns_prefix" {
  description = "DNS prefix for the AKS cluster"
  type        = string
}

variable "kubernetes_version" {
  description = "Kubernetes version"
  type        = string
  default     = "1.27.3"
}

variable "node_count" {
  description = "The initial number of nodes in the default node pool"
  type        = number
  default     = 3
}

variable "vm_size" {
  description = "The size of the VMs in the default node pool"
  type        = string
  default     = "Standard_DS2_v2"
}

variable "os_disk_size_gb" {
  description = "The OS disk size for nodes in the default node pool"
  type        = number
  default     = 128
}

variable "subnet_id" {
  description = "The ID of the subnet where the nodes will be deployed"
  type        = string
}

variable "enable_auto_scaling" {
  description = "Enable auto scaling for the default node pool"
  type        = bool
  default     = true
}

variable "min_node_count" {
  description = "The minimum number of nodes in the default node pool when auto scaling is enabled"
  type        = number
  default     = 1
}

variable "max_node_count" {
  description = "The maximum number of nodes in the default node pool when auto scaling is enabled"
  type        = number
  default     = 5
}

variable "admin_group_object_ids" {
  description = "Object IDs of Azure AD groups with admin access to the cluster"
  type        = list(string)
  default     = []
}

variable "service_cidr" {
  description = "The CIDR for Kubernetes services"
  type        = string
  default     = "10.0.0.0/16"
}

variable "dns_service_ip" {
  description = "The IP address for the Kubernetes DNS service"
  type        = string
  default     = "10.0.0.10"
}

variable "docker_bridge_cidr" {
  description = "The CIDR for the Docker bridge network"
  type        = string
  default     = "172.17.0.1/16"
}

variable "log_analytics_workspace_id" {
  description = "The ID of the Log Analytics workspace for AKS monitoring"
  type        = string
}

variable "additional_node_pools" {
  description = "Additional node pools to create"
  type = map(object({
    vm_size             = string
    node_count          = number
    os_disk_size_gb     = number
    enable_auto_scaling = bool
    min_count           = number
    max_count           = number
    node_taints         = list(string)
    node_labels         = map(string)
  }))
  default = {}
}

variable "acr_id" {
  description = "The ID of the Azure Container Registry"
  type        = string
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}