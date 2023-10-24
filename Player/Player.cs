using Godot;
using System;
using System.Linq;

public partial class Player : CharacterBody3D
{
	[Export]
	private float movementVelocity = 10.0f;

	[Export]
	private float jumpVelocity = 5.0f;

	[Export]
	private float sanityCutoff = -100.0f;

	[Export]
	private float selectorRange = 2.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private RayCast3D rayCastFront;
	private SpringArm3D springArm3D;
	private Camera3D camera3D;
	private Island island;

	private bool isControllerInput;
	private Vector3I? selectedCell;
	private int selectorID;

	private int farmlandVoxelID;

	private ItemDefinition? SelectedItemDefinition;
	private ToolDefinition? SelectedToolDefinition;
	private PlantDefinition? SelectedPlantDefinition;

	public override void _Ready()
	{
		rayCastFront = GetNode<RayCast3D>("RayCastFront"); // TODO Not needed?
		springArm3D = GetNode<SpringArm3D>("SpringArm3D");
		camera3D = GetNode<Camera3D>("SpringArm3D/Camera3D");
		island = GetNode<Island>("/root/MainGame/Island");
		selectorID = island.MeshLibrary.FindItemByName("Selector");

		farmlandVoxelID = island.MeshLibrary.FindItemByName("Farmland");
	}

	public override void _Process(double delta)
	{
		var hotbar = GetNodeOrNull<HotBar>("/root/MainGame/HUD/HotBar");
		if (hotbar != null && hotbar.IsEmpty()) // TODO: Need a "new game" trigger instead
		{
			hotbar.AddItem(new InventoryItem
			{
				Name = "Hoe",
				Amount = 1,
			});
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleMovement();
		HandleGravity(delta);

		MoveAndSlide();

		SanityCheck();

		HandleMouseMovement();
		HandleSelectorInteraction();
	}

	private void HandleMovement()
	{
		var current_velocity = Velocity;

		var input_vector = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		var direction = springArm3D.Transform.Basis * new Vector3(input_vector.X, 0.0f, input_vector.Y).Normalized();

		current_velocity.X = direction.X * movementVelocity;
		current_velocity.Z = direction.Z * movementVelocity;

		if (IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			current_velocity.Y = jumpVelocity;
		}

		Velocity = current_velocity;
	}

	private void HandleGravity(double delta)
	{
		if (!IsOnFloor())
		{
			var current_velocity = Velocity;
			current_velocity.Y -= gravity * (float)delta;
			Velocity = current_velocity;
		}
	}

	private void SanityCheck()
	{
		if (Position.Y <= sanityCutoff)
		{
			GD.PushWarning("Sanity check! Returning player to spawn!");
			Position = new(0.0f, 5.0f, 0.0f);
		}
	}

	private void HandleMouseMovement()
	{
		// If there is a selected cell, remove it
		if (selectedCell != null)
		{
			island.SetCellItem(selectedCell.Value, (int)GridMap.InvalidCellItem);
			selectedCell = null;
		}

		Vector3 position;

		var input_vector = Input.GetVector("cursor_left", "cursor_right", "cursor_forward", "cursor_backward");
		if (input_vector.Length() > 0.5f)
		{
			var rounded = input_vector.Round();
			position = Position + new Vector3(rounded.X, -1, rounded.Y);

			// If there are controller inputs, reset the mouse to the center of the window and disable the mouse cursor.
			var window = GetViewport().GetWindow();
			var window_position = window.Position;
			var window_size = window.Size;
			var window_half_size = window_size / new Vector2(2f, 2f);
			var center_of_window = window_position + window_half_size;

			window.WarpMouse(center_of_window);
			Input.MouseMode = Input.MouseModeEnum.Hidden;
		}
		else
		{
			// Ray cast mouse position into world
			var mouse_position = GetViewport().GetMousePosition();
			var space_state = GetWorld3D().DirectSpaceState;

			var origin = camera3D.ProjectRayOrigin(mouse_position);
			var end = origin + camera3D.ProjectRayNormal(mouse_position) * 35.0f;

			var query = PhysicsRayQueryParameters3D.Create(origin, end);
			query.CollideWithAreas = true;
			query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };

			// If there is no position key, we probably didn't 
			// hit anything anyways so we just return out of here ...
			var result = space_state.IntersectRay(query);
			if (!result.ContainsKey("position"))
			{
				return;
			}

			Input.MouseMode = Input.MouseModeEnum.Visible;
			position = (Vector3)result["position"];
		}

		var player_cell = new Vector3I(
			(int)Math.Round(Position.X),
			(int)Math.Round(Position.Y),
			(int)Math.Round(Position.Z)
		);

		var cell_position = new Vector3I(
			(int)Math.Floor(position.X),
			(int)Math.Floor(position.Y),
			(int)Math.Floor(position.Z)
		);

		// Only select the cell if there is a single voxel distance
		var distance = player_cell - cell_position;
		if (distance.X <= 1 && distance.X >= -1 && distance.Z <= 1 && distance.Z >= -1)
		{
			for (int i = 0; i <= 2; i++)
			{
				var local_cell_position = cell_position + new Vector3I(0, i, 0);

				var cell_id = island.GetCellItem(local_cell_position);
				if (cell_id == GridMap.InvalidCellItem)
				{
					selectedCell = local_cell_position;
					island.SetCellItem(selectedCell.Value, selectorID);
					break;
				}
			}
		}
	}

