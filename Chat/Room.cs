﻿namespace napredneBaze.Chat.Room
{
    public class Room
    {
        public class ChatRoom
        {
            public string Id { get; set; } = Guid.NewGuid().ToString();
            public string RoomName { get; set; }
            public IEnumerable<string>? Names { get; set; }
        }
    }
}
