use godot::{
    engine::{Sprite2D, Sprite2DVirtual},
    prelude::*,
};

struct MyExtension;

#[gdextension]
unsafe impl ExtensionLibrary for MyExtension {}

#[derive(GodotClass)]
#[class(base=Sprite2D)]
struct Player {
    speed: f64,
    angular_speed: f64,

    #[base]
    sprite: Base<Sprite2D>, // Optional!
}

#[godot_api]
impl Sprite2DVirtual for Player {
    fn init(sprite: Base<Sprite2D>) -> Self {
        godot_print!("Hello, world!"); // Prints to the Godot console

        Self {
            speed: 400.0,
            angular_speed: std::f64::consts::PI,
            sprite,
        }
    }

    fn physics_process(&mut self, delta: f64) {
        self.sprite.rotate((self.angular_speed * delta) as f32);
    }
}
