use godot::{engine::Node, prelude::*};

struct DateAndTimeExtension;

#[gdextension]
unsafe impl ExtensionLibrary for DateAndTimeExtension {}

#[derive(GodotClass)]
#[class(init, base=Node)]
struct DateAndTime {
    #[base]
    base: Base<Node>,

    #[export]
    #[init(default = 24)]
    pub hours_per_day: u32,

    #[export]
    #[init(default = 60)]
    pub minutes_per_hour: u32,

    #[export]
    #[init(default = 30)]
    pub days_per_month: u32,

    #[export]
    #[init(default = 12)]
    pub months_per_year: u32,

    #[export]
    #[init(default = 1.0)]
    pub increment_factor: f64,

    #[export]
    #[init(default = 0.0)]
    pub time: f64,

    #[export]
    #[init(default = 0)]
    pub hour: u32,

    #[export]
    #[init(default = 0)]
    pub minute: u32,

    #[export]
    #[init(default = 1)]
    pub day: u32,

    #[export]
    #[init(default = 1)]
    pub month: u32,

    #[export]
    #[init(default = 1000)]
    pub year: u32,
}

#[godot_api]
impl DateAndTime {
    #[signal]
    fn minute_passed(time: f64);

    #[signal]
    fn hour_passed(time: f64);

    #[signal]
    fn day_passed(day: u32);

    #[signal]
    fn month_passed(month: u32);

    #[signal]
    fn year_passed(year: u32);

    #[signal]
    fn time_changed(day: u32, month: u32, year: u32, hour: u32, minute: u32);
}

#[godot_api]
impl NodeVirtual for DateAndTime {
    fn ready(&mut self) {
        self.base.emit_signal(
            "time_changed".into(),
            &[
                self.day.to_variant(),
                self.month.to_variant(),
                self.year.to_variant(),
                self.hour.to_variant(),
                self.minute.to_variant(),
            ],
        );
    }

    fn process(&mut self, delta: f64) {
        // Calculate factor and add to time
        let factor = delta * self.increment_factor;
        self.time += factor;

        if self.time >= 1.0 {
            self.time -= 1.0;
            self.minute += 1;

            self.base
                .emit_signal("minute_passed".into(), &[self.minute.to_variant()]);
        }

        if self.minute > self.minutes_per_hour {
            self.minute = 0;
            self.hour += 1;

            self.base
                .emit_signal("hour_passed".into(), &[self.hour.to_variant()]);
        }

        if self.hour > self.hours_per_day {
            self.hour = 1;
            self.day += 1;

            self.base
                .emit_signal("day_passed".into(), &[self.day.to_variant()]);
        }

        if self.day > self.days_per_month {
            self.day = 1;
            self.month += 1;

            self.base
                .emit_signal("month_passed".into(), &[self.month.to_variant()]);
        }

        if self.month > self.months_per_year {
            self.month = 1;
            self.year += 1;

            self.base
                .emit_signal("year_passed".into(), &[self.year.to_variant()]);
        }
    }
}
