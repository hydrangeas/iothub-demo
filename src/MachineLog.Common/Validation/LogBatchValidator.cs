using FluentValidation;
using MachineLog.Common.Constants;
using MachineLog.Common.Models;
using System;

namespace MachineLog.Common.Validation
{
  /// <summary>
  /// LogBatchモデルの検証ロジックを提供するクラス
  /// </summary>
  public class LogBatchValidator : AbstractValidator<LogBatch>
  {
    /// <summary>
    /// コンストラクタ
    /// </summary>
    public LogBatchValidator()
    {
      RuleFor(x => x.BatchId)
          .NotEmpty().WithMessage("バッチIDは必須です。");

      RuleFor(x => x.CreatedAt)
          .NotEmpty().WithMessage("作成日時は必須です。")
          .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("未来の日付は指定できません。");

      RuleFor(x => x.Entries)
          .NotEmpty().WithMessage("少なくとも1つのログエントリが必要です。")
          .Must(entries => entries.Count <= LogConstants.MaxBatchEntries)
          .WithMessage($"バッチには最大{LogConstants.MaxBatchEntries}個のエントリしか含められません。");

      RuleFor(x => x.Size)
          .Must(size => size <= LogConstants.MaxBatchSizeBytes)
          .WithMessage($"バッチサイズは{LogConstants.MaxBatchSizeBytes / 1024}KB以下である必要があります。");

      RuleForEach(x => x.Entries)
          .SetValidator(new LogEntryValidator());
    }
  }
}
