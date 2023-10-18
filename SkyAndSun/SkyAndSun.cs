using Godot;

public partial class SkyAndSun : Node3D
{
	private uint hoursPerDay;
	private uint minutesPerHour;
	private uint currentHour;
	private uint currentMinute;

	private Node3D anchor;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		anchor = GetNode<Node3D>("WorldEnvironment/Anchor");

		var DateAndTime = GetNode<DateAndTime>("/root/MainGame/DateAndTime");
		hoursPerDay = DateAndTime.hoursPerDay;
		minutesPerHour = DateAndTime.minutesPerHour;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var rotation = CalculateRotation();
		var rotation_vector = new Vector3(Mathf.DegToRad(rotation), 0f, 0f);

		anchor.Rotation = rotation_vector;
	}

	public void OnTimeChanged(uint day, uint month, uint year, uint hour, uint minute)
	{
		currentHour = hour;
		currentMinute = minute;
	}

	private float CalculateRotation()
	{
		var current_time_in_minutes = currentMinute + currentHour * minutesPerHour;
		var max_minutes_per_day = minutesPerHour * hoursPerDay;

		float value = current_time_in_minutes / (float)max_minutes_per_day;

		return -(360.0f * value) + 90.0f;
	}
}
