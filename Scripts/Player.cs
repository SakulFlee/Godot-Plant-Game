using Godot;
using System;
using System.Linq;
using System.Net.WebSockets;

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

	private SpringArm3D SpringArm3D;
	private Camera3D Camera3D;
	private Island Island;

	private Vector3I? SelectedCell;
	private int SelectorID;

	private int FarmlandVoxelID;
	private int RadishID;

	public override void _Ready()
	{
		SpringArm3D = GetNode<SpringArm3D>("SpringArm3D");
		Camera3D = GetNode<Camera3D>("SpringArm3D/Camera3D");
		Island = GetNode<Island>("/root/Node/Island");
		SelectorID = Island.MeshLibrary.FindItemByName("Selector");

		FarmlandVoxelID = Island.MeshLibrary.FindItemByName("Farmland");
		RadishID = Island.MeshLibrary.FindItemByName("Radish");
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleCamera();
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
		var direction = SpringArm3D.Transform.Basis * new Vector3(input_vector.X, 0.0f, input_vector.Y).Normalized();

		current_velocity.X = direction.X * MovementVelocity;
		current_velocity.Z = direction.Z * MovementVelocity;

		if (IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			current_velocity.Y = JumpVelocity;
		}

		Velocity = current_velocity;
	}

	private void HandleCamera()
	{
		if (Input.IsActionJustPressed("camera_left"))
		{
			SpringArm3D.RotateY(Mathf.DegToRad(90f));
		}

		if (Input.IsActionJustPressed("camera_right"))
		{
			SpringArm3D.RotateY(Mathf.DegToRad(-90f));
		}
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
		// If there is a selected cell, remove it
		if (SelectedCell != null)
		{
			Island.SetCellItem(SelectedCell.Value, (int)GridMap.InvalidCellItem);
			SelectedCell = null;
		}

		// Ray cast mouse position into world
		var mouse_position = GetViewport().GetMousePosition();
		var space_state = GetWorld3D().DirectSpaceState;

		var origin = Camera3D.ProjectRayOrigin(mouse_position);
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

		var player_cell = new Vector3I(
			(int)Math.Floor(Position.X),
			(int)Math.Floor(Position.Y),
			(int)Math.Floor(Position.Z)
		);

		var position = (Vector3)result["position"];
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

				var cell_id = Island.GetCellItem(local_cell_position);
				if (cell_id == GridMap.InvalidCellItem)
				{
					SelectedCell = local_cell_position;
					Island.SetCellItem(SelectedCell.Value, SelectorID);
					break;
				}
			}
		}
	}

	private void HandleSelectorInteraction()
	{
		if (SelectedCell == null)
		{
			return;
		}

		var cell_position = SelectedCell.Value - new Vector3I(0, 1, 0);
		var cell_id = Island.GetCellItem(cell_position);

		if (SelectedCell != null && Input.IsActionJustPressed("primary_action"))
		{
			if (Island.PlowableVoxelIDs.Contains(cell_id))
			{
				Island.SetCellItem(cell_position, FarmlandVoxelID);
			}

			if (Island.HarvestableVoxelIDs.Contains(cell_id))
			{
				Island.SetCellItem(cell_position, (int)Island.InvalidCellItem);
			}
		}

		if (SelectedCell != null && Input.IsActionJustPressed("secondary_action"))
		{
			if (Island.PlantableVoxelIDs.Contains(cell_id))
			{
				var cell_above = cell_position + new Vector3I(0, 1, 0);

				SelectedCell = null;
				Island.SetCellItem(cell_above, RadishID);
			}
		}
	}
}

// TODO: Controller input
// TODO: Use NoNeighbourVoxelIDs
