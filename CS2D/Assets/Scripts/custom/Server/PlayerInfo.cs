using System.Net;

namespace custom.Server
{
    public class PlayerInfo
    {
        private int id;
        private IPEndPoint endPoint;

        public PlayerInfo(int id, IPEndPoint endPoint)
        {
            this.id = id;
            this.endPoint = endPoint;
        }

        public int Id => id;
    
        public IPEndPoint EndPoint => endPoint;


        protected bool Equals(PlayerInfo other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PlayerInfo) obj);
        }

        public override int GetHashCode()
        {
            return id;
        }
    }
}
