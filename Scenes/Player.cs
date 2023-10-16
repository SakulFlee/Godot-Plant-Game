using Godot;
using System;

public partial class Player : CharacterBody3D
{
	[Export]
	private float MovementVelocity = 10.0f;

	[Export]
	private float JumpVelocity = 5.0f;

	[Export]
	private float SanityCutoff = -100.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	private float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private SpringArm3D SpringArm3D;

	public override void _Ready()
	{
		SpringArm3D = GetNode<SpringArm3D>("SpringArm3D");
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleCamera();
		HandleMovement();
		HandleGravity(delta);

		MoveAndSlide();

		SanityCheck();
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
}
