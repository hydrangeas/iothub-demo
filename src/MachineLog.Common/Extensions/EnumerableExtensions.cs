using MachineLog.Common.Constants;
using MachineLog.Common.Models;
using MachineLog.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MachineLog.Common.Extensions
{
  /// <summary>
  /// IEnumerableに対する拡張メソッドを提供するクラス
  /// </summary>
  public static class EnumerableExtensions
  {
    /// <summary>
    /// コレクションを指定サイズのバッチに分割します
    /// </summary>
    /// <typeparam name="T">コレクションの要素の型</typeparam>
    /// <param name="source">分割するコレクション</param>
    /// <param name="batchSize">バッチサイズ</param>
    /// <returns>バッチのコレクション</returns>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      if (batchSize <= 0)
      {
        throw new ArgumentException("バッチサイズは1以上である必要があります。", nameof(batchSize));
      }

      using (var enumerator = source.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          yield return GetBatch(enumerator, batchSize);
        }
      }
    }

    /// <summary>
    /// nullチェック付きのAny拡張メソッド
    /// </summary>
    /// <typeparam name="T">コレクションの要素の型</typeparam>
    /// <param name="source">検査するコレクション</param>
    /// <returns>コレクションがnullでなく、少なくとも1つの要素を含む場合はtrue、それ以外はfalse</returns>
    public static bool SafeAny<T>(this IEnumerable<T>? source)
    {
      return source != null && source.Any();
    }

    /// <summary>
    /// nullチェック付きのAny拡張メソッド（条件付き）
    /// </summary>
    /// <typeparam name="T">コレクションの要素の型</typeparam>
    /// <param name="source">検査するコレクション</param>
    /// <param name="predicate">各要素が満たすべき条件</param>
    /// <returns>コレクションがnullでなく、条件を満たす要素が少なくとも1つある場合はtrue、それ以外はfalse</returns>
    public static bool SafeAny<T>(this IEnumerable<T>? source, Func<T, bool> predicate)
    {
      return source != null && source.Any(predicate);
    }

    /// <summary>
    /// ログエントリのコレクションをバッチに変換します
    /// </summary>
    /// <param name="entries">ログエントリのコレクション</param>
    /// <param name="maxBatchSizeBytes">最大バッチサイズ（バイト単位）</param>
    /// <param name="maxBatchEntries">最大バッチエントリ数</param>
    /// <returns>ログバッチのコレクション</returns>
    public static IEnumerable<LogBatch> ToBatches(
        this IEnumerable<LogEntry> entries,
        int maxBatchSizeBytes = LogConstants.MaxBatchSizeBytes,
        int maxBatchEntries = LogConstants.MaxBatchEntries)
    {
      return LogSizeCalculator.CreateBatches(entries, maxBatchSizeBytes, maxBatchEntries);
    }

    /// <summary>
    /// エンコーダから次のバッチを取得します
    /// </summary>
    /// <typeparam name="T">コレクションの要素の型</typeparam>
    /// <param name="enumerator">コレクションのエンコーダ</param>
    /// <param name="batchSize">バッチサイズ</param>
    /// <returns>バッチ</returns>
    private static IEnumerable<T> GetBatch<T>(IEnumerator<T> enumerator, int batchSize)
    {
      yield return enumerator.Current;

      for (int i = 1; i < batchSize && enumerator.MoveNext(); i++)
      {
        yield return enumerator.Current;
      }
    }
  }
}