	private void HandleSelectorInteraction()
	{
		if (selectedCell == null)
		{
			return;
		}

		var cell_position = selectedCell.Value - new Vector3I(0, 1, 0);
		var cell_id = island.GetCellItem(cell_position);

		if (selectedCell != null && Input.IsActionPressed("primary_action"))
		{
			if (SelectedItemDefinition != null && SelectedToolDefinition != null && SelectedToolDefinition.CanPlow)
			{
				if (island.plowableVoxelIDs.Contains(cell_id))
				{
					island.SetCellItem(cell_position, farmlandVoxelID);
				}
			}

			if (island.harvestableVoxelIDs.Contains(cell_id))
			{
				island.SetCellItem(cell_position, (int)Island.InvalidCellItem);

				// TODO: Add item to inventory (random?)
			}
		}

		if (selectedCell != null && Input.IsActionPressed("secondary_action"))
		{
			if (island.plantableVoxelIDs.Contains(cell_id) && SelectedPlantDefinition != null)
			{
				var cell_above = cell_position + new Vector3I(0, 1, 0);

				var plantID = island.MeshLibrary.FindItemByName(SelectedPlantDefinition.Name);
				if (plantID < 0)
				{
					GD.PrintErr($"Selected plant '{SelectedPlantDefinition}' could not be found in MeshLibrary!");
				}

				// TODO: Add a Radish item
				// TODO: Add Radish alongside Hoe to inventory

				selectedCell = null;
				island.SetCellItem(cell_above, plantID);
			}
		}
	}

	public void OnHotBarSelectionChanged(InventoryItem? item)
	{
		if (item == null)
		{
			SelectedItemDefinition = null;
		}
		else
		{
			var itemDef = ItemDatabase.Instance.FindItem(item);
			SelectedItemDefinition = itemDef;

			if (itemDef != null)
			{
				if (itemDef.Type == ItemType.Generic)
				{
					SelectedToolDefinition = null;
					SelectedPlantDefinition = null;
				}
				else if (itemDef.Type == ItemType.Tool)
				{
					SelectedPlantDefinition = null;

					var toolDefinition = ItemDatabase.Instance.FindTool(itemDef!.Name);
					SelectedToolDefinition = toolDefinition;
				}
				else if (itemDef.Type == ItemType.Plant)
				{
					SelectedToolDefinition = null;

					var plantDefinition = ItemDatabase.Instance.FindPlant(itemDef!.Name);
					SelectedPlantDefinition = plantDefinition;
				}
			}
		}
	}
}
