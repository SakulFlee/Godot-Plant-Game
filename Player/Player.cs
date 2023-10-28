using Godot;
using System;
using System.Linq;

public partial class Player : CharacterBody3D
{
	[Export]
	private float MovementVelocity = 10.0f;

	[Export]
	private float JumpVelocity = 5.0f;

	[Export]
	private float SanityCutoff = -100.0f;

	[Export]
	private float SelectorRange = 2.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private SpringArm3D? SpringArm3D;
	private Camera3D? Camera3D;
	private Island? Island;

	private bool IsControllerInput;
	private Vector3I? SelectedCell;
	private int SelectorID;
	private int MinimumYSelectorSnap = 0;
	private int MaximumYSelectorSnap = 10;

	private int FarmlandVoxelID;

	private HotBar? HotBar;
	private ItemDefinition? SelectedItemDefinition;
	private ToolDefinition? SelectedToolDefinition;
	private PlantDefinition? SelectedPlantDefinition;

	public override void _Ready()
	{
		SpringArm3D = GetNode<SpringArm3D>("SpringArm3D");
		Camera3D = GetNode<Camera3D>("SpringArm3D/Camera3D");
		Island = GetNode<Island>("/root/MainGame/Island");
		SelectorID = Island.MeshLibrary.FindItemByName("Selector");

		FarmlandVoxelID = Island.MeshLibrary.FindItemByName("Farmland");

		HotBar = GetNode<HotBar>("/root/MainGame/HUD/HotBar");
	}

	public override void _Process(double delta)
	{
		if (HotBar!.IsEmpty())
		{
			HotBar.AddItem(new InventoryItem
			{
				Name = "Hoe",
				Amount = 1,
			});
			HotBar.AddItem(new InventoryItem
			{
				Name = "Radish",
				Amount = 20,
			});
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleMovement();
		HandleGravity(delta);

		MoveAndSlide();

		SanityCheck();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion || @event is InputEventJoypadMotion)
		{
			HandleMouseMovement();
		}

		if (@event is InputEventMouseButton || @event is InputEventJoypadButton || @event is InputEventMouseMotion || @event is InputEventJoypadButton)
		{
			HandleSelectorInteraction();
		}
	}

	private void HandleMovement()
	{
		var current_velocity = Velocity;

		var input_vector = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		var direction = SpringArm3D!.Transform.Basis * new Vector3(input_vector.X, 0.0f, input_vector.Y).Normalized();

		current_velocity.X = direction.X * MovementVelocity;
		current_velocity.Z = direction.Z * MovementVelocity;

		if (IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			current_velocity.Y = JumpVelocity;
		}

		Velocity = current_velocity;
	}

	private void HandleGravity(double delta)
	{
		if (!IsOnFloor())
		{
			var current_velocity = Velocity;
			current_velocity.Y -= Gravity * (float)delta;
			Velocity = current_velocity;
		}
	}

	private void SanityCheck()
	{
		if (Position.Y <= SanityCutoff)
		{
			GD.PushWarning("Sanity check! Returning player to spawn!");
			Position = new(0.0f, 5.0f, 0.0f);
		}
	}

	private void HandleMouseMovement()
	{
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

			var origin = Camera3D!.ProjectRayOrigin(mouse_position);
			var end = origin + Camera3D.ProjectRayNormal(mouse_position) * 35.0f;

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

		var playerCell = new Vector3I(
			(int)Math.Round(Position.X),
			(int)Math.Round(Position.Y),
			(int)Math.Round(Position.Z)
		);

		var cellPosition = new Vector3I(
			(int)Math.Floor(position.X),
			(int)Math.Floor(position.Y),
			(int)Math.Floor(position.Z)
		);

		// Only select the cell if there is a single voxel distance
		Vector3I? newSelectedCell = null;
		var distance = playerCell - cellPosition;
		if (distance.X <= 1 && distance.X >= -1 && distance.Z <= 1 && distance.Z >= -1)
		{
			for (int y = MinimumYSelectorSnap; y <= MaximumYSelectorSnap; y++)
			{
				var localCellPosition = new Vector3I(cellPosition.X, y, cellPosition.Z);
				var localCellId = Island!.GetCellItem(localCellPosition);

				if (localCellId == GridMap.InvalidCellItem || localCellId == SelectorID)
				{
					newSelectedCell = localCellPosition - new Vector3I(0, 1, 0);
					break;
				}
			}
		}

		// Only change the selector if the existing and new cell aren't the same
		if (SelectedCell != newSelectedCell)
		{
			// Remove the existing selector if it exists
			if (SelectedCell != null)
			{
				Island!.SetCellItem(SelectedCell!.Value + new Vector3I(0, 1, 0), (int)GridMap.InvalidCellItem);
			}

			// Set the new selected cell
			SelectedCell = newSelectedCell;

			// Set the new selector if not null
			if (SelectedCell != null)
			{
				Island!.SetCellItem(SelectedCell!.Value + new Vector3I(0, 1, 0), SelectorID);
			}
		}
	}

	private void HandleSelectorInteraction()
	{
		if (SelectedCell == null)
		{
			return;
		}

		var cellId = Island!.GetCellItem(SelectedCell!.Value);

		if (Input.IsActionPressed("primary_action"))
		{
			if (SelectedToolDefinition != null && SelectedToolDefinition.CanPlow)
			{
				if (Island.PlowableVoxelIDs.Contains(cellId))
				{
					Island.SetCellItem(SelectedCell!.Value, FarmlandVoxelID);
				}
			}

			if (Island.HarvestableVoxelIDs.Contains(cellId))
			{
				Island.SetCellItem(SelectedCell!.Value, (int)Island.InvalidCellItem);
			}
		}

		if (Input.IsActionPressed("secondary_action"))
		{
			if (SelectedPlantDefinition != null && SelectedPlantDefinition.CanBePlanted)
			{
				if (Island.PlantableVoxelIDs.Contains(cellId) && SelectedPlantDefinition != null)
				{
					var cell_above = SelectedCell!.Value + new Vector3I(0, 1, 0);

					var plantID = Island.MeshLibrary.FindItemByName(SelectedPlantDefinition.Name);
					if (plantID < 0)
					{
						GD.PrintErr($"Selected plant '{SelectedPlantDefinition}' could not be found in MeshLibrary!");
					}

					SelectedCell = null;
					Island.SetCellItem(cell_above, plantID);
					HotBar!.RemoveItem(1);
				}
			}
		}
	}

	public void OnHotBarSelectionChanged(InventoryItem? item)
	{
		if (item == null)
		{
			SelectedItemDefinition = null;
			SelectedToolDefinition = null;
			SelectedPlantDefinition = null;
		}
		else
		{
			var itemDef = ItemDatabase.Instance.FindItem(item);
			SelectedItemDefinition = itemDef;

			if (itemDef == null || itemDef.Type == ItemType.Generic)
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
