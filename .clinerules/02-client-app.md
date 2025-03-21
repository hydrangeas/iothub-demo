# MachineLog.Collector - クライアントアプリケーション

## アプリケーション構造

MachineLog.Collectorは.NET 8.0 Worker Serviceとして実装してください。アプリケーションは関心事の分離原則に従って構造化し、以下の主要コンポーネントを含めてください：

- **エントリポイント**: 依存性注入の設定、ホスト構成を行うプログラムクラス
- **ワーカーサービス**: バックグラウンドで実行される主要サービスクラス
- **サービスインターフェース**: 各機能を抽象化したインターフェース定義
- **サービス実装**: インターフェースの具体的な実装クラス
- **設定クラス**: アプリケーション設定を表現するクラス
- **ユーティリティクラス**: 共通機能を提供するヘルパークラス

## 主要機能要件

### ファイル監視機能

- FileSystemWatcherを使用したリアルタイムファイル変更検知
- 複数ファイルの同時監視対応
- ファイル読み取り位置の永続化（前回読み取り位置からの継続）
- ファイルロック対策（共有読み取りモード）
- 監視対象ディレクトリが存在しない場合の自動作成

### ログ処理機能

- JSON Lines形式のログ解析
- 16KB以下のレコードサイズ制限対応
- メタデータ付与（MachineId, TimeGenerated, Severity, EventId, Message, OperationId, Tags）
- UTF-8エンコーディング対応
- 不正な形式のJSONエントリの検出と処理

### ネットワーク回復力

- 指数バックオフを使用したリトライメカニズム
- ローカルバッファリング（接続障害時）
- 接続回復時の一括送信（最大1MB）
- 接続状態監視
- 送信失敗ログの永続化と再送

### ログローテーション

- 日次ログファイル作成
- サイズベースローテーション（50MB上限）
- 古いログファイルの自動アーカイブ
- 保持期間（7日）を超えたログの自動削除
- ディスク容量監視と空き容量確保

## C#実装上の考慮事項

### 非同期処理

- I/O操作（ファイル読み取り、API呼び出し）は非同期メソッドで実装
- `async`/`await`パターンを一貫して使用
- `Task.Run`の不必要な使用を避ける
- キャンセレーション対応（CancellationToken）
- 非同期ストリーム（IAsyncEnumerable）の活用

### スレッド安全性

- 共有リソースへの並行アクセスを適切に制御
- スレッドセーフなコレクション（ConcurrentQueue, ConcurrentDictionary）の使用
- ロック機構（SemaphoreSlim, lock）の適切な使用
- 不変オブジェクトの活用
- 競合状態（race condition）の回避

### リソース管理

- IDisposableの適切な実装（Dispose パターン）
- using文またはusingディレクティブによるリソース解放
- ファイルハンドルの適切な管理
- 非同期破棄（IAsyncDisposable）の実装
- ファイナライザーの適切な使用

### 例外処理

- 特定の例外タイプに対する具体的な処理
- グローバル例外ハンドラーの実装
- 構造化ログ記録（例外コンテキスト情報を含む）
- 致命的でない例外からの回復メカニズム
- 例外フィルタリングの活用

### パフォーマンス最適化

- ストリーム処理によるメモリ効率の向上
- バッファプーリングの使用
- 不必要なオブジェクト生成の回避
- 大量データ処理時のパフォーマンスボトルネック対策
- 適切なデータ構造の選択

### 設定管理

- IOptionsパターンを使用した設定の注入
- 環境別設定ファイル（appsettings.{Environment}.json）
- シークレット情報の安全な管理（User Secrets, KeyVault）
- 実行時の設定変更検知と再読み込み
- 設定の検証ロジック

## 設定項目

アプリケーションの設定ファイルには以下の項目を含めてください：

### Azure Monitor設定

- ワークスペースID
- クライアントID
- クライアントシークレット
- テナントID
- データ収集エンドポイント
- データ収集ルールID
- ストリーム名

### コレクター設定

- 機械ID
- ログディレクトリパス
- ログファイルパターン
- 最大バッチサイズ
- 最大バッチ数
- アップロード間隔
- リトライ回数
- リトライ基本間隔
- バッファディレクトリ
- ローテーション間隔
- 最大ファイルサイズ
- 保持日数

### ロギング設定

- ログレベル設定
- ログ出力先
