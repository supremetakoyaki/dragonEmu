using System.Text;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class FloorHeightMapMessageEvent : IMessageEvent
{
    private readonly string _heightmap;
    private readonly int _doorX;
    private readonly int _doorY;
    private readonly double _doorZ;

    public FloorHeightMapMessageEvent(string heightmap, int doorX, int doorY, double doorZ)
    {
        _heightmap = heightmap;
        _doorX = doorX;
        _doorY = doorY;
        _doorZ = doorZ;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.FloorHeightMapMessageEvent);

        string[] rows = _heightmap.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        for (int y = 0; y < rows.Length; y++)
        {
            string row = rows[y];

            for (int x = 0; x < row.Length; x++)
            {
                string square = x == _doorX && y == _doorY
                    ? ((int)_doorZ).ToString()
                    : row[x].ToString().Trim().ToLowerInvariant();

                packet.AppendBytes(Encoding.UTF8.GetBytes(square));
            }

            packet.AppendBytes([(byte)13]);
        }

        return packet;
    }
}
