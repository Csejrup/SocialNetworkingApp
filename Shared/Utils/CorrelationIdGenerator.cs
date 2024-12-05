namespace Shared.Utils;

public static class CorrelationIdGenerator
{
    public static Guid Generate() => Guid.NewGuid();
}