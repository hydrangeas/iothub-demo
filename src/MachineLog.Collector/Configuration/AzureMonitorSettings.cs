namespace MachineLog.Collector.Configuration
{
  /// <summary>
  /// Azure Monitor Logsの設定を表すクラス
  /// </summary>
  public class AzureMonitorSettings
  {
    /// <summary>
    /// ワークスペースID
    /// </summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>
    /// クライアントID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// クライアントシークレット
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// テナントID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// データ収集エンドポイント
    /// </summary>
    public string DataCollectionEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// データ収集ルールID
    /// </summary>
    public string DataCollectionRuleId { get; set; } = string.Empty;

    /// <summary>
    /// ストリーム名
    /// </summary>
    public string StreamName { get; set; } = "Custom-MachineLogStream";

    /// <summary>
    /// APIバージョン
    /// </summary>
    public string ApiVersion { get; set; } = "2022-10-01";
  }
}
