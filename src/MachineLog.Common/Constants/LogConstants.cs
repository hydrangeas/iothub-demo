namespace MachineLog.Common.Constants
{
  /// <summary>
  /// ログ関連の定数を定義するクラス
  /// </summary>
  public static class LogConstants
  {
    /// <summary>
    /// 最大メッセージ長
    /// </summary>
    public const int MaxMessageLength = 8000;

    /// <summary>
    /// 最大タグ値長
    /// </summary>
    public const int MaxTagValueLength = 1000;

    /// <summary>
    /// 最大バッチサイズ（バイト）
    /// </summary>
    public const int MaxBatchSizeBytes = 1048576; // 1MB

    /// <summary>
    /// 最大バッチエントリ数
    /// </summary>
    public const int MaxBatchEntries = 1000;

    /// <summary>
    /// デフォルトリトライ回数
    /// </summary>
    public const int DefaultRetryCount = 3;

    /// <summary>
    /// デフォルトリトライ間隔（ミリ秒）
    /// </summary>
    public const int DefaultRetryIntervalMs = 1000;

    /// <summary>
    /// 最大ファイルサイズ（バイト）
    /// </summary>
    public const long MaxFileSizeBytes = 52428800; // 50MB

    /// <summary>
    /// デフォルト保持日数
    /// </summary>
    public const int DefaultRetentionDays = 7;
  }
}
