/// <summary>Class used to save game state and player progress</summary>
public class Save
{
	// TODO : What do we need to save here ?

	public bool soundOn;
	public bool vibrationsOn;
	public InputManager.TiltType tiltType;

	public Save()
	{
		soundOn = true;
		vibrationsOn = true;
		tiltType = InputManager.TiltType.Gyroscope;
	}
}