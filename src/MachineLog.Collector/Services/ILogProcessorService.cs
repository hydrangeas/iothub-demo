using MachineLog.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MachineLog.Collector.Services
{
  /// <summary>
  /// ログ処理サービスのインターフェース
  /// </summary>
  public interface ILogProcessorService
  {
    /// <summary>
    /// ファイルからログエントリを処理します
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>処理されたログエントリのコレクション</returns>
    Task<IEnumerable<LogEntry>> ProcessLogFileAsync(string filePath, CancellationToken cancellationToken);

    /// <summary>
    /// ログエントリをバッチに変換します
    /// </summary>
    /// <param name="entries">ログエントリのコレクション</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>ログバッチのコレクション</returns>
    Task<IEnumerable<LogBatch>> CreateBatchesAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken);

    /// <summary>
    /// ファイルの読み取り位置を保存します
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <param name="position">読み取り位置</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    Task SaveFilePositionAsync(string filePath, long position, CancellationToken cancellationToken);

    /// <summary>
    /// ファイルの読み取り位置を取得します
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>読み取り位置</returns>
    Task<long> GetFilePositionAsync(string filePath, CancellationToken cancellationToken);
  }
}
