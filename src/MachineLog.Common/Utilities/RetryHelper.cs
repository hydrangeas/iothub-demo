using MachineLog.Common.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MachineLog.Common.Utilities
{
  /// <summary>
  /// 再試行ロジックを提供するユーティリティクラス
  /// </summary>
  public static class RetryHelper
  {
    /// <summary>
    /// 指定された関数を再試行ポリシーに従って実行します
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="func">実行する関数</param>
    /// <param name="retryCount">再試行回数</param>
    /// <param name="retryIntervalMs">再試行間隔（ミリ秒）</param>
    /// <param name="exponentialBackoff">指数バックオフを使用するかどうか</param>
    /// <returns>関数の戻り値</returns>
    public static T ExecuteWithRetry<T>(
        Func<T> func,
        int retryCount = LogConstants.DefaultRetryCount,
        int retryIntervalMs = LogConstants.DefaultRetryIntervalMs,
        bool exponentialBackoff = true)
    {
      if (func == null)
      {
        throw new ArgumentNullException(nameof(func));
      }

      var exceptions = new System.Collections.Generic.List<Exception>();

      for (int retry = 0; retry <= retryCount; retry++)
      {
        try
        {
          return func();
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);

          if (retry >= retryCount)
          {
            throw new AggregateException($"操作が{retryCount}回の再試行後も失敗しました。", exceptions);
          }

          int delay = exponentialBackoff
              ? retryIntervalMs * (int)Math.Pow(2, retry)
              : retryIntervalMs;

          Thread.Sleep(delay);
        }
      }

      // ここには到達しないはずだが、コンパイラのために必要
      throw new InvalidOperationException("予期しない実行パス");
    }

    /// <summary>
    /// 指定された非同期関数を再試行ポリシーに従って実行します
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="func">実行する非同期関数</param>
    /// <param name="retryCount">再試行回数</param>
    /// <param name="retryIntervalMs">再試行間隔（ミリ秒）</param>
    /// <param name="exponentialBackoff">指数バックオフを使用するかどうか</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>関数の戻り値を含むタスク</returns>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> func,
        int retryCount = LogConstants.DefaultRetryCount,
        int retryIntervalMs = LogConstants.DefaultRetryIntervalMs,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
      if (func == null)
      {
        throw new ArgumentNullException(nameof(func));
      }

      var exceptions = new System.Collections.Generic.List<Exception>();

      for (int retry = 0; retry <= retryCount; retry++)
      {
        try
        {
          return await func().ConfigureAwait(false);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
          exceptions.Add(ex);

          if (retry >= retryCount)
          {
            throw new AggregateException($"操作が{retryCount}回の再試行後も失敗しました。", exceptions);
          }

          int delay = exponentialBackoff
              ? retryIntervalMs * (int)Math.Pow(2, retry)
              : retryIntervalMs;

          await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
      }

      // ここには到達しないはずだが、コンパイラのために必要
      throw new InvalidOperationException("予期しない実行パス");
    }

    /// <summary>
    /// 指定された非同期関数を再試行ポリシーに従って実行します（戻り値なし）
    /// </summary>
    /// <param name="func">実行する非同期関数</param>
    /// <param name="retryCount">再試行回数</param>
    /// <param name="retryIntervalMs">再試行間隔（ミリ秒）</param>
    /// <param name="exponentialBackoff">指数バックオフを使用するかどうか</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>完了を表すタスク</returns>
    public static async Task ExecuteWithRetryAsync(
        Func<Task> func,
        int retryCount = LogConstants.DefaultRetryCount,
        int retryIntervalMs = LogConstants.DefaultRetryIntervalMs,
        bool exponentialBackoff = true,
        CancellationToken cancellationToken = default)
    {
      if (func == null)
      {
        throw new ArgumentNullException(nameof(func));
      }

      var exceptions = new System.Collections.Generic.List<Exception>();

      for (int retry = 0; retry <= retryCount; retry++)
      {
        try
        {
          await func().ConfigureAwait(false);
          return;
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
          exceptions.Add(ex);

          if (retry >= retryCount)
          {
            throw new AggregateException($"操作が{retryCount}回の再試行後も失敗しました。", exceptions);
          }

          int delay = exponentialBackoff
              ? retryIntervalMs * (int)Math.Pow(2, retry)
              : retryIntervalMs;

          await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }
      }
    }
  }
}
