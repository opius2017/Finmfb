# Network Module for AKS
resource "azurerm_resource_group" "network" {
  name     = var.resource_group_name
  location = var.location
  tags     = var.tags
}

# Create a virtual network
resource "azurerm_virtual_network" "vnet" {
  name                = var.vnet_name
  address_space       = var.address_space
  location            = azurerm_resource_group.network.location
  resource_group_name = azurerm_resource_group.network.name
  tags                = var.tags
}

# Create subnets
resource "azurerm_subnet" "subnets" {
  for_each = var.subnets

  name                 = each.key
  resource_group_name  = azurerm_resource_group.network.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = [each.value.address_prefix]
  
  dynamic "delegation" {
    for_each = each.value.delegations != null ? each.value.delegations : {}
    content {
      name = delegation.key
      service_delegation {
        name    = delegation.value.service_name
        actions = delegation.value.actions
      }
    }
  }

  # Service endpoints
  service_endpoints = each.value.service_endpoints
  
  # Private endpoint network policies
  private_endpoint_network_policies_enabled = each.value.private_endpoint_network_policies_enabled
  
  # Private link service network policies
  private_link_service_network_policies_enabled = each.value.private_link_service_network_policies_enabled
}

# Create a NAT Gateway if required
resource "azurerm_public_ip" "nat_gateway_ip" {
  count               = var.create_nat_gateway ? 1 : 0
  
  name                = "${var.vnet_name}-nat-gateway-ip"
  location            = azurerm_resource_group.network.location
  resource_group_name = azurerm_resource_group.network.name
  allocation_method   = "Static"
  sku                 = "Standard"
  zones               = ["1", "2", "3"]
  tags                = var.tags
}

resource "azurerm_nat_gateway" "nat_gateway" {
  count               = var.create_nat_gateway ? 1 : 0
  
  name                = "${var.vnet_name}-nat-gateway"
  location            = azurerm_resource_group.network.location
  resource_group_name = azurerm_resource_group.network.name
  sku_name            = "Standard"
  idle_timeout_in_minutes = 10
  zones               = ["1", "2", "3"]
  tags                = var.tags
}

resource "azurerm_nat_gateway_public_ip_association" "nat_gateway_ip_association" {
  count                = var.create_nat_gateway ? 1 : 0
  
  nat_gateway_id       = azurerm_nat_gateway.nat_gateway[0].id
  public_ip_address_id = azurerm_public_ip.nat_gateway_ip[0].id
}

# Associate subnets with NAT Gateway
resource "azurerm_subnet_nat_gateway_association" "subnet_nat_association" {
  for_each = var.create_nat_gateway ? {
    for name, subnet in var.subnets : name => subnet
    if subnet.associate_with_nat_gateway
  } : {}
  
  subnet_id      = azurerm_subnet.subnets[each.key].id
  nat_gateway_id = azurerm_nat_gateway.nat_gateway[0].id
}

# Create Network Security Groups
resource "azurerm_network_security_group" "nsgs" {
  for_each = var.network_security_groups

  name                = each.key
  location            = azurerm_resource_group.network.location
  resource_group_name = azurerm_resource_group.network.name
  tags                = var.tags
}

# Create NSG rules
resource "azurerm_network_security_rule" "nsg_rules" {
  for_each = var.nsg_rules

  name                        = each.key
  priority                    = each.value.priority
  direction                   = each.value.direction
  access                      = each.value.access
  protocol                    = each.value.protocol
  source_port_range           = each.value.source_port_range
  destination_port_range      = each.value.destination_port_range
  source_address_prefix       = each.value.source_address_prefix
  destination_address_prefix  = each.value.destination_address_prefix
  resource_group_name         = azurerm_resource_group.network.name
  network_security_group_name = azurerm_network_security_group.nsgs[each.value.nsg_name].name
  
  # Use ranges instead of single values if provided
  source_port_ranges          = each.value.source_port_ranges
  destination_port_ranges     = each.value.destination_port_ranges
  source_address_prefixes     = each.value.source_address_prefixes
  destination_address_prefixes = each.value.destination_address_prefixes
}

# Associate NSGs with subnets
resource "azurerm_subnet_network_security_group_association" "nsg_associations" {
  for_each = {
    for pair in var.nsg_subnet_associations : "${pair.subnet_name}-${pair.nsg_name}" => pair
  }

  subnet_id                 = azurerm_subnet.subnets[each.value.subnet_name].id
  network_security_group_id = azurerm_network_security_group.nsgs[each.value.nsg_name].id
}

# Create route tables
resource "azurerm_route_table" "route_tables" {
  for_each = var.route_tables

  name                = each.key
  location            = azurerm_resource_group.network.location
  resource_group_name = azurerm_resource_group.network.name
  
  disable_bgp_route_propagation = each.value.disable_bgp_route_propagation
  tags                = var.tags
}

# Create routes
resource "azurerm_route" "routes" {
  for_each = var.routes

  name                = each.key
  resource_group_name = azurerm_resource_group.network.name
  route_table_name    = azurerm_route_table.route_tables[each.value.route_table_name].name
  address_prefix      = each.value.address_prefix
  next_hop_type       = each.value.next_hop_type
  next_hop_in_ip_address = each.value.next_hop_in_ip_address
}

# Associate route tables with subnets
resource "azurerm_subnet_route_table_association" "route_table_associations" {
  for_each = {
    for pair in var.route_table_subnet_associations : "${pair.subnet_name}-${pair.route_table_name}" => pair
  }

  subnet_id      = azurerm_subnet.subnets[each.value.subnet_name].id
  route_table_id = azurerm_route_table.route_tables[each.value.route_table_name].id
}

output "vnet_id" {
  value = azurerm_virtual_network.vnet.id
}

output "vnet_name" {
  value = azurerm_virtual_network.vnet.name
}

output "subnet_ids" {
  value = {
    for subnet_name, subnet in azurerm_subnet.subnets : subnet_name => subnet.id
  }
}

output "nsg_ids" {
  value = {
    for nsg_name, nsg in azurerm_network_security_group.nsgs : nsg_name => nsg.id
  }
}

output "nat_gateway_id" {
  value = var.create_nat_gateway ? azurerm_nat_gateway.nat_gateway[0].id : null
}

output "route_table_ids" {
  value = {
    for rt_name, rt in azurerm_route_table.route_tables : rt_name => rt.id
  }
}