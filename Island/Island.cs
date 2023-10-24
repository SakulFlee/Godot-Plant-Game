using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

[Tool]
public partial class Island : GridMap
{
	[ExportCategory("Island Update")]
	[Export]
	public UpdateStateRequest UpdateStateRequest = UpdateStateRequest.FullUpdate;

	[ExportCategory("Island Settings")]
	[Export(PropertyHint.Range, "5.0, 100.0,")]
	public uint Radius = 50;

	[Export]
	public float BelowGroundFactor = 2.0f;

	[Export(PropertyHint.Range, "0.0, 1.0, ")]
	public float TerrainIndentFactor = 0.05f;

	[Export]
	public string SpawnPlatformVoxel = "Grass";

	[Export]
	public Godot.Collections.Dictionary<int, Array<string>>? VoxelGeneration;

	[Export]
	public int WaterLevel = 0;

	[ExportCategory("Island Noise")]
	[Export]
	public NoiseTexture2D TerrainNoiseA = new NoiseTexture2D();

	[Export]
	public NoiseTexture2D TerrainNoiseB = new NoiseTexture2D();

	[ExportCategory("Island Change")]
	[Export]
	public Godot.Collections.Dictionary<Vector3I, int>? Changes;

	public int[] NoNeighbourVoxelIDs { get; private set; } = new int[0];
	public int[] PlowableVoxelIDs { get; private set; } = new int[0];
	public int[] PlantableVoxelIDs { get; private set; } = new int[0];
	public int[] HarvestableVoxelIDs { get; private set; } = new int[0];

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Force an update
		UpdateStateRequest = UpdateStateRequest.FullUpdate;

		NoNeighbourVoxelIDs = new int[]{
			(int)InvalidCellItem,
			MeshLibrary.FindItemByName("Water"),
			MeshLibrary.FindItemByName("Selector")
		};
		PlowableVoxelIDs = new int[]{
			MeshLibrary.FindItemByName("Grass"),
			MeshLibrary.FindItemByName("Dirt")
		};
		PlantableVoxelIDs = new int[]{
			MeshLibrary.FindItemByName("Farmland")
		};
		HarvestableVoxelIDs = new int[]{
			MeshLibrary.FindItemByName("Radish")
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (UpdateStateRequest == UpdateStateRequest.None)
		{
			return;
		}

		// Ensure there is a center island for the player to spawn
		EnsureCenterVoxels();

		// Remove all currently set voxels
		if (UpdateStateRequest == UpdateStateRequest.FullUpdate || UpdateStateRequest == UpdateStateRequest.Clear)
		{
			Clear();
		}

		if (UpdateStateRequest == UpdateStateRequest.FullUpdate)
		{
			// Utilize noise maps to generate the island(s)
			UpdateIslandWithNoise();
		}

		if (UpdateStateRequest == UpdateStateRequest.FullUpdate || UpdateStateRequest == UpdateStateRequest.ApplyChangeOnly)
		{
			// Apply any changes made to the island
			UpdateIslandWithChange();

			// Place water
			UpdateWater();

			// Cull any voxels that won't be in view
			CullIsland();
		}

		UpdateStateRequest = UpdateStateRequest.None;
	}

	private void UpdateWater()
	{
		// Calculate the range
		var range = Math.Pow(Radius, 2.0);

		var water_id = MeshLibrary.FindItemByName("Water");

		// Loop over (X, Y) coordinates (horizontal coordinates)
		for (int x = -(int)Radius; x <= Radius; x++)
		{
			for (int z = -(int)Radius; z <= Radius; z++)
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
		to_be_culled.ForEach(position => SetCellItem(position, (int)InvalidCellItem));
	}

	private void EnsureCenterVoxels()
	{
		if (Changes!.Count > 0)
		{
			return;
		}

		var voxel_id = MeshLibrary.FindItemByName(SpawnPlatformVoxel);

		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				Changes.Add(new Vector3I(z, 0, x), voxel_id);
			}
		}
	}

	private void UpdateIslandWithNoise()
	{
		// Calculate the range
		var range = Math.Pow(Radius, 2.0);

		// Loop over (X, Y) coordinates (horizontal coordinates)
		for (int x = -(int)Radius; x <= Radius; x++)
		{
			for (int z = -(int)Radius; z <= Radius; z++)
			{
				// If distance (without Y!) is greater than the range, skip
				var distance = new Vector3(x, 0, z).DistanceSquaredTo(Vector3.Zero);
				if (distance > range)
				{
					continue;
				}

				// Calculate noise values
				var noise_a = TerrainNoiseA.Noise.GetNoise2D(x, z) * 100.0;
				var noise_b = TerrainNoiseB.Noise.GetNoise2D(x, z) * 100.0;
				var below_ground = noise_a * noise_b / 100.0 * BelowGroundFactor;
				var terrain_indent = (noise_b - noise_a) * TerrainIndentFactor;

				// Use noise value for (Y) axis (vertical)
				for (int y = -(int)below_ground; y <= (int)terrain_indent; y++)
				{
					// If distance (with Y!) is greater than the range, skip
					distance = new Vector3(x, y, z).DistanceSquaredTo(Vector3.Zero);
					if (distance > range)
					{
						continue;
					}

					var voxel_id = (int)InvalidCellItem;
					var index = y;
					while (!VoxelGeneration!.ContainsKey(index))
					{
						if (index > 0)
						{
							index++;
						}
						else
						{
							index--;
						}

						if (index > Radius || index < -Radius)
						{
							index = int.MinValue;
							break;
						}
					}

					if (index > int.MinValue)
					{
						var options = VoxelGeneration[index];

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
		foreach ((var position, var voxel) in Changes!)
		{
			SetCellItem(position, voxel);
		}
	}
}
