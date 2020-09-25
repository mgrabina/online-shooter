using System.Collections.Generic;
using custom.Utils;

namespace custom.Network
{
    public class ClientUpdateMessage : Message
    {
        private List<Commands> commands;

        public ClientUpdateMessage(int id = 0, Channel channel = null, string destinationIp = null, int destinationPort = 0) : base(Type.CLIENT_UPDATE, id, channel, destinationIp, destinationPort)
        {
        }

        public ClientUpdateMessage(Type type, int id, Packet packet) : base(type, id, packet)
        {
            int q = packet.buffer.GetInt();
            commands = new List<Commands>();
            for (int i = 0; i < q; i++)
            {
                var commands = new Commands();
                commands.Deserialize(packet.buffer);
                this.commands.Add(commands);
            }
        }

        public ClientUpdateMessage setArguments(List<Commands> commands)
        {
            this.commands = commands;
            packet.buffer.PutInt(commands.Count);
            foreach (var command in commands)
            {
                command.Serialize(packet.buffer);
            }
            return this;
        }

        public List<Commands> Commands => commands;
    }
}