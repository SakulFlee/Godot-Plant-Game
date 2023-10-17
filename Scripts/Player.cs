using Godot;
using System;
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
	private GridMap Island;

	private Vector3I? SelectedCell;

	public override void _Ready()
	{
		SpringArm3D = GetNode<SpringArm3D>("SpringArm3D");
		Camera3D = GetNode<Camera3D>("SpringArm3D/Camera3D");
		Island = GetNode<GridMap>("/root/Node/Island");
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleCamera();
		HandleMovement();
		HandleGravity(delta);

		MoveAndSlide();

		SanityCheck();

		HandleMouse();
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

	private void HandleMouse()
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
		var result = space_state.IntersectRay(query);
		// If there is no position key, we probably didn't 
		// hit anything anyways so we just return out of here ...
		if (!result.ContainsKey("position"))
		{
			GD.PrintErr("No position");
			return;
		}

		var position = (Vector3)result["position"];
		GD.Print("Position: " + position);
		var distance_to_player = position.DistanceSquaredTo(Position);

		// Only if the selection is within range set the selector
		if (distance_to_player <= Math.Pow(SelectorRange, 2.0))
		{
			var cell_position = new Vector3I(
			(int)Math.Round(position.X, 0),
			(int)Math.Round(position.Y, 0),
			(int)Math.Round(position.Z, 0)
		);
			GD.Print("Cell: " + cell_position);

			var cell_id = Island.GetCellItem(cell_position);
			GD.Print("Cell ID: " + cell_id);

			// Only if the cell hit is empty set the selector
			if (cell_id == GridMap.InvalidCellItem)
			{
				SelectedCell = cell_position;
				Island.SetCellItem(SelectedCell.Value, Island.MeshLibrary.FindItemByName("Selector"));
			}
		}
	}
}

// TODO: Controller input
// TODO: Change to farmland
// TODO: Plant
