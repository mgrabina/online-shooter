using System;
using System.Net;

namespace custom.Network
{
    public abstract class Message
    {
        protected Type type;
        protected int id;     // Client Id or 0 if new or -1 if server 
        protected Channel channel;
        protected String destinationIp;
        protected int destinationPort;
        protected Packet packet;

        protected Message(Type type, int id = 0, Channel channel = null, string destinationIp = null, int destinationPort = 0)
        {
            packet = Packet.Obtain();
            this.type = type;
            this.id = id;    
            this.channel = channel;
            this.destinationIp = destinationIp;
            this.destinationPort = destinationPort;
            
            packet.buffer.PutInt(SerializeType(this.type));
            packet.buffer.PutInt(id);
        }

        protected Message(Type type, int id, Packet packet)
        {
            this.type = type;
            this.id = id;
            this.packet = packet;
        }

        public void Send()
        {
            packet.buffer.Flush();
            channel.Send(packet, new IPEndPoint(IPAddress.Parse(destinationIp), destinationPort));
            packet.Free();
        }


        protected static int SerializeType(Type type)
        {
            for (int i = 0; i < typesSerialization.Length; i++)
            {
                if (typesSerialization[i].Equals(type))
                {
                    return i;
                }
            }
            return -1;
        }

        protected static Type? DeserializeType(int type)
        {
            if (type >= 0 && type < typesSerialization.Length)
            {
                return typesSerialization[type];
            }
            return null;
        }

        public static Message getMessage(Packet incomePacket)
        {
            Type type = DeserializeType(incomePacket.buffer.GetInt()).GetValueOrDefault(Type.NULL);
            int id = incomePacket.buffer.GetInt();
            switch (type)
            {
                case Type.JOIN_GAME: return new JoinGameMessage(type, id, incomePacket);
                case Type.INIT_STATUS: return new InitStatusMessage(type, id, incomePacket);
                case Type.PLAYER_JOINED: return new PlayerJoinedMessage(type, id, incomePacket);
                case Type.CLIENT_UPDATE: return new ClientUpdateMessage(type, id, incomePacket);
                case Type.HIT_ENEMY_MESSAGE: return new HitEnemyMessage(type, id, incomePacket);
                case Type.GAME_STATE_UPDATE: return new ServerUpdateMessage(type, id, incomePacket);
                case Type.CLIENT_UPDATE_ACK: return new ServerACKMessage(type, id, incomePacket);
                case Type.GOODBYE: return new GoodbyeMessage(type, id, incomePacket);
            }
            return null;
        }
        
        public enum Type
        {
            JOIN_GAME,
            PLAYER_JOINED,
            INIT_STATUS,
            
            CLIENT_UPDATE,
            CLIENT_UPDATE_ACK,
            GAME_STATE_UPDATE,
            
            HIT_ENEMY_MESSAGE,

            DISCONNECT_REQUEST,
            PLAYER_DISCONNECTED,
            GOODBYE,
            
            NULL
        }

        public static Type[] typesSerialization =
        {
            Type.JOIN_GAME,
            Type.PLAYER_JOINED,
            Type.INIT_STATUS,
            
            Type.CLIENT_UPDATE,
            Type.CLIENT_UPDATE_ACK,
            Type.GAME_STATE_UPDATE,
            
            Type.HIT_ENEMY_MESSAGE,

            Type.DISCONNECT_REQUEST,
            Type.PLAYER_DISCONNECTED,
            
            Type.GOODBYE
        };

        public new Type GetType => type;

        public int GetId => id;

        public Packet Packet => packet;
    }
}