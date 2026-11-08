using DragonEmu.Domain;
using DragonEmu.Protocol.Messages;

namespace DragonEmu.Game.Settings;

public static class ReaderLanguage
{
    private static readonly AsyncLocal<IGameSession?> Reader = new();

    public static Language Current => Reader.Value?.Language ?? Language.En;

    public static Scope For(IGameSession session)
    {
        IGameSession? previous = Reader.Value;
        Reader.Value = session;
        return new Scope(previous);
    }

    public readonly struct Scope : IDisposable
    {
        private readonly IGameSession? _previous;

        internal Scope(IGameSession? previous)
        {
            _previous = previous;
        }

        public void Dispose() => Reader.Value = _previous;
    }
}
