use godot::{
    engine::{DirectionalLight3D, WorldEnvironment, WorldEnvironmentVirtual},
    prelude::{utilities::deg_to_rad, *},
};

struct SkyExtension;

#[gdextension]
unsafe impl ExtensionLibrary for SkyExtension {}

#[derive(GodotClass)]
#[class(init, base=WorldEnvironment)]
struct SkyExt {
    #[base]
    base: Base<WorldEnvironment>,

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

    pub directional_light: Option<Gd<DirectionalLight3D>>,
}

#[godot_api]
impl SkyExt {
    pub fn to_rotation_deg(&self) -> f64 {
        let current_time_in_minutes = (self.minute + self.hour * self.minutes_per_hour) as f64;
        let max_minutes_per_day = (self.minutes_per_hour * self.hours_per_day) as f64;

        let value = current_time_in_minutes / max_minutes_per_day;
        return -(360.0 * value) + 90.0;
    }

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
    fn time_init(day: u32, month: u32, year: u32, hour: u32, minute: u32);
}

#[godot_api]
impl WorldEnvironmentVirtual for SkyExt {
    fn ready(&mut self) {
        self.directional_light = self
            .base
            .find_child("DirectionalLight3D".into())
            .and_then(|x| x.try_cast::<DirectionalLight3D>());
        if self.directional_light.is_none() {
            godot_warn!("DirectionalLight3D missing!");
        }

        self.base.emit_signal(
            "time_init".into(),
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

        if self.minute >= self.minutes_per_hour {
            self.minute = 0;
            self.hour += 1;

            self.base
                .emit_signal("hour_passed".into(), &[self.hour.to_variant()]);
        }

        if self.hour >= self.hours_per_day {
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

        let rotation = self.to_rotation_deg();
        if let Some(light) = self.directional_light.as_mut() {
            light.set_rotation(Vector3::new(deg_to_rad(rotation) as f32, 0.0, 0.0))
        }
    }
}
