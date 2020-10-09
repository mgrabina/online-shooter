using custom.Network;

public class PlayerInput {

	//Vector2 lookDir; //si moving esta en true mandar 2 floats
	public float x;
	public float y;
	public bool shoot;

	public void Save(BitBuffer buffer) {
		buffer.PutFloat(x);
		buffer.PutFloat(y);
		buffer.PutBit (shoot);
	}
	
	public void Load(BitBuffer buffer) {
		x = buffer.GetFloat();
		y = buffer.GetFloat();
		shoot = buffer.GetBit ();
	}
}

