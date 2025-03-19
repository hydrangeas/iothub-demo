using MachineLog.Common.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MachineLog.Collector.Services
{
  /// <summary>
  /// Azure Monitor Logsサービスのインターフェース
  /// </summary>
  public interface IAzureMonitorService
  {
    /// <summary>
    /// ログバッチをAzure Monitor Logsに送信します
    /// </summary>
    /// <param name="batches">送信するログバッチのコレクション</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    Task SendLogsAsync(IEnumerable<LogBatch> batches, CancellationToken cancellationToken);

    /// <summary>
    /// 送信に失敗したログバッチをバッファに保存します
    /// </summary>
    /// <param name="batch">保存するログバッチ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    Task BufferBatchAsync(LogBatch batch, CancellationToken cancellationToken);

    /// <summary>
    /// バッファからログバッチを読み込みます
    /// </summary>
    /// <param name="maxBatchCount">読み込む最大バッチ数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>ログバッチのコレクション</returns>
    Task<IEnumerable<LogBatch>> ReadBufferedBatchesAsync(int maxBatchCount, CancellationToken cancellationToken);

    /// <summary>
    /// バッファからログバッチを削除します
    /// </summary>
    /// <param name="batchId">削除するバッチのID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    Task RemoveBufferedBatchAsync(string batchId, CancellationToken cancellationToken);
  }
}
