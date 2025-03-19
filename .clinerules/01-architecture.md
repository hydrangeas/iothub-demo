# システムアーキテクチャ

## 全体アーキテクチャ

```
[機械（クライアント）] → [MachineLog.Collector] → [Azure Monitor Logs] → [MachineLog.Monitor]
   ↓                                               ↓
[ローカルログファイル]                            [Azure Storage](長期保存用)
```

## コンポーネント構成

### MachineLog.Collector

- .NET 8.0コンソールアプリケーション
- Worker Serviceパターンを使用した長時間実行バックグラウンドサービス
- ファイルシステム監視機能
- ログ処理・変換機能
- Azure Monitor Logs APIとの連携
- ネットワーク回復力（リトライ、バッファリング）
- ログローテーション機能

### Azure Monitor Logs

- 中央ログリポジトリ
- Log Analyticsワークスペース
- クエリ機能
- データ保持ポリシー（365日）

### MachineLog.Monitor

- ASP.NET 8.0 Webアプリケーション
- Blazor WebAssemblyフロントエンド
- Azure Monitor Query APIとの連携
- ダッシュボード機能
- レポート生成機能
- アラート設定機能

### MachineLog.Common

- 共通モデル定義
- 共通ユーティリティ
- 検証ロジック

### MachineLog.Infrastructure

- Terraformを使用したインフラストラクチャコード
- Azure リソースのプロビジョニング
- 環境別設定（開発、テスト、本番）

## データフロー

1. 機械がログを生成し、ローカルファイルに保存
2. MachineLog.Collectorがファイル変更を検出
3. ログエントリを処理し、JSON Lines形式に変換
4. Azure Monitor Logs APIを使用してデータを送信
5. データはLog Analyticsワークスペースに保存
6. MachineLog.MonitorがAzure Monitor Query APIを使用してデータにアクセス
7. ユーザーはWebインターフェースを通じてデータを閲覧・分析

## 技術選定ポイント

- **非同期処理**: I/O操作を非同期で実行し、スレッドブロッキングを回避
- **キャンセレーション対応**: 長時間実行操作はCancellationTokenをサポート
- **メモリ効率**: 大量ログ処理時のメモリ使用量を最適化（ストリーム処理）
- **スレッドセーフ**: 並行アクセスに対応したスレッドセーフな実装
- **例外処理**: 堅牢な例外処理と適切なログ記録
- **設定の外部化**: 環境に応じた設定の外部化（appsettings.json）
- **テスト容易性**: 依存性注入とインターフェース分離によるテスト容易性
- **インフラストラクチャコード化**: Terraformを使用したインフラの再現性確保
