using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

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

	[Export(PropertyHint.Range, "0.0, 1.0, ")]
	private float terrain_indent_factor = 0.05f;

	[Export]
	private string spawn_platform_voxel = "Grass";

	[Export]
	private Godot.Collections.Dictionary<int, Godot.Collections.Array<string>> voxel_generation;

	[Export]
	private int water_level = 0;

	[ExportCategory("Island Noise")]
	[Export]
	private NoiseTexture2D terrain_noise_a;

	[Export]
	private NoiseTexture2D terrain_noise_b;

	[ExportCategory("Island Change")]
	[Export]
	private Godot.Collections.Dictionary<Vector3I, int> change_dict;

	public int[] NoNeighbourVoxelIDs { get; private set; }
	public int[] PlowableVoxelIDs { get; private set; }
	public int[] PlantableVoxelIDs { get; private set; }
	public int[] HarvestableVoxelIDs { get; private set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Force an update
		update_state_request = UpdateStateRequest.FullUpdate;

		NoNeighbourVoxelIDs = new int[] {
			(int) InvalidCellItem,
			MeshLibrary.FindItemByName("Water"),
			MeshLibrary.FindItemByName("Selector")
		};
		PlowableVoxelIDs = new int[] {
			MeshLibrary.FindItemByName("Grass"),
			MeshLibrary.FindItemByName("Dirt")
		};
		PlantableVoxelIDs = new int[] {
			MeshLibrary.FindItemByName("Farmland")
		};
		HarvestableVoxelIDs = new int[] {
			MeshLibrary.FindItemByName("Radish")
		};
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
		if (update_state_request == UpdateStateRequest.FullUpdate || update_state_request == UpdateStateRequest.Clear)
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

			// Place water
			UpdateWater();

			// Cull any voxels that won't be in view
			CullIsland();
		}

		update_state_request = UpdateStateRequest.None;
	}

	private void UpdateWater()
	{
		// Calculate the range
		var range = Math.Pow(radius, 2.0);

		var water_id = MeshLibrary.FindItemByName("Water");

		// Loop over (X, Y) coordinates (horizontal coordinates)
		for (int x = -(int)radius; x <= radius; x++)
		{
			for (int z = -(int)radius; z <= radius; z++)
			{
				// If distance (without Y!) is greater than the range, skip
				var distance = new Vector3(x, 0, z).DistanceSquaredTo(Vector3.Zero);
				if (distance > range)
				{
					continue;
				}

				var cell_position = new Vector3I(x, 0, z);
				var current_cell_id = GetCellItem(cell_position);

				if (current_cell_id < 0)
				{
					SetCellItem(cell_position, water_id);
				}
			}
		}
	}

	private void CullIsland()
	{
		var to_be_culled = new List<Vector3I>();

		var used_cells = GetUsedCells();
		var count_before = used_cells.Count;
		for (int i = 0; i < count_before; i++)
		{
			var position = used_cells[i];

			var neighbours = 0;
			// TODO: Voxel Library
			if (GetCellItem(position + new Vector3I(1, 0, 0)) > 0)
			{
				neighbours++;
			}
			if (GetCellItem(position + new Vector3I(-1, 0, 0)) > 0)
			{
				neighbours++;
			}
			if (GetCellItem(position + new Vector3I(0, 1, 0)) > 0)
			{
				neighbours++;
			}
			if (GetCellItem(position + new Vector3I(0, -1, 0)) > 0)
			{
				neighbours++;
			}
			if (GetCellItem(position + new Vector3I(0, 0, 1)) > 0)
			{
				neighbours++;
			}
			if (GetCellItem(position + new Vector3I(0, 0, -1)) > 0)
			{
				neighbours++;
			}

			if (neighbours == 6)
			{
				to_be_culled.Add(position);
			}
		}

		GD.Print("Culling " + to_be_culled.Count + "/" + count_before);
		to_be_culled.ForEach(position => SetCellItem(position, -1));
	}

	private void EnsureCenterVoxels()
	{
		if (change_dict.Count > 0)
		{
			return;
		}

		var voxel_id = MeshLibrary.FindItemByName(spawn_platform_voxel);

		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				change_dict.Add(new Vector3I(z, 0, x), voxel_id);
			}
		}
	}

	private void UpdateIslandWithNoise()
	{
		// Calculate the range
		var range = Math.Pow(radius, 2.0);

		// Loop over (X, Y) coordinates (horizontal coordinates)
		for (int x = -(int)radius; x <= radius; x++)
		{
			for (int z = -(int)radius; z <= radius; z++)
			{
				// If distance (without Y!) is greater than the range, skip
				var distance = new Vector3(x, 0, z).DistanceSquaredTo(Vector3.Zero);
				if (distance > range)
				{
					continue;
				}

				// Calculate noise values
				var noise_a = terrain_noise_a.Noise.GetNoise2D(x, z) * 100.0;
				var noise_b = terrain_noise_b.Noise.GetNoise2D(x, z) * 100.0;
				var below_ground = (noise_a * noise_b) / 100.0 * below_ground_factor;
				var terrain_indent = (noise_b - noise_a) * terrain_indent_factor;

				// Use noise value for (Y) axis (vertical)
				for (int y = -(int)below_ground; y <= (int)terrain_indent; y++)
				{
					// If distance (with Y!) is greater than the range, skip
					distance = new Vector3(x, y, z).DistanceSquaredTo(Vector3.Zero);
					if (distance > range)
					{
						continue;
					}

					var voxel_id = -1;
					var index = y;
					while (!voxel_generation.ContainsKey(index))
					{
						if (index > 0)
						{
							index++;
						}
						else
						{
							index--;
						}

						if (index > radius || index < -radius)
						{
							index = int.MinValue;
							break;
						}
					}

					if (index > int.MinValue)
					{
						var options = voxel_generation[index];

						Random random = new();
						int option_index = random.Next(0, options.Count);

						voxel_id = MeshLibrary.FindItemByName(options[option_index]);
					}

					var cell_position = new Vector3I(x, y, z);
					SetCellItem(cell_position, voxel_id);
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
