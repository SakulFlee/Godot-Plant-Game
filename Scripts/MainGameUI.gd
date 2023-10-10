extends Control

var day = 0;
var month = 0;
var year = 0;
var hour = 0;
var minute = 0;

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	var date_and_time = find_child("DateAndTime");
	date_and_time.text = "Date: %02d.%02d.%04d\nTime: %02d:%02d" % [self.day, self.month, self.year, self.hour, self.minute];
	pass

func _on_sky_day_passed(day):
	self.day = day;

func _on_sky_hour_passed(time):
	self.hour = time;

func _on_sky_minute_passed(time):
	self.minute = time;

func _on_sky_month_passed(month):
	self.month = month;

func _on_sky_year_passed(year):
	self.year = year;

func _on_sky_time_init(day, month, year, hour, minute):
	self.day = day;
	self.month = month;
	self.year = year;
	self.hour = hour;
	self.minute = minute;
