using System;
using System.Collections.Generic;

namespace MachineLog.Common.Models
{
  /// <summary>
  /// 複数のログエントリをバッチとして表現するモデルクラス
  /// </summary>
  public class LogBatch
  {
    /// <summary>
    /// バッチの一意識別子
    /// </summary>
    public Guid BatchId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// バッチ作成時刻
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ログエントリのコレクション
    /// </summary>
    public IReadOnlyCollection<LogEntry> Entries { get; }

    /// <summary>
    /// バッチのサイズ（バイト単位）
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="entries">ログエントリのコレクション</param>
    public LogBatch(IReadOnlyCollection<LogEntry> entries)
    {
      Entries = entries ?? throw new ArgumentNullException(nameof(entries));
    }
  }
}
