# リソースグループ設定
resource_group_name = "rg-iothub-demo"
location            = "japaneast"
environment         = "dev"

# Log Analytics設定
log_analytics_sku     = "PerGB2018"
log_retention_in_days = 30

# ストレージアカウント設定
storage_account_tier     = "Standard"
storage_replication_type = "LRS"

# App Service設定
app_service_plan_sku = "B1"

# アラート設定
alert_email_address = "admin@example.com"

# タグ設定
tags = {
  Environment = "Development"
  Project     = "IoTHub Demo"
  Owner       = "DevOps Team"
  ManagedBy   = "Terraform"
}
