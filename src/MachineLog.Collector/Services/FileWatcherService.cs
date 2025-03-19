using MachineLog.Collector.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MachineLog.Collector.Services
{
  /// <summary>
  /// ファイル監視サービスの実装
  /// </summary>
  public class FileWatcherService : IFileWatcherService, IDisposable
  {
    private readonly ILogger<FileWatcherService> _logger;
    private readonly CollectorSettings _settings;
    private FileSystemWatcher? _watcher;
    private bool _disposed;

    /// <summary>
    /// ファイル変更イベント
    /// </summary>
    public event EventHandler<FileChangedEventArgs>? FileChanged;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="settings">設定</param>
    public FileWatcherService(
        ILogger<FileWatcherService> logger,
        IOptions<CollectorSettings> settings)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// 監視を開始します
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation("ファイル監視サービスを開始しています...");

      try
      {
        // 監視対象ディレクトリが存在しない場合は作成
        if (!Directory.Exists(_settings.LogDirectoryPath))
        {
          _logger.LogInformation("監視対象ディレクトリが存在しないため作成します: {DirectoryPath}", _settings.LogDirectoryPath);
          Directory.CreateDirectory(_settings.LogDirectoryPath);
        }

        // FileSystemWatcherの設定
        _watcher = new FileSystemWatcher
        {
          Path = _settings.LogDirectoryPath,
          Filter = _settings.LogFilePattern,
          NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
          EnableRaisingEvents = true
        };

        // イベントハンドラの登録
        _watcher.Created += OnFileChanged;
        _watcher.Changed += OnFileChanged;
        _watcher.Renamed += OnFileRenamed;
        _watcher.Deleted += OnFileChanged;

        _logger.LogInformation("ファイル監視サービスを開始しました。監視対象: {DirectoryPath}, パターン: {FilePattern}",
            _settings.LogDirectoryPath, _settings.LogFilePattern);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "ファイル監視サービスの開始中にエラーが発生しました");
        throw;
      }

      return Task.CompletedTask;
    }

    /// <summary>
    /// 監視を停止します
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation("ファイル監視サービスを停止しています...");

      try
      {
        if (_watcher != null)
        {
          _watcher.EnableRaisingEvents = false;
          _watcher.Created -= OnFileChanged;
          _watcher.Changed -= OnFileChanged;
          _watcher.Renamed -= OnFileRenamed;
          _watcher.Deleted -= OnFileChanged;
        }

        _logger.LogInformation("ファイル監視サービスを停止しました");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "ファイル監視サービスの停止中にエラーが発生しました");
        throw;
      }

      return Task.CompletedTask;
    }

    /// <summary>
    /// リソースを解放します
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// リソースを解放します
    /// </summary>
    /// <param name="disposing">マネージドリソースを解放するかどうか</param>
    protected virtual void Dispose(bool disposing)
    {
      if (_disposed)
      {
        return;
      }

      if (disposing)
      {
        _watcher?.Dispose();
      }

      _disposed = true;
    }

    /// <summary>
    /// ファイル変更イベントハンドラ
    /// </summary>
    /// <param name="sender">イベント発生元</param>
    /// <param name="e">イベント引数</param>
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
      var changeType = e.ChangeType switch
      {
        WatcherChangeTypes.Created => FileChangeType.Created,
        WatcherChangeTypes.Changed => FileChangeType.Changed,
        WatcherChangeTypes.Deleted => FileChangeType.Deleted,
        _ => FileChangeType.Changed
      };

      _logger.LogDebug("ファイル変更を検出しました: {FilePath}, 変更タイプ: {ChangeType}", e.FullPath, changeType);
      FileChanged?.Invoke(this, new FileChangedEventArgs(e.FullPath, changeType));
    }

    /// <summary>
    /// ファイル名変更イベントハンドラ
    /// </summary>
    /// <param name="sender">イベント発生元</param>
    /// <param name="e">イベント引数</param>
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
      _logger.LogDebug("ファイル名変更を検出しました: {OldPath} -> {NewPath}", e.OldFullPath, e.FullPath);
      FileChanged?.Invoke(this, new FileChangedEventArgs(e.FullPath, FileChangeType.Renamed));
    }
  }
}
