# MachineLog.Common - 共通ライブラリ

## ライブラリ構造

MachineLog.Commonは.NET 8.0クラスライブラリとして実装し、クライアントアプリケーションとWebアプリケーション間で共有される共通コードを含めてください。ライブラリは以下の主要コンポーネントを含む構造で設計してください：

- **モデル**: データモデルを定義するクラス
- **検証**: モデルの検証ロジックを提供するクラス
- **拡張メソッド**: 共通の拡張機能を提供するクラス
- **ユーティリティ**: 共通ユーティリティ機能を提供するクラス
- **定数**: システム全体で使用される定数を定義するクラス

## モデル定義

### ログエントリモデル

ログエントリを表現するモデルクラスを実装してください。このモデルは以下のプロパティを含みます：

- TimeGenerated: ログ生成時刻（DateTime）
- MachineId: 機械の一意識別子（string）
- Severity: ログの重要度（enum）
- EventId: イベントID（int）
- Message: ログメッセージ（string）
- OperationId: 関連する操作の一意識別子（string）
- Tags: タグ情報（Dictionary<string, string>）
  - Code: エラーコードなど
  - Parameter: 関連パラメータ
  - Info1: 追加情報1
  - Info2: 追加情報2
  - Info3: 追加情報3

### ログバッチモデル

複数のログエントリをバッチとして表現するモデルクラスを実装してください。このモデルは以下のプロパティを含みます：

- BatchId: バッチの一意識別子（Guid）
- CreatedAt: バッチ作成時刻（DateTime）
- Entries: ログエントリのコレクション（IReadOnlyCollection<LogEntry>）
- Size: バッチのサイズ（バイト単位）（int）

## 検証ロジック

FluentValidationを使用して、モデルの検証ロジックを実装してください。以下の検証ルールを含めます：

### ログエントリ検証

- TimeGenerated: 必須、未来の日付は不可
- MachineId: 必須、最大長50文字
- Severity: 必須、有効な列挙値
- Message: 必須、最大長8000文字
- EventId: 必須
- Tags: 各値の最大長1000文字

### ログバッチ検証

- BatchId: 必須、空でないGuid
- CreatedAt: 必須、未来の日付は不可
- Entries: 必須、少なくとも1つのエントリを含む、最大1000エントリ
- Size: 1MB（1,048,576バイト）以下

## 拡張メソッド

以下の拡張メソッドを実装してください：

### 文字列拡張

- TruncateIfNeeded: 文字列が指定された長さを超える場合に切り詰める
- ToSafeJson: 文字列をJSON安全な形式に変換
- IsValidJson: 文字列が有効なJSONかどうかを検証

### 日時拡張

- ToIso8601String: DateTimeをISO 8601形式の文字列に変換
- FromIso8601String: ISO 8601形式の文字列からDateTimeに変換

### コレクション拡張

- Batch: コレクションを指定サイズのバッチに分割
- SafeAny: nullチェック付きのAny拡張メソッド

## ユーティリティクラス

以下のユーティリティクラスを実装してください：

### JsonHelper

- Serialize: オブジェクトをJSON文字列にシリアライズ
- Deserialize: JSON文字列からオブジェクトにデシリアライズ
- IsValidJson: 文字列が有効なJSONかどうかを検証
- MergeJsonObjects: 複数のJSONオブジェクトをマージ

### RetryHelper

- ExecuteWithRetry: 指定された関数を再試行ポリシーに従って実行
- ExecuteWithRetryAsync: 非同期関数版のExecuteWithRetry

### LogSizeCalculator

- CalculateSize: ログエントリのサイズ（バイト単位）を計算
- CalculateBatchSize: ログバッチのサイズを計算

## 定数定義

以下の定数クラスを実装してください：

### LogConstants

- 最大メッセージ長
- 最大タグ値長
- 最大バッチサイズ
- 最大バッチエントリ数
- デフォルトリトライ回数
- デフォルトリトライ間隔

### ApiConstants

- APIエンドポイントパス
- APIバージョン
- デフォルトタイムアウト値

## C#実装上の考慮事項

### 型の安全性

- nullableリファレンス型の活用
- 適切な型の使用（string vs. Guid vs. 専用の型）
- パターンマッチングの活用

### イミュータビリティ

- 不変オブジェクトの設計
- レコード型の活用
- 初期化専用プロパティの使用

### パフォーマンス

- 構造体の適切な使用
- メモリ割り当ての最小化
- 文字列連結の最適化（StringBuilder）

### コード品質

- コード分析ツールの設定（.editorconfig）
- XMLドキュメントコメントの充実
- 単体テストの網羅性
