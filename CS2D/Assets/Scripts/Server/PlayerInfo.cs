using System.Net;

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
    
}
