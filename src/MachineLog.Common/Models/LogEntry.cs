using System;
using System.Collections.Generic;

namespace MachineLog.Common.Models
{
  /// <summary>
  /// ログエントリを表現するモデルクラス
  /// </summary>
  public class LogEntry
  {
    /// <summary>
    /// ログ生成時刻
    /// </summary>
    public DateTime TimeGenerated { get; set; }

    /// <summary>
    /// 機械の一意識別子
    /// </summary>
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// ログの重要度
    /// </summary>
    public LogSeverity Severity { get; set; }

    /// <summary>
    /// イベントID
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// ログメッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 関連する操作の一意識別子
    /// </summary>
    public string? OperationId { get; set; }

    /// <summary>
    /// タグ情報
    /// </summary>
    public Dictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
  }

  /// <summary>
  /// ログの重要度を表す列挙型
  /// </summary>
  public enum LogSeverity
  {
    /// <summary>
    /// 詳細情報
    /// </summary>
    Verbose,

    /// <summary>
    /// デバッグ情報
    /// </summary>
    Debug,

    /// <summary>
    /// 情報
    /// </summary>
    Information,

    /// <summary>
    /// 警告
    /// </summary>
    Warning,

    /// <summary>
    /// エラー
    /// </summary>
    Error,

    /// <summary>
    /// 致命的エラー
    /// </summary>
    Critical
  }
}
