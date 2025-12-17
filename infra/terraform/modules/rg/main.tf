terraform {
  required_version = ">= 1.6.0"
  required_providers {
    azurerm = { source = "hashicorp/azurerm", version = "~> 3.0" }
  }
}
provider "azurerm" { features {} }

variable "name" { type = string }
variable "location" { type = string }

resource "azurerm_resource_group" "rg" {
  name     = var.name
  location = var.location
}

output "name" { value = azurerm_resource_group.rg.name }
output "location" { value = azurerm_resource_group.rg.location }
