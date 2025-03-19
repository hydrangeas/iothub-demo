namespace MachineLog.Collector.Configuration
{
  /// <summary>
  /// コレクターの設定を表すクラス
  /// </summary>
  public class CollectorSettings
  {
    /// <summary>
    /// 機械ID
    /// </summary>
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// ログディレクトリパス
    /// </summary>
    public string LogDirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// ログファイルパターン
    /// </summary>
    public string LogFilePattern { get; set; } = "*.log";

    /// <summary>
    /// 最大バッチサイズ（バイト単位）
    /// </summary>
    public int MaxBatchSizeBytes { get; set; } = 1048576; // 1MB

    /// <summary>
    /// 最大バッチ数
    /// </summary>
    public int MaxBatchCount { get; set; } = 100;

    /// <summary>
    /// アップロード間隔（ミリ秒）
    /// </summary>
    public int UploadIntervalMs { get; set; } = 5000; // 5秒

    /// <summary>
    /// リトライ回数
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// リトライ基本間隔（ミリ秒）
    /// </summary>
    public int RetryIntervalMs { get; set; } = 1000; // 1秒

    /// <summary>
    /// バッファディレクトリ
    /// </summary>
    public string BufferDirectoryPath { get; set; } = "buffer";

    /// <summary>
    /// ローテーション間隔（日数）
    /// </summary>
    public int RotationIntervalDays { get; set; } = 1;

    /// <summary>
    /// 最大ファイルサイズ（バイト単位）
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 52428800; // 50MB

    /// <summary>
    /// 保持日数
    /// </summary>
    public int RetentionDays { get; set; } = 7;
  }
}
