using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class Island : GridMap
{
	[ExportCategory("Island Update")]
	[Export]
	private UpdateStateRequest update_state_request = UpdateStateRequest.FullUpdate;

	[ExportCategory("Island Settings")]
	[Export(PropertyHint.Range, "5.0, 100.0,")]
	private uint radius = 50;

	[Export]
	private float below_ground_factor = 2.0f;

	[Export]
	private float terrain_indent_factor = 1.5f;

	[Export]
	private int spawn_platform_voxel_id = 1;

	[Export]
	private int water_level = 0;

	[ExportCategory("Island Noise")]
	[Export]
	private NoiseTexture2D terrain_noise_a;

	[Export]
	private NoiseTexture2D terrain_noise_b;

	[ExportCategory("Island Change")]
	[Export]
	private Dictionary<Vector3I, int> change_dict;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Force an update
		update_state_request = UpdateStateRequest.FullUpdate;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		DoUpdate();
	}

	private void DoUpdate()
	{

		if (update_state_request == UpdateStateRequest.None)
		{
			return;
		}

		GD.Print("Doing Update!");

		// Ensure there is a center island for the player to spawn
		EnsureCenterVoxels();

		// Remove all currently set voxels
		if (update_state_request == UpdateStateRequest.FullUpdate)
		{
			Clear();
		}

		if (update_state_request == UpdateStateRequest.FullUpdate)
		{
			// Utilize noise maps to generate the island(s)
			UpdateIslandWithNoise();
		}

		if (update_state_request == UpdateStateRequest.FullUpdate || update_state_request == UpdateStateRequest.ApplyChangeOnly)
		{
			// Apply any changes made to the island
			UpdateIslandWithChange();
		}

		update_state_request = UpdateStateRequest.None;
	}

	private void EnsureCenterVoxels()
	{
		if (change_dict.Count > 0)
		{
			return;
		}

		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				change_dict.Add(new Vector3I(z, 0, x), spawn_platform_voxel_id);
			}
		}
	}

	private void UpdateIslandWithNoise()
	{
		for (int x = -(int)radius; x <= radius; x++)
		{
			for (int z = -(int)radius; z <= radius; z++)
			{
				var position = new Vector3(x, 0, z);
				var distance = position.DistanceSquaredTo(Vector3.Zero);

				if (distance > Math.Pow(radius, 2.0f))
				{
					continue;
				}

				var noise_a = terrain_noise_a.Noise.GetNoise2D(x, z) * 100.0;
				var noise_b = terrain_noise_b.Noise.GetNoise2D(x, z) * 100.0;

				var height = (int)Math.Round(noise_a - noise_b);

				for (int y = -height; y <= 0; y++)
				{
					var p = new Vector3I(x, y, z);
					SetCellItem(p, 1);
				}
			}
		}
	}

	private void UpdateIslandWithChange()
	{
		foreach ((var position, var voxel) in change_dict)
		{
			SetCellItem(position, voxel);
		}
	}
}
