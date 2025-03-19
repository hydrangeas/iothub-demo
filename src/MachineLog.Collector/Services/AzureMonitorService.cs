using Azure.Core;
using Azure.Identity;
using MachineLog.Collector.Configuration;
using MachineLog.Common.Constants;
using MachineLog.Common.Models;
using MachineLog.Common.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MachineLog.Collector.Services
{
  /// <summary>
  /// Azure Monitor Logsサービスの実装
  /// </summary>
  public class AzureMonitorService : IAzureMonitorService
  {
    private readonly ILogger<AzureMonitorService> _logger;
    private readonly AzureMonitorSettings _settings;
    private readonly CollectorSettings _collectorSettings;
    private readonly HttpClient _httpClient;
    private readonly string _bufferDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="settings">Azure Monitor設定</param>
    /// <param name="collectorSettings">コレクター設定</param>
    /// <param name="httpClient">HTTPクライアント</param>
    public AzureMonitorService(
        ILogger<AzureMonitorService> logger,
        IOptions<AzureMonitorSettings> settings,
        IOptions<CollectorSettings> collectorSettings,
        HttpClient httpClient)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
      _collectorSettings = collectorSettings?.Value ?? throw new ArgumentNullException(nameof(collectorSettings));
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

      // バッファディレクトリの設定
      _bufferDirectory = Path.Combine(_collectorSettings.BufferDirectoryPath, "batches");
      Directory.CreateDirectory(_bufferDirectory);

      // JSONシリアライズオプションの設定
      _jsonOptions = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
      };
    }

    /// <summary>
    /// ログバッチをAzure Monitor Logsに送信します
    /// </summary>
    /// <param name="batches">送信するログバッチのコレクション</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    public async Task SendLogsAsync(IEnumerable<LogBatch> batches, CancellationToken cancellationToken)
    {
      if (batches == null || !batches.Any())
      {
        _logger.LogDebug("送信するログバッチがありません");
        return;
      }

      _logger.LogInformation("Azure Monitor Logsにログを送信します: バッチ数: {Count}", batches.Count());

      foreach (var batch in batches)
      {
        try
        {
          await RetryHelper.ExecuteWithRetryAsync(
              async () => await SendBatchAsync(batch, cancellationToken),
              _collectorSettings.RetryCount,
              _collectorSettings.RetryIntervalMs,
              true,
              cancellationToken);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "ログバッチの送信に失敗しました: BatchId: {BatchId}", batch.BatchId);

          // 送信に失敗したバッチをバッファに保存
          await BufferBatchAsync(batch, cancellationToken);
        }
      }
    }

    /// <summary>
    /// 送信に失敗したログバッチをバッファに保存します
    /// </summary>
    /// <param name="batch">保存するログバッチ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    public async Task BufferBatchAsync(LogBatch batch, CancellationToken cancellationToken)
    {
      if (batch == null)
      {
        throw new ArgumentNullException(nameof(batch));
      }

      var filePath = GetBatchFilePath(batch.BatchId.ToString());

      try
      {
        var json = JsonSerializer.Serialize(batch, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
        _logger.LogInformation("ログバッチをバッファに保存しました: BatchId: {BatchId}", batch.BatchId);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "ログバッチのバッファへの保存に失敗しました: BatchId: {BatchId}", batch.BatchId);
        throw;
      }
    }

    /// <summary>
    /// バッファからログバッチを読み込みます
    /// </summary>
    /// <param name="maxBatchCount">読み込む最大バッチ数</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>ログバッチのコレクション</returns>
    public async Task<IEnumerable<LogBatch>> ReadBufferedBatchesAsync(int maxBatchCount, CancellationToken cancellationToken)
    {
      var batches = new List<LogBatch>();

      try
      {
        var batchFiles = Directory.GetFiles(_bufferDirectory, "*.json")
            .OrderBy(f => new FileInfo(f).CreationTime)
            .Take(maxBatchCount);

        foreach (var file in batchFiles)
        {
          try
          {
            var json = await File.ReadAllTextAsync(file, cancellationToken);
            var batch = JsonSerializer.Deserialize<LogBatch>(json, _jsonOptions);
            if (batch != null)
            {
              batches.Add(batch);
            }
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "バッファからのログバッチの読み込みに失敗しました: {FilePath}", file);
          }
        }

        _logger.LogInformation("バッファからログバッチを読み込みました: 読み込み数: {Count}", batches.Count);
        return batches;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "バッファからのログバッチの読み込み中にエラーが発生しました");
        throw;
      }
    }

    /// <summary>
    /// バッファからログバッチを削除します
    /// </summary>
    /// <param name="batchId">削除するバッチのID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    public Task RemoveBufferedBatchAsync(string batchId, CancellationToken cancellationToken)
    {
      if (string.IsNullOrEmpty(batchId))
      {
        throw new ArgumentException("バッチIDが指定されていません", nameof(batchId));
      }

      var filePath = GetBatchFilePath(batchId);

      try
      {
        if (File.Exists(filePath))
        {
          File.Delete(filePath);
          _logger.LogInformation("バッファからログバッチを削除しました: BatchId: {BatchId}", batchId);
        }
        else
        {
          _logger.LogWarning("削除対象のログバッチがバッファに存在しません: BatchId: {BatchId}", batchId);
        }

        return Task.CompletedTask;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "バッファからのログバッチの削除に失敗しました: BatchId: {BatchId}", batchId);
        throw;
      }
    }

    /// <summary>
    /// バッチファイルのパスを取得します
    /// </summary>
    /// <param name="batchId">バッチID</param>
    /// <returns>バッチファイルのパス</returns>
    private string GetBatchFilePath(string batchId)
    {
      return Path.Combine(_bufferDirectory, $"{batchId}.json");
    }

    /// <summary>
    /// ログバッチを送信します
    /// </summary>
    /// <param name="batch">送信するログバッチ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    private async Task SendBatchAsync(LogBatch batch, CancellationToken cancellationToken)
    {
      // アクセストークンの取得
      var credential = new ClientSecretCredential(
          _settings.TenantId,
          _settings.ClientId,
          _settings.ClientSecret);

      var token = await credential.GetTokenAsync(
          new TokenRequestContext(new[] { "https://monitor.azure.com/.default" }),
          cancellationToken);

      // リクエストの構築
      var url = $"{_settings.DataCollectionEndpoint}/dataCollectionRules/{_settings.DataCollectionRuleId}/streams/{_settings.StreamName}?api-version={_settings.ApiVersion}";

      using var request = new HttpRequestMessage(HttpMethod.Post, url);
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

      // ログデータの準備
      var logData = batch.Entries.Select(entry => new Dictionary<string, object>
      {
        ["TimeGenerated"] = entry.TimeGenerated,
        ["MachineId"] = entry.MachineId,
        ["Severity"] = entry.Severity.ToString(),
        ["EventId"] = entry.EventId,
        ["Message"] = entry.Message,
        ["OperationId"] = entry.OperationId ?? string.Empty,
        ["Tags"] = entry.Tags
      });

      var content = JsonSerializer.Serialize(logData, _jsonOptions);
      request.Content = new StringContent(content, Encoding.UTF8, "application/json");

      // リクエストの送信
      var response = await _httpClient.SendAsync(request, cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"Azure Monitor Logsへのログ送信に失敗しました: StatusCode: {response.StatusCode}, Error: {errorContent}");
      }

      _logger.LogInformation("Azure Monitor Logsにログを送信しました: BatchId: {BatchId}, エントリ数: {Count}", batch.BatchId, batch.Entries.Count);
    }
  }
}
