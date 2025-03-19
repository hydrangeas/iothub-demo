terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.0"
    }
  }
  backend "azurerm" {}
}

provider "azurerm" {
  features {}
}

provider "azuread" {}

# モジュールの呼び出し
module "azure_monitor" {
  source = "./modules/azure-monitor"

  resource_group_name   = var.resource_group_name
  location              = var.location
  environment           = var.environment
  log_analytics_sku     = var.log_analytics_sku
  log_retention_in_days = var.log_retention_in_days
  tags                  = var.tags
}

module "azure_storage" {
  source = "./modules/azure-storage"

  resource_group_name      = var.resource_group_name
  location                 = var.location
  environment              = var.environment
  storage_account_tier     = var.storage_account_tier
  storage_replication_type = var.storage_replication_type
  tags                     = var.tags
}

module "app_service" {
  source = "./modules/app-service"

  resource_group_name  = var.resource_group_name
  location             = var.location
  environment          = var.environment
  app_service_plan_sku = var.app_service_plan_sku
  tags                 = var.tags
}

module "entra_id" {
  source = "./modules/entra-id"

  application_name = "MachineLog"
  environment      = var.environment
}
