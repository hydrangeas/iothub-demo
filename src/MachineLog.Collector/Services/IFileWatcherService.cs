using System;
using System.Threading;
using System.Threading.Tasks;

namespace MachineLog.Collector.Services
{
  /// <summary>
  /// ファイル監視サービスのインターフェース
  /// </summary>
  public interface IFileWatcherService
  {
    /// <summary>
    /// ファイル変更イベント
    /// </summary>
    event EventHandler<FileChangedEventArgs> FileChanged;

    /// <summary>
    /// 監視を開始します
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 監視を停止します
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    Task StopAsync(CancellationToken cancellationToken);
  }

  /// <summary>
  /// ファイル変更イベントの引数
  /// </summary>
  public class FileChangedEventArgs : EventArgs
  {
    /// <summary>
    /// ファイルパス
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// 変更タイプ
    /// </summary>
    public FileChangeType ChangeType { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <param name="changeType">変更タイプ</param>
    public FileChangedEventArgs(string filePath, FileChangeType changeType)
    {
      FilePath = filePath;
      ChangeType = changeType;
    }
  }

  /// <summary>
  /// ファイル変更タイプ
  /// </summary>
  public enum FileChangeType
  {
    /// <summary>
    /// 作成
    /// </summary>
    Created,

    /// <summary>
    /// 変更
    /// </summary>
    Changed,

    /// <summary>
    /// 名前変更
    /// </summary>
    Renamed,

    /// <summary>
    /// 削除
    /// </summary>
    Deleted
  }
}
