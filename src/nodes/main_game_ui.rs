use godot::{
    engine::{Control, RichTextLabel},
    prelude::*,
};

#[derive(GodotClass)]
#[class(init, base=Control)]
struct MainGameUI {
    #[base]
    base: Base<Control>,

    date_and_time_label: Option<Gd<RichTextLabel>>,
}

#[godot_api]
impl MainGameUI {
    #[func]
    fn _on_time_change(&mut self, day: u32, month: u32, year: u32, hour: u32, minute: u32) {
        if let Some(date_and_time_label) = self.date_and_time_label.as_mut() {
            date_and_time_label.set_text(
                format!("Date: {day:0>2}.{month:0>2}.{year:0>4}\nTime: {hour:0>2}:{minute:0>2}")
                    .into(),
            );
        }
    }
}

#[godot_api]
impl NodeVirtual for MainGameUI {
    fn ready(&mut self) {
        if let Some(node) = self.base.find_child("DateAndTimeLabel".into()) {
            if let Some(date_and_time_label) = node.try_cast::<RichTextLabel>() {
                self.date_and_time_label = Some(date_and_time_label);
            } else {
                godot_warn!("DateAndTimeLabel found, but wrong type!");
            }
        } else {
            godot_warn!("DateAndTimeLabel not found!");
        }
    }
}
