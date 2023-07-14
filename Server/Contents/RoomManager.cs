using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

        object _lock = new object();
        Dictionary<int, Room> _rooms = new Dictionary<int, Room>();
        int _roomId = 1;

        public Room Add()
        {
            Room room = new Room();
            room.Init();
            lock (_lock)
            {
                room.RoomId = _roomId;
                _rooms.Add(_roomId, room);
                _roomId++;
            }

            return room;
        }

        public bool Remove(int roomId)
        {
            lock (_lock)
            {
                return _rooms.Remove(roomId);
            }
        }

        public Room Find(int roomId)
        {
            lock (_lock)
            {
                Room room = null;
                if (_rooms.TryGetValue(roomId, out room))
                    return room;
                return null;
            }
        }
    }
}
