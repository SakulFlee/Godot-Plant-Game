using Godot;

public partial class MainMenu : Control
{
	private Button button;

	public override void _Ready()
	{
		button = GetNode<Button>("Button");
	}

	public void OnButtonPressed()
	{
		GetTree().ChangeSceneToFile("res:///MainGame/MainGame.tscn");
	}
}
