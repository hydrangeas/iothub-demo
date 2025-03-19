namespace MachineLog.Common.Constants
{
  /// <summary>
  /// API関連の定数を定義するクラス
  /// </summary>
  public static class ApiConstants
  {
    /// <summary>
    /// APIエンドポイントパス
    /// </summary>
    public static class Endpoints
    {
      /// <summary>
      /// ログAPI基本パス
      /// </summary>
      public const string LogBase = "/api/logs";

      /// <summary>
      /// ログバッチアップロードパス
      /// </summary>
      public const string LogBatchUpload = "/api/logs/batch";

      /// <summary>
      /// 機械ステータスAPI基本パス
      /// </summary>
      public const string MachineStatusBase = "/api/machines";

      /// <summary>
      /// 機械ステータス取得パス
      /// </summary>
      public const string MachineStatusGet = "/api/machines/{machineId}/status";

      /// <summary>
      /// ヘルスチェックパス
      /// </summary>
      public const string Health = "/health";
    }

    /// <summary>
    /// APIバージョン
    /// </summary>
    public static class Versions
    {
      /// <summary>
      /// 現在のAPIバージョン
      /// </summary>
      public const string Current = "1.0";

      /// <summary>
      /// Log Analytics APIバージョン
      /// </summary>
      public const string LogAnalytics = "2022-10-01";
    }

    /// <summary>
    /// デフォルトタイムアウト値（ミリ秒）
    /// </summary>
    public const int DefaultTimeoutMs = 30000; // 30秒

    /// <summary>
    /// 最大ページサイズ
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// デフォルトページサイズ
    /// </summary>
    public const int DefaultPageSize = 20;
  }
}
