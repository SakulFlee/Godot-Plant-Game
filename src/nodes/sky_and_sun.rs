use crate::{nodes::DateAndTime, utils::FindChildInScene};
use godot::{
    engine::{WorldEnvironment, WorldEnvironmentVirtual},
    prelude::{utilities::deg_to_rad, *},
};

#[derive(GodotClass)]
#[class(init, base=WorldEnvironment)]
struct SkyAndSun {
    #[base]
    base: Base<WorldEnvironment>,

    hours_per_day: u32,
    minutes_per_hour: u32,

    current_minute: u32,
    current_hour: u32,

    anchor: Option<Gd<Node3D>>,
}

#[godot_api]
impl SkyAndSun {
    pub fn to_rotation_deg(&mut self) -> Option<f64> {
        let current_time_in_minutes =
            (self.current_minute + self.current_hour * self.minutes_per_hour) as f64;
        let max_minutes_per_day = (self.minutes_per_hour * self.hours_per_day) as f64;

        let value = current_time_in_minutes / max_minutes_per_day;
        return Some(-(360.0 * value) + 90.0);
    }

    #[func]
    pub fn _on_time_change(&mut self, _day: u32, _month: u32, _year: u32, hour: u32, minute: u32) {
        self.current_minute = minute;
        self.current_hour = hour;
    }
}

#[godot_api]
impl WorldEnvironmentVirtual for SkyAndSun {
    fn ready(&mut self) {
        self.anchor = self
            .base
            .find_child("Anchor".into())
            .and_then(|x| x.try_cast::<Node3D>());
        if self.anchor.is_none() {
            godot_warn!("Anchor missing!");
        }

        if let Some(node) = self.base.find_child_in_scene("DateAndTime".into(), true) {
            if let Some(date_and_time) = node.try_cast::<DateAndTime>() {
                self.minutes_per_hour = date_and_time.get("minutes_per_hour".into()).to();
                self.hours_per_day = date_and_time.get("hours_per_day".into()).to();
            } else {
                godot_warn!("DateAndTime found, but wrong type!");
            }
        } else {
            godot_warn!("DateAndTime not found!");
        }
    }

    fn process(&mut self, _delta: f64) {
        if let Some(rotation) = self.to_rotation_deg() {
            if let Some(light) = self.anchor.as_mut() {
                light.set_rotation(Vector3::new(deg_to_rad(rotation) as f32, 0.0, 0.0))
            }
        }
    }
}
