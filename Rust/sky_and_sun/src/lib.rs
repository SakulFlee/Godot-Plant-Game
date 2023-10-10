use godot::{
    engine::{WorldEnvironment, WorldEnvironmentVirtual},
    prelude::{utilities::deg_to_rad, *},
};
use utils::FindChildInScene;

struct SkyAndSunExtension;

#[gdextension]
unsafe impl ExtensionLibrary for SkyAndSunExtension {}

#[derive(GodotClass)]
#[class(init, base=WorldEnvironment)]
struct SkyAndSun {
    #[base]
    base: Base<WorldEnvironment>,

    time: Option<Gd<Node>>,

    anchor: Option<Gd<Node3D>>,
}

#[godot_api]
impl SkyAndSun {
    pub fn to_rotation_deg(&mut self) -> Option<f64> {
        if self.time.is_none() {
            godot_warn!("Time is missing!");
            return None;
        }

        let time = self.time.as_mut().unwrap();

        godot_print!("Properties:");
        // TODO: Cast first, then try again?
        let properties = time.get_meta_list();
        for index in 0..properties.len() {
            let property = properties.get(index);
            godot_print!("{:#?}", property);
        }

        let minutes_per_hour: u32 = time.call("minutes_per_hour".into(), &[]).to();
        let hours_per_day: u32 = time.call("hours_per_day".into(), &[]).to();
        let minute: u32 = time.call("minute".into(), &[]).to();
        let hour: u32 = time.call("hour".into(), &[]).to();

        let current_time_in_minutes = (minute + hour * minutes_per_hour) as f64;
        let max_minutes_per_day = (minutes_per_hour * hours_per_day) as f64;

        let value = current_time_in_minutes / max_minutes_per_day;
        return Some(-(360.0 * value) + 90.0);
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

        self.time = self.base.find_child_in_scene("DateAndTime".into(), true);
        if self.time.is_none() {
            godot_warn!("Time not found!");
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
