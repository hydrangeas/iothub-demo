using MachineLog.Collector.Configuration;
using MachineLog.Common.Constants;
using MachineLog.Common.Extensions;
using MachineLog.Common.Models;
using MachineLog.Common.Utilities;
using MachineLog.Common.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MachineLog.Collector.Services
{
  /// <summary>
  /// ログ処理サービスの実装
  /// </summary>
  public class LogProcessorService : ILogProcessorService
  {
    private readonly ILogger<LogProcessorService> _logger;
    private readonly CollectorSettings _settings;
    private readonly LogEntryValidator _logEntryValidator;
    private readonly string _positionsDirectory;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="settings">設定</param>
    /// <param name="logEntryValidator">ログエントリバリデータ</param>
    public LogProcessorService(
        ILogger<LogProcessorService> logger,
        IOptions<CollectorSettings> settings,
        LogEntryValidator logEntryValidator)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
      _logEntryValidator = logEntryValidator ?? throw new ArgumentNullException(nameof(logEntryValidator));

      // 位置情報保存ディレクトリの設定
      _positionsDirectory = Path.Combine(_settings.BufferDirectoryPath, "positions");
      Directory.CreateDirectory(_positionsDirectory);
    }

    /// <summary>
    /// ファイルからログエントリを処理します
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>処理されたログエントリのコレクション</returns>
    public async Task<IEnumerable<LogEntry>> ProcessLogFileAsync(string filePath, CancellationToken cancellationToken)
    {
      _logger.LogDebug("ログファイルの処理を開始します: {FilePath}", filePath);

      var entries = new List<LogEntry>();

      try
      {
        // 前回の読み取り位置を取得
        long position = await GetFilePositionAsync(filePath, cancellationToken);

        // ファイルが存在しない場合は空のリストを返す
        if (!File.Exists(filePath))
        {
          _logger.LogWarning("ログファイルが存在しません: {FilePath}", filePath);
          return entries;
        }

        // ファイルを開いて読み取り
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
          // 前回の位置から読み取り開始
          if (position > 0 && position < fileStream.Length)
          {
            fileStream.Seek(position, SeekOrigin.Begin);
          }

          using (var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true))
          {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null && !cancellationToken.IsCancellationRequested)
            {
              if (string.IsNullOrWhiteSpace(line))
              {
                continue;
              }

              try
              {
                // JSON形式のログエントリを解析
                if (line.IsValidJson())
                {
                  var entry = JsonSerializer.Deserialize<LogEntry>(line);
                  if (entry != null)
                  {
                    // 機械IDを設定
                    if (string.IsNullOrEmpty(entry.MachineId))
                    {
                      entry.MachineId = _settings.MachineId;
                    }

                    // バリデーション
                    var validationResult = await _logEntryValidator.ValidateAsync(entry, cancellationToken);
                    if (validationResult.IsValid)
                    {
                      entries.Add(entry);
                    }
                    else
                    {
                      _logger.LogWarning("無効なログエントリ: {Errors}", string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
                    }
                  }
                }
                else
                {
                  _logger.LogWarning("無効なJSON形式のログエントリ: {Line}", line.TruncateIfNeeded(100));
                }
              }
              catch (Exception ex)
              {
                _logger.LogError(ex, "ログエントリの処理中にエラーが発生しました: {Line}", line.TruncateIfNeeded(100));
              }
            }

            // 現在の位置を保存
            await SaveFilePositionAsync(filePath, fileStream.Position, cancellationToken);
          }
        }

        _logger.LogInformation("ログファイルの処理が完了しました: {FilePath}, エントリ数: {Count}", filePath, entries.Count);
        return entries;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "ログファイルの処理中にエラーが発生しました: {FilePath}", filePath);
        throw;
      }
    }

    /// <summary>
    /// ログエントリをバッチに変換します
    /// </summary>
    /// <param name="entries">ログエントリのコレクション</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>ログバッチのコレクション</returns>
    public Task<IEnumerable<LogBatch>> CreateBatchesAsync(IEnumerable<LogEntry> entries, CancellationToken cancellationToken)
    {
      _logger.LogDebug("ログエントリをバッチに変換します: エントリ数: {Count}", entries.Count());

      try
      {
        // バッチに変換
        var batches = entries.ToBatches(
            _settings.MaxBatchSizeBytes,
            LogConstants.MaxBatchEntries).ToList();

        _logger.LogInformation("ログバッチの作成が完了しました: バッチ数: {Count}", batches.Count);
        return Task.FromResult<IEnumerable<LogBatch>>(batches);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "ログバッチの作成中にエラーが発生しました");
        throw;
      }
    }

    /// <summary>
    /// ファイルの読み取り位置を保存します
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <param name="position">読み取り位置</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    public async Task SaveFilePositionAsync(string filePath, long position, CancellationToken cancellationToken)
    {
      var positionFilePath = GetPositionFilePath(filePath);

      try
      {
        await File.WriteAllTextAsync(positionFilePath, position.ToString(), cancellationToken);
        _logger.LogDebug("ファイル位置を保存しました: {FilePath}, 位置: {Position}", filePath, position);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "ファイル位置の保存中にエラーが発生しました: {FilePath}", filePath);
        throw;
      }
    }

    /// <summary>
    /// ファイルの読み取り位置を取得します
    /// </summary>
    /// <param name="filePath">ファイルパス</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>読み取り位置</returns>
    public async Task<long> GetFilePositionAsync(string filePath, CancellationToken cancellationToken)
    {
      var positionFilePath = GetPositionFilePath(filePath);

      try
      {
        if (File.Exists(positionFilePath))
        {
          var positionText = await File.ReadAllTextAsync(positionFilePath, cancellationToken);
          if (long.TryParse(positionText, out long position))
          {
            _logger.LogDebug("ファイル位置を読み込みました: {FilePath}, 位置: {Position}", filePath, position);
            return position;
          }
        }

        _logger.LogDebug("ファイル位置が見つからないため、0を返します: {FilePath}", filePath);
        return 0;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "ファイル位置の読み込み中にエラーが発生しました: {FilePath}", filePath);
        return 0;
      }
    }

    /// <summary>
    /// 位置情報ファイルのパスを取得します
    /// </summary>
    /// <param name="filePath">元のファイルパス</param>
    /// <returns>位置情報ファイルのパス</returns>
    private string GetPositionFilePath(string filePath)
    {
      // ファイルパスをハッシュ化して位置情報ファイル名とする
      var fileNameHash = filePath.GetHashCode().ToString("X8");
      return Path.Combine(_positionsDirectory, $"{fileNameHash}.pos");
    }
  }
}
