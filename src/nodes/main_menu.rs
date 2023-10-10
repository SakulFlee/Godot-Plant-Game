use godot::{engine::Control, prelude::*};

#[derive(GodotClass)]
#[class(init, base=Control)]
struct MainMenu {
    #[base]
    base: Base<Control>,
}

#[godot_api]
impl MainMenu {
    #[func]
    fn start_button_pressed(&mut self) {
        self.base
            .get_tree()
            .unwrap()
            .change_scene_to_file("res:///scenes/main_game.tscn".into());
    }
}

#[godot_api]
impl NodeVirtual for MainMenu {
    fn ready(&mut self) {}
}
