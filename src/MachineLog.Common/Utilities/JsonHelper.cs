using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MachineLog.Common.Utilities
{
  /// <summary>
  /// JSON処理のユーティリティメソッドを提供するクラス
  /// </summary>
  public static class JsonHelper
  {
    /// <summary>
    /// デフォルトのJSONシリアライズオプション
    /// </summary>
    private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
      WriteIndented = false
    };

    /// <summary>
    /// オブジェクトをJSON文字列にシリアライズします
    /// </summary>
    /// <typeparam name="T">シリアライズするオブジェクトの型</typeparam>
    /// <param name="value">シリアライズするオブジェクト</param>
    /// <param name="options">シリアライズオプション（省略可）</param>
    /// <returns>JSON文字列</returns>
    public static string Serialize<T>(T value, JsonSerializerOptions? options = null)
    {
      return JsonSerializer.Serialize(value, options ?? DefaultOptions);
    }

    /// <summary>
    /// JSON文字列からオブジェクトにデシリアライズします
    /// </summary>
    /// <typeparam name="T">デシリアライズ先のオブジェクト型</typeparam>
    /// <param name="json">JSON文字列</param>
    /// <param name="options">デシリアライズオプション（省略可）</param>
    /// <returns>デシリアライズされたオブジェクト</returns>
    public static T? Deserialize<T>(string json, JsonSerializerOptions? options = null)
    {
      if (string.IsNullOrEmpty(json))
      {
        return default;
      }

      return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
    }

    /// <summary>
    /// 文字列が有効なJSONかどうかを検証します
    /// </summary>
    /// <param name="json">検証するJSON文字列</param>
    /// <returns>有効なJSONの場合はtrue、それ以外はfalse</returns>
    public static bool IsValidJson(string json)
    {
      if (string.IsNullOrEmpty(json))
      {
        return false;
      }

      try
      {
        using (JsonDocument.Parse(json))
        {
          return true;
        }
      }
      catch (JsonException)
      {
        return false;
      }
    }

    /// <summary>
    /// 複数のJSONオブジェクトをマージします
    /// </summary>
    /// <param name="target">ターゲットとなるJSONオブジェクト</param>
    /// <param name="source">マージ元のJSONオブジェクト</param>
    /// <returns>マージされたJSONオブジェクト</returns>
    public static JsonElement MergeJsonObjects(JsonElement target, JsonElement source)
    {
      if (target.ValueKind != JsonValueKind.Object || source.ValueKind != JsonValueKind.Object)
      {
        throw new ArgumentException("両方のパラメータがJSONオブジェクトである必要があります。");
      }

      var targetDoc = JsonSerializer.SerializeToUtf8Bytes(target);
      var sourceDoc = JsonSerializer.SerializeToUtf8Bytes(source);

      var targetDict = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, JsonElement>>(targetDoc);
      var sourceDict = JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, JsonElement>>(sourceDoc);

      if (targetDict == null || sourceDict == null)
      {
        throw new InvalidOperationException("JSONオブジェクトの変換に失敗しました。");
      }

      foreach (var item in sourceDict)
      {
        targetDict[item.Key] = item.Value;
      }

      var mergedJson = JsonSerializer.Serialize(targetDict);
      using var doc = JsonDocument.Parse(mergedJson);
      return doc.RootElement.Clone();
    }
  }
}
