using Godot;
using System;

public partial class Island : GridMap
{
	[ExportCategory("General Terrain")]
	[Export(PropertyHint.Range, "5.0, 100.0,")]
	private uint radius = 50;

	[Export]
	private float below_ground_factor = 2.0f;

	[Export]
	private float terrain_indent_factor = 1.5f;

	[Export]
	private int water_level = 0;

	[ExportCategory("Terrain A")]
	[Export]
	private uint terrain_seed_a = 12345;

	[Export(PropertyHint.Range, "1.0, 6.0,")]
	private uint octaves_a = 6;

	[Export]
	private float frequency_a = 0.04f;

	[Export]
	private float lacunarity_a = 3.0f;

	[Export]
	private float persistence_a = 0.5f;

	[ExportCategory("Terrain B")]

	[Export]
	private uint terrain_seed_b = 54321;

	[Export(PropertyHint.Range, "1.0, 6.0,")]
	private uint octaves_b = 6;

	[Export]
	private float frequency_b = 0.025f;

	[Export]
	private float lacunarity_b = 6.0f;

	[Export]
	private float persistence_b = 0.5f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Island Settings: ");
		GD.Print("radius\t\t\t\t\t= " + radius);
		GD.Print("below_ground_factor\t\t= " + below_ground_factor);
		GD.Print("terrain_indent_factor\t= " + terrain_indent_factor);
		GD.Print("water_level\t\t\t\t= " + water_level);
		GD.Print("terrain_seed_a\t\t\t= " + terrain_seed_a);
		GD.Print("octaves_a\t\t\t\t= " + octaves_a);
		GD.Print("frequency_a\t\t\t\t= " + frequency_a);
		GD.Print("lacunarity_a\t\t\t= " + lacunarity_a);
		GD.Print("persistence_a\t\t\t= " + persistence_a);
		GD.Print("terrain_seed_b\t\t\t= " + terrain_seed_b);
		GD.Print("octaves_b\t\t\t\t= " + octaves_b);
		GD.Print("frequency_b\t\t\t\t= " + frequency_b);
		GD.Print("lacunarity_b\t\t\t= " + lacunarity_b);
		GD.Print("persistence_b\t\t\t= " + persistence_b);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
