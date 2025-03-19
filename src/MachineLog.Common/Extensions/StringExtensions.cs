using MachineLog.Common.Utilities;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace MachineLog.Common.Extensions
{
  /// <summary>
  /// 文字列に対する拡張メソッドを提供するクラス
  /// </summary>
  public static class StringExtensions
  {
    /// <summary>
    /// 文字列が指定された長さを超える場合に切り詰めます
    /// </summary>
    /// <param name="value">対象の文字列</param>
    /// <param name="maxLength">最大長</param>
    /// <param name="suffix">切り詰め時に追加するサフィックス（省略可）</param>
    /// <returns>切り詰められた文字列</returns>
    public static string TruncateIfNeeded(this string value, int maxLength, string suffix = "...")
    {
      if (string.IsNullOrEmpty(value))
      {
        return value;
      }

      if (value.Length <= maxLength)
      {
        return value;
      }

      int truncateLength = maxLength - suffix.Length;
      if (truncateLength <= 0)
      {
        return suffix.Substring(0, maxLength);
      }

      return value.Substring(0, truncateLength) + suffix;
    }

    /// <summary>
    /// 文字列をJSON安全な形式に変換します
    /// </summary>
    /// <param name="value">対象の文字列</param>
    /// <returns>JSON安全な文字列</returns>
    public static string ToSafeJson(this string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return value;
      }

      // 制御文字を削除
      string safeValue = Regex.Replace(value, @"[\u0000-\u001F\u007F-\u009F]", string.Empty);

      // バックスラッシュをエスケープ
      safeValue = safeValue.Replace("\\", "\\\\");

      // ダブルクォートをエスケープ
      safeValue = safeValue.Replace("\"", "\\\"");

      return safeValue;
    }

    /// <summary>
    /// 文字列が有効なJSONかどうかを検証します
    /// </summary>
    /// <param name="value">検証する文字列</param>
    /// <returns>有効なJSONの場合はtrue、それ以外はfalse</returns>
    public static bool IsValidJson(this string value)
    {
      return JsonHelper.IsValidJson(value);
    }

    /// <summary>
    /// 文字列をBase64エンコードします
    /// </summary>
    /// <param name="value">エンコードする文字列</param>
    /// <returns>Base64エンコードされた文字列</returns>
    public static string ToBase64(this string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return value;
      }

      byte[] bytes = Encoding.UTF8.GetBytes(value);
      return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Base64エンコードされた文字列をデコードします
    /// </summary>
    /// <param name="value">デコードするBase64文字列</param>
    /// <returns>デコードされた文字列</returns>
    public static string FromBase64(this string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return value;
      }

      try
      {
        byte[] bytes = Convert.FromBase64String(value);
        return Encoding.UTF8.GetString(bytes);
      }
      catch (FormatException)
      {
        return value;
      }
    }
  }
}
