using Godot;
using System;

public partial class SkyAndSun : Node3D
{
	private uint HoursPerDay;
	private uint MinutesPerHour;

	private uint CurrentHour;
	private uint CurrentMinute;

	private Godot.Node3D Anchor;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Anchor = GetNode<Godot.Node3D>("WorldEnvironment/Anchor");

		var DateAndTime = GetNode<DateAndTime>("/root/MainGame/DateAndTime");
		HoursPerDay = DateAndTime.HoursPerDay;
		MinutesPerHour = DateAndTime.MinutesPerHour;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var rotation = CalculateRotation();
		var rotation_vector = new Vector3(Mathf.DegToRad(rotation), 0f, 0f);

		Anchor.Rotation = rotation_vector;
	}

	public void OnTimeChanged(uint day, uint month, uint year, uint hour, uint minute)
	{
		CurrentHour = hour;
		CurrentMinute = minute;
	}

	private float CalculateRotation()
	{
		var current_time_in_minutes = CurrentMinute + CurrentHour * MinutesPerHour;
		var max_minutes_per_day = MinutesPerHour * HoursPerDay;

		float value = current_time_in_minutes / (float)max_minutes_per_day;

		return -(360.0f * value) + 90.0f;
	}
}
