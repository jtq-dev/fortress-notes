terraform {
  backend "azurerm" {
    # Fill these in once you create a storage account + container for state
    resource_group_name  = "tfstate-rg"
    storage_account_name = "YOURTFSTATEACCOUNT"
    container_name       = "tfstate"
    key                  = "fortress-${terraform.workspace}.tfstate"
  }
}

module "rg" {
  source   = "../modules/rg"
  name     = "fortress-${terraform.workspace}-rg"
  location = "canadacentral"
}

output "rg_name" { value = module.rg.name }
