# 機械ログ収集・分析システム

世界中に分散した20,000台の機械から出力されるログを収集し、中央で管理・分析するシステムです。

## システム概要

このシステムは、以下のコンポーネントで構成されています：

1. **MachineLog.Collector** - クライアント側ログ収集アプリケーション (.NET 8.0)
   - 機械から出力されるログファイルを監視し、Azure Monitor Logsに送信します
   - ネットワーク障害時にはローカルにバッファリングし、接続回復時に再送します
   - ログローテーション機能を備えています

2. **MachineLog.Monitor** - 監視用Webアプリケーション (ASP.NET 8.0)
   - Azure Monitor Logsからデータを取得し、ダッシュボードを提供します
   - ログの検索、フィルタリング、可視化機能を備えています

3. **MachineLog.Common** - 共通コードライブラリ (.NET 8.0)
   - 両アプリケーションで共有されるモデル、ユーティリティ、検証ロジックを提供します

4. **MachineLog.Infrastructure** - インフラストラクチャコード
   - Terraformを使用してAzureリソースをコード化しています

## アーキテクチャ

```
[機械（クライアント）] → [MachineLog.Collector] → [Azure Monitor Logs] → [MachineLog.Monitor]
   ↓                                               ↓
[ローカルログファイル]                            [Azure Storage](長期保存用)
```

## 前提条件

- .NET 8.0 SDK
- Azure サブスクリプション
- Azure CLI
- Terraform (インフラストラクチャのデプロイ用)

## セットアップ手順

### 1. リポジトリのクローン

```bash
git clone https://github.com/yourusername/machinelog.git
cd machinelog
```

### 2. 設定ファイルの編集

`src/MachineLog.Collector/appsettings.json` を編集し、Azure Monitor Logsの接続情報を設定します：

```json
"AzureMonitor": {
  "WorkspaceId": "YOUR_WORKSPACE_ID",
  "ClientId": "YOUR_CLIENT_ID",
  "ClientSecret": "YOUR_CLIENT_SECRET",
  "TenantId": "YOUR_TENANT_ID",
  "DataCollectionEndpoint": "https://YOUR_DCE_NAME-YOUR_DCR_IMMUTABLEID.eastus-1.ingest.monitor.azure.com",
  "DataCollectionRuleId": "dcr-YOUR_DCR_ID",
  "StreamName": "Custom-MachineLogStream",
  "ApiVersion": "2022-10-01"
}
```

### 3. インフラストラクチャのデプロイ

```bash
cd src/MachineLog.Infrastructure/environments/dev
terraform init
terraform plan
terraform apply
```

### 4. アプリケーションのビルドと実行

```bash
# ソリューションのビルド
dotnet build

# コレクターの実行
cd src/MachineLog.Collector
dotnet run

# モニターの実行
cd src/MachineLog.Monitor
dotnet run
```

## 開発ガイドライン

- Microsoft C# コーディング規約に従ってください
- 新機能の追加時には単体テストを作成してください
- コミット前に `dotnet format` を実行してください

## ライセンス

MIT

## 貢献

プルリクエストを歓迎します。大きな変更を加える場合は、まずissueを作成して変更内容を議論してください。
