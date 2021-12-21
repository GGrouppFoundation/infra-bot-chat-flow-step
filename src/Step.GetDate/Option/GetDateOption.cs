using System;
using System.Diagnostics.CodeAnalysis;

namespace GGroupp.Infra.Bot.Builder;

public readonly record struct GetDateOption
{
    private const string DefaultConfirmButtonText = "Ok";

    private static readonly DateOnly DefaultDateOnly = DateOnly.FromDateTime(DateTime.Now);

    public GetDateOption(
        [AllowNull] string confirmButtonText = DefaultConfirmButtonText,
        DateOnly? defaultDate = null,
        DateOnly? minDate = null,
        DateOnly? maxDate = null)
    {
        ConfirmButtonText = confirmButtonText ?? DefaultConfirmButtonText;
        DefaultDate = defaultDate ?? DefaultDateOnly; 
        MinDate = minDate ?? DateOnly.MinValue;
        MaxDate = maxDate ?? DateOnly.MaxValue;
    }

    public string ConfirmButtonText { get; }

    public readonly DateOnly DefaultDate { get; }

    public readonly DateOnly MinDate { get; }

    public readonly DateOnly MaxDate { get; }
}