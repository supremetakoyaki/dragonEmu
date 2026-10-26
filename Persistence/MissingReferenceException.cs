namespace DragonEmu.Persistence;

public sealed class MissingReferenceException : Exception
{
    public MissingReferenceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
