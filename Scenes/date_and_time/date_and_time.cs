using Godot;
using System;

public partial class DateAndTime : Node
{
	[Export]
	public uint HoursPerDay { get; private set; } = 24;

	[Export]
	public uint MinutesPerHour { get; private set; } = 60;

	[Export]
	public uint DaysPerMonth { get; private set; } = 30;

	[Export]
	public uint MonthsPerYear { get; private set; } = 12;

	[Export]
	public float IncrementFactor { get; private set; } = 1;

	[Export]
	public float Time { get; private set; } = 0;

	[Export]
	public uint Hour { get; private set; } = 6;

	[Export]
	public uint Minute { get; private set; } = 0;

	[Export]
	public uint Day { get; private set; } = 1;

	[Export]
	public uint Month { get; private set; } = 1;

	[Export]
	public uint Year { get; private set; } = 1000;

	[Signal]
	public delegate void MinutePassedEventHandler(float time);

	[Signal]
	public delegate void HourPassedEventHandler(float time);

	[Signal]
	public delegate void DayPassedEventHandler(uint day);

	[Signal]
	public delegate void MonthPassedEventHandler(uint month);

	[Signal]
	public delegate void YearPassedEventHandler(uint year);

	[Signal]
	public delegate void TimeChangedEventHandler(uint day, uint month, uint year, uint hour, uint minute);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		EmitSignal(SignalName.TimeChanged, Day, Month, Year, Hour, Minute);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Calculate factor and add to time
		var factor = (float)delta * IncrementFactor;
		Time += factor;

		if (Time >= 1.0f)
		{
			Time -= 1.0f;
			Minute += 1;

			EmitSignal(SignalName.MinutePassed, Minute);
		}

		if (Minute > MinutesPerHour)
		{
			Minute = 0;
			Hour += 1;

			EmitSignal(SignalName.HourPassed, Hour);
		}

		if (Hour >= HoursPerDay)
		{
			Hour = 0;
			Day += 1;

			EmitSignal(SignalName.DayPassed, Day);
		}

		if (Day > DaysPerMonth)
		{
			Day = 1;
			Month += 1;

			EmitSignal(SignalName.MonthPassed, Month);
		}

		if (Month > MonthsPerYear)
		{
			Month = 1;
			Year += 1;

			EmitSignal(SignalName.YearPassed, Year);
		}

		EmitSignal(
			SignalName.TimeChanged,
			Day,
			Month,
			Year,
			Hour,
			Minute
		);
	}
}
