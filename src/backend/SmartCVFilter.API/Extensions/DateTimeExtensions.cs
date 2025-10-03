public static class DateTimeExtensions
{
    public static DateTime? ToUtcSafe(this DateTime? dateTime)
    {
        if (!dateTime.HasValue)
            return null;

        return dateTime.Value.Kind == DateTimeKind.Utc
            ? dateTime.Value
            : dateTime.Value.ToUniversalTime();
    }
}