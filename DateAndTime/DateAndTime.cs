using Godot;

public partial class DateAndTime : Node
{
	[Export]
	public uint hoursPerDay { get; private set; } = 24;

	[Export]
	public uint minutesPerHour { get; private set; } = 60;

	[Export]
	public uint daysPerMonth { get; private set; } = 30;

	[Export]
	public uint monthsPerYear { get; private set; } = 12;

	[Export]
	public float incrementFactor { get; private set; } = 1;

	[Export]
	public float time { get; private set; } = 0;

	[Export]
	public uint hour { get; private set; } = 6;

	[Export]
	public uint minute { get; private set; } = 0;

	[Export]
	public uint day { get; private set; } = 1;

	[Export]
	public uint month { get; private set; } = 1;

	[Export]
	public uint year { get; private set; } = 1000;

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
		EmitSignal(SignalName.TimeChanged, day, month, year, hour, minute);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Calculate factor and add to time
		var factor = (float)delta * incrementFactor;
		time += factor;

		if (time >= 1.0f)
		{
			time -= 1.0f;
			minute += 1;

			EmitSignal(SignalName.MinutePassed, minute);
		}

		if (minute > minutesPerHour)
		{
			minute = 0;
			hour += 1;

			EmitSignal(SignalName.HourPassed, hour);
		}

		if (hour >= hoursPerDay)
		{
			hour = 0;
			day += 1;

			EmitSignal(SignalName.DayPassed, day);
		}

		if (day > daysPerMonth)
		{
			day = 1;
			month += 1;

			EmitSignal(SignalName.MonthPassed, month);
		}

		if (month > monthsPerYear)
		{
			month = 1;
			year += 1;

			EmitSignal(SignalName.YearPassed, year);
		}

		EmitSignal(
			SignalName.TimeChanged,
			day,
			month,
			year,
			hour,
			minute
		);
	}
}
