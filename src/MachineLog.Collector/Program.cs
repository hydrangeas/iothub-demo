using MachineLog.Collector.Configuration;
using MachineLog.Collector.Services;
using MachineLog.Common.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;

namespace MachineLog.Collector
{
  /// <summary>
  /// プログラムクラス
  /// </summary>
  public class Program
  {
    /// <summary>
    /// エントリポイント
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    /// <summary>
    /// ホストビルダーを作成します
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    /// <returns>ホストビルダー</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, config) =>
            {
              config.SetBasePath(Directory.GetCurrentDirectory());
              config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
              config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
              config.AddEnvironmentVariables();
              config.AddCommandLine(args);
            })
            .ConfigureServices((hostContext, services) =>
            {
              // 設定の登録
              services.Configure<CollectorSettings>(hostContext.Configuration.GetSection("Collector"));
              services.Configure<AzureMonitorSettings>(hostContext.Configuration.GetSection("AzureMonitor"));

              // バリデータの登録
              services.AddSingleton<LogEntryValidator>();
              services.AddSingleton<LogBatchValidator>();

              // HTTPクライアントの登録
              services.AddHttpClient();
              services.AddSingleton<HttpClient>();

              // サービスの登録
              services.AddSingleton<IFileWatcherService, FileWatcherService>();
              services.AddSingleton<ILogProcessorService, LogProcessorService>();
              services.AddSingleton<IAzureMonitorService, AzureMonitorService>();

              // ワーカーサービスの登録
              services.AddHostedService<CollectorWorkerService>();
            })
            .ConfigureLogging((hostContext, logging) =>
            {
              logging.ClearProviders();
              logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
              logging.AddConsole();
              logging.AddDebug();
              logging.AddEventSourceLogger();
            })
            .UseSerilog((hostContext, services, loggerConfiguration) =>
            {
              // ファイルログの設定
              var logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
              Directory.CreateDirectory(logsDirectory);

              loggerConfiguration
                .ReadFrom.Configuration(hostContext.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                  Path.Combine(logsDirectory, "collector-.log"),
                  rollingInterval: RollingInterval.Day,
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            });
  }
}
