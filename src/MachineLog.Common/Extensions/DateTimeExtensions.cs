using System;
using System.Globalization;

namespace MachineLog.Common.Extensions
{
  /// <summary>
  /// DateTimeに対する拡張メソッドを提供するクラス
  /// </summary>
  public static class DateTimeExtensions
  {
    /// <summary>
    /// DateTimeをISO 8601形式の文字列に変換します
    /// </summary>
    /// <param name="dateTime">変換する日時</param>
    /// <returns>ISO 8601形式の文字列</returns>
    public static string ToIso8601String(this DateTime dateTime)
    {
      return dateTime.ToString("o", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// ISO 8601形式の文字列からDateTimeに変換します
    /// </summary>
    /// <param name="iso8601String">ISO 8601形式の文字列</param>
    /// <returns>変換されたDateTime</returns>
    public static DateTime FromIso8601String(this string iso8601String)
    {
      if (string.IsNullOrEmpty(iso8601String))
      {
        throw new ArgumentException("文字列がnullまたは空です。", nameof(iso8601String));
      }

      return DateTime.Parse(iso8601String, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    /// <summary>
    /// 日時をUNIXタイムスタンプ（秒）に変換します
    /// </summary>
    /// <param name="dateTime">変換する日時</param>
    /// <returns>UNIXタイムスタンプ（秒）</returns>
    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
      return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();
    }

    /// <summary>
    /// 日時をUNIXタイムスタンプ（ミリ秒）に変換します
    /// </summary>
    /// <param name="dateTime">変換する日時</param>
    /// <returns>UNIXタイムスタンプ（ミリ秒）</returns>
    public static long ToUnixTimeMilliseconds(this DateTime dateTime)
    {
      return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// UNIXタイムスタンプ（秒）からDateTimeに変換します
    /// </summary>
    /// <param name="unixTimeSeconds">UNIXタイムスタンプ（秒）</param>
    /// <returns>変換されたDateTime（UTC）</returns>
    public static DateTime FromUnixTimeSeconds(this long unixTimeSeconds)
    {
      return DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;
    }

    /// <summary>
    /// UNIXタイムスタンプ（ミリ秒）からDateTimeに変換します
    /// </summary>
    /// <param name="unixTimeMilliseconds">UNIXタイムスタンプ（ミリ秒）</param>
    /// <returns>変換されたDateTime（UTC）</returns>
    public static DateTime FromUnixTimeMilliseconds(this long unixTimeMilliseconds)
    {
      return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds).UtcDateTime;
    }

    /// <summary>
    /// 日時が別の日時の範囲内かどうかを判定します
    /// </summary>
    /// <param name="dateTime">判定する日時</param>
    /// <param name="startDate">開始日時</param>
    /// <param name="endDate">終了日時</param>
    /// <returns>範囲内の場合はtrue、それ以外はfalse</returns>
    public static bool IsBetween(this DateTime dateTime, DateTime startDate, DateTime endDate)
    {
      return dateTime >= startDate && dateTime <= endDate;
    }
  }
}
