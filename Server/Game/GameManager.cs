using System.Collections.Concurrent;

namespace Server.Game
{
    // quan lys all client trong room
    public class GameManager
    {
        private readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
        private int _roomSeq = 1;

        public GameRoom AssignRoom(ClientRef client)
        {
            var room = _rooms.Values.FirstOrDefault(r => r.CanJoin);
            if (room == null)
            {
                var id = $"room-{Interlocked.Increment(ref _roomSeq)}";
                room = new GameRoom(id);
                _rooms[id] = room;
                Console.WriteLine($"Tạo phòng mới: {id}");
            }

            room.Join(client);
            return room;
        }

        public void RemoveRoom(GameRoom room)
        {
            if (room.IsEmpty)
            {
                _rooms.TryRemove(room.Id, out _);
                Console.WriteLine($"🗑 Xóa phòng {room.Id} (trống).");
            }
        }
    }

    public sealed class ClientRef
    {
        public string PlayerId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "Player";
        public Action<string>? SendJsonLine { get; set; }
    }
}
