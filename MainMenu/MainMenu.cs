using Godot;

public partial class MainMenu : Control
{
	public void OnButtonPressed()
	{
		GetTree().ChangeSceneToFile("res:///MainGame/MainGame.tscn");
	}
}
