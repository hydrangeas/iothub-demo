using MachineLog.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineLog.Common.Utilities
{
  /// <summary>
  /// ログエントリとバッチのサイズ計算を提供するユーティリティクラス
  /// </summary>
  public static class LogSizeCalculator
  {
    /// <summary>
    /// ログエントリのサイズ（バイト単位）を計算します
    /// </summary>
    /// <param name="entry">サイズを計算するログエントリ</param>
    /// <returns>ログエントリのサイズ（バイト単位）</returns>
    public static int CalculateSize(LogEntry entry)
    {
      if (entry == null)
      {
        throw new ArgumentNullException(nameof(entry));
      }

      int size = 0;

      // TimeGenerated (8バイト)
      size += 8;

      // MachineId (文字列)
      size += CalculateStringSize(entry.MachineId);

      // Severity (列挙型、4バイト)
      size += 4;

      // EventId (整数、4バイト)
      size += 4;

      // Message (文字列)
      size += CalculateStringSize(entry.Message);

      // OperationId (文字列、null可)
      size += CalculateStringSize(entry.OperationId);

      // Tags (ディクショナリ)
      if (entry.Tags != null && entry.Tags.Count > 0)
      {
        foreach (var tag in entry.Tags)
        {
          size += CalculateStringSize(tag.Key);
          size += CalculateStringSize(tag.Value);
        }
      }

      return size;
    }

    /// <summary>
    /// ログバッチのサイズ（バイト単位）を計算します
    /// </summary>
    /// <param name="batch">サイズを計算するログバッチ</param>
    /// <returns>ログバッチのサイズ（バイト単位）</returns>
    public static int CalculateBatchSize(LogBatch batch)
    {
      if (batch == null)
      {
        throw new ArgumentNullException(nameof(batch));
      }

      int size = 0;

      // BatchId (16バイト)
      size += 16;

      // CreatedAt (8バイト)
      size += 8;

      // Entries (コレクション)
      if (batch.Entries != null && batch.Entries.Count > 0)
      {
        size += batch.Entries.Sum(CalculateSize);
      }

      return size;
    }

    /// <summary>
    /// 文字列のサイズ（バイト単位）を計算します
    /// </summary>
    /// <param name="value">サイズを計算する文字列</param>
    /// <returns>文字列のサイズ（バイト単位）</returns>
    private static int CalculateStringSize(string? value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return 0;
      }

      return Encoding.UTF8.GetByteCount(value);
    }

    /// <summary>
    /// ログエントリのコレクションをバッチに分割します
    /// </summary>
    /// <param name="entries">分割するログエントリのコレクション</param>
    /// <param name="maxBatchSizeBytes">最大バッチサイズ（バイト単位）</param>
    /// <param name="maxBatchEntries">最大バッチエントリ数</param>
    /// <returns>ログバッチのリスト</returns>
    public static List<LogBatch> CreateBatches(
        IEnumerable<LogEntry> entries,
        int maxBatchSizeBytes,
        int maxBatchEntries)
    {
      if (entries == null)
      {
        throw new ArgumentNullException(nameof(entries));
      }

      var batches = new List<LogBatch>();
      var currentBatch = new List<LogEntry>();
      int currentBatchSize = 0;

      foreach (var entry in entries)
      {
        int entrySize = CalculateSize(entry);

        // バッチがいっぱいになったら新しいバッチを作成
        if (currentBatch.Count >= maxBatchEntries ||
            currentBatchSize + entrySize > maxBatchSizeBytes)
        {
          if (currentBatch.Count > 0)
          {
            var batch = new LogBatch(currentBatch);
            batch.Size = currentBatchSize;
            batches.Add(batch);

            currentBatch = new List<LogEntry>();
            currentBatchSize = 0;
          }
        }

        currentBatch.Add(entry);
        currentBatchSize += entrySize;
      }

      // 残りのエントリをバッチに追加
      if (currentBatch.Count > 0)
      {
        var batch = new LogBatch(currentBatch);
        batch.Size = currentBatchSize;
        batches.Add(batch);
      }

      return batches;
    }
  }
}
