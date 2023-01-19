// ReSharper disable once CheckNamespace

namespace Serilog;

using System;
using System.Runtime.CompilerServices;

public static class SerilogExtensions
{
    public static ILogger ForContext<T>(
        this ILogger logger,
        T? value,
        [CallerArgumentExpression(nameof(value))]
        string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        return logger.ForContext(propertyName, value);
    }

    public static ILogger ForContextSourceMember(
        this ILogger logger,
        [CallerMemberName] string? sourceMember = null) =>
        logger.ForContext("SourceMember", sourceMember);
}