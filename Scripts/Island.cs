using Godot;
using System;

public partial class Island : GridMap
{
	[ExportGroup("Island")]
	[Export]
	public int some_value { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Some Value: " + some_value);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
