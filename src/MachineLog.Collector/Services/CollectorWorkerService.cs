using MachineLog.Collector.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MachineLog.Collector.Services
{
  /// <summary>
  /// コレクターのワーカーサービス
  /// </summary>
  public class CollectorWorkerService : BackgroundService
  {
    private readonly ILogger<CollectorWorkerService> _logger;
    private readonly CollectorSettings _settings;
    private readonly IFileWatcherService _fileWatcherService;
    private readonly ILogProcessorService _logProcessorService;
    private readonly IAzureMonitorService _azureMonitorService;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="settings">設定</param>
    /// <param name="fileWatcherService">ファイル監視サービス</param>
    /// <param name="logProcessorService">ログ処理サービス</param>
    /// <param name="azureMonitorService">Azure Monitorサービス</param>
    public CollectorWorkerService(
        ILogger<CollectorWorkerService> logger,
        IOptions<CollectorSettings> settings,
        IFileWatcherService fileWatcherService,
        ILogProcessorService logProcessorService,
        IAzureMonitorService azureMonitorService)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
      _fileWatcherService = fileWatcherService ?? throw new ArgumentNullException(nameof(fileWatcherService));
      _logProcessorService = logProcessorService ?? throw new ArgumentNullException(nameof(logProcessorService));
      _azureMonitorService = azureMonitorService ?? throw new ArgumentNullException(nameof(azureMonitorService));
    }

    /// <summary>
    /// サービスを開始します
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation("コレクターサービスを開始しています...");

      // ファイル変更イベントハンドラを登録
      _fileWatcherService.FileChanged += OnFileChanged;

      // ファイル監視を開始
      await _fileWatcherService.StartAsync(cancellationToken);

      // バッファされたログの送信を試行
      await TrySendBufferedLogsAsync(cancellationToken);

      await base.StartAsync(cancellationToken);
      _logger.LogInformation("コレクターサービスを開始しました");
    }

    /// <summary>
    /// サービスを停止します
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
      _logger.LogInformation("コレクターサービスを停止しています...");

      // ファイル変更イベントハンドラを解除
      _fileWatcherService.FileChanged -= OnFileChanged;

      // ファイル監視を停止
      await _fileWatcherService.StopAsync(cancellationToken);

      await base.StopAsync(cancellationToken);
      _logger.LogInformation("コレクターサービスを停止しました");
    }

    /// <summary>
    /// バックグラウンド処理を実行します
    /// </summary>
    /// <param name="stoppingToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("バックグラウンド処理を開始しました");

      try
      {
        // 定期的にバッファされたログの送信を試行
        while (!stoppingToken.IsCancellationRequested)
        {
          await Task.Delay(TimeSpan.FromMilliseconds(_settings.UploadIntervalMs), stoppingToken);
          await TrySendBufferedLogsAsync(stoppingToken);
        }
      }
      catch (OperationCanceledException)
      {
        _logger.LogInformation("バックグラウンド処理がキャンセルされました");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "バックグラウンド処理中にエラーが発生しました");
        throw;
      }
    }

    /// <summary>
    /// ファイル変更イベントハンドラ
    /// </summary>
    /// <param name="sender">イベント発生元</param>
    /// <param name="e">イベント引数</param>
    private async void OnFileChanged(object? sender, FileChangedEventArgs e)
    {
      try
      {
        // 作成または変更の場合のみ処理
        if (e.ChangeType == FileChangeType.Created || e.ChangeType == FileChangeType.Changed)
        {
          _logger.LogDebug("ファイル変更を検出しました: {FilePath}, 変更タイプ: {ChangeType}", e.FilePath, e.ChangeType);

          // ファイルからログエントリを処理
          var entries = await _logProcessorService.ProcessLogFileAsync(e.FilePath, CancellationToken.None);
          if (entries.Any())
          {
            // ログエントリをバッチに変換
            var batches = await _logProcessorService.CreateBatchesAsync(entries, CancellationToken.None);

            // バッチをAzure Monitor Logsに送信
            await _azureMonitorService.SendLogsAsync(batches, CancellationToken.None);
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "ファイル変更の処理中にエラーが発生しました: {FilePath}", e.FilePath);
      }
    }

    /// <summary>
    /// バッファされたログの送信を試行します
    /// </summary>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    private async Task TrySendBufferedLogsAsync(CancellationToken cancellationToken)
    {
      try
      {
        // バッファからログバッチを読み込み
        var batches = await _azureMonitorService.ReadBufferedBatchesAsync(_settings.MaxBatchCount, cancellationToken);
        if (!batches.Any())
        {
          return;
        }

        _logger.LogInformation("バッファされたログバッチの送信を試行します: バッチ数: {Count}", batches.Count());

        foreach (var batch in batches)
        {
          try
          {
            // バッチを送信
            await _azureMonitorService.SendLogsAsync(new[] { batch }, cancellationToken);

            // 送信成功したバッチをバッファから削除
            await _azureMonitorService.RemoveBufferedBatchAsync(batch.BatchId.ToString(), cancellationToken);
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "バッファされたログバッチの送信に失敗しました: BatchId: {BatchId}", batch.BatchId);
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "バッファされたログの送信試行中にエラーが発生しました");
      }
    }
  }
}
