using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Contents
{
    public class RoomManager : JobSerializer
    {
        public static RoomManager Instance { get; } = new RoomManager();

        Dictionary<int, Room> _rooms = new Dictionary<int, Room>();
        int _roomId = 1;

        public void Update()
        {
            Flush();

            foreach (Room room in _rooms.Values)
                room.Update();
        }
        public Room Add(int mapId)
        {
            Room room = new Room();
            room.Push(room.Init, mapId, 10);

            room.RoomId = _roomId;
            _rooms.Add(_roomId, room);
            _roomId++;
            
            return room;
        }

        public bool Remove(int roomId)
        {
            return _rooms.Remove(roomId);
        }

        public Room Find(int roomId)
        {
            Room room = null;
            if (_rooms.TryGetValue(roomId, out room))
                return room;
            return null;           
        }
    }
}
