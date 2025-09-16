variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
}

variable "vnet_name" {
  description = "The name of the virtual network"
  type        = string
}

variable "address_space" {
  description = "The address space for the virtual network"
  type        = list(string)
}

variable "subnets" {
  description = "Map of subnet configurations"
  type = map(object({
    address_prefix = string
    service_endpoints = list(string)
    delegations = map(object({
      service_name = string
      actions = list(string)
    }))
    private_endpoint_network_policies_enabled = bool
    private_link_service_network_policies_enabled = bool
    associate_with_nat_gateway = bool
  }))
}

variable "create_nat_gateway" {
  description = "Whether to create a NAT Gateway"
  type        = bool
  default     = false
}

variable "network_security_groups" {
  description = "Map of network security group names to create"
  type        = set(string)
  default     = []
}

variable "nsg_rules" {
  description = "Map of NSG rules to create"
  type = map(object({
    nsg_name                     = string
    priority                     = number
    direction                    = string
    access                       = string
    protocol                     = string
    source_port_range            = string
    destination_port_range       = string
    source_address_prefix        = string
    destination_address_prefix   = string
    source_port_ranges           = list(string)
    destination_port_ranges      = list(string)
    source_address_prefixes      = list(string)
    destination_address_prefixes = list(string)
  }))
  default = {}
}

variable "nsg_subnet_associations" {
  description = "List of NSG to subnet associations"
  type = list(object({
    subnet_name = string
    nsg_name    = string
  }))
  default = []
}

variable "route_tables" {
  description = "Map of route tables to create"
  type = map(object({
    disable_bgp_route_propagation = bool
  }))
  default = {}
}

variable "routes" {
  description = "Map of routes to create"
  type = map(object({
    route_table_name      = string
    address_prefix        = string
    next_hop_type         = string
    next_hop_in_ip_address = string
  }))
  default = {}
}

variable "route_table_subnet_associations" {
  description = "List of route table to subnet associations"
  type = list(object({
    subnet_name      = string
    route_table_name = string
  }))
  default = []
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}