using Godot;

public partial class HUD : Control
{
	private RichTextLabel? DateAndTimeLabel;

	public override void _Ready()
	{
		DateAndTimeLabel = GetNode<RichTextLabel>("DateAndTimeLabel");
	}

	public void OnTimeChanged(uint day, uint month, uint year, uint hour, uint minute)
	{
		string local_day = day.ToString().PadLeft(2, '0');
		string local_month = month.ToString().PadLeft(2, '0');
		string local_year = year.ToString().PadLeft(4, '0');
		string local_hour = hour.ToString().PadLeft(2, '0');
		string local_minute = minute.ToString().PadLeft(2, '0');

		DateAndTimeLabel!.Text = $"Date: {local_day}.{local_month}.{local_year}\nTime: {local_hour}:{local_minute}";
	}
}
