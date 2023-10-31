using Godot;
using System;

public partial class EscapeMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Hide when being added to the node tree
		Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("escape_menu"))
		{
			// Invert visibility on node
			Visible = !Visible;
		}
	}

	public void OnResumeButtonPressed()
	{
		// Hide again, nothing to do! :)
		Visible = false;
	}

	public void OnSaveGameButtonPressed()
	{
		var saveResource = SaveResource.FromCurrent(this);
		saveResource.SaveToFile("test");
	}
 
	public void OnSettingsButtonPressed()
	{
		// TODO
	}

	public void OnExitToMainMenuButtonPressed()
	{
		// TODO
	}
}
