using FluentValidation;
using MachineLog.Common.Constants;
using MachineLog.Common.Models;
using System;

namespace MachineLog.Common.Validation
{
  /// <summary>
  /// LogEntryモデルの検証ロジックを提供するクラス
  /// </summary>
  public class LogEntryValidator : AbstractValidator<LogEntry>
  {
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public LogEntryValidator()
    {
      RuleFor(x => x.TimeGenerated)
          .NotEmpty().WithMessage("ログ生成時刻は必須です。")
          .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("未来の日付は指定できません。");

      RuleFor(x => x.MachineId)
          .NotEmpty().WithMessage("機械IDは必須です。")
          .MaximumLength(50).WithMessage("機械IDは50文字以内で指定してください。");

      RuleFor(x => x.Severity)
          .IsInEnum().WithMessage("無効な重要度が指定されています。");

      RuleFor(x => x.EventId)
          .NotEmpty().WithMessage("イベントIDは必須です。");

      RuleFor(x => x.Message)
          .NotEmpty().WithMessage("メッセージは必須です。")
          .MaximumLength(LogConstants.MaxMessageLength).WithMessage($"メッセージは{LogConstants.MaxMessageLength}文字以内で指定してください。");

      RuleForEach(x => x.Tags.Values)
          .MaximumLength(LogConstants.MaxTagValueLength).WithMessage($"タグ値は{LogConstants.MaxTagValueLength}文字以内で指定してください。");
    }
  }
}
