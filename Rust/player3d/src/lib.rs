use godot::{
    engine::{
        input::MouseMode, CharacterBody3D, InputEvent, InputEventJoypadMotion, InputEventKey,
        InputEventMouseMotion,
    },
    prelude::{
        utilities::{clampf, deg_to_rad},
        *,
    },
};

struct Player3DExtension;

#[gdextension]
unsafe impl ExtensionLibrary for Player3DExtension {}

#[derive(GodotClass)]
#[class(base=Node3D)]
struct Player3D {
    #[base]
    base: Base<Node3D>,

    #[export]
    pub movement_speed: f32,

    #[export]
    pub mouse_sensitivity: f32,
}

#[godot_api]
impl Player3D {
    fn find_camera_origin(&self) -> Option<Gd<Node3D>> {
        let camera_origin: Option<Gd<Node3D>> = match self
            .base
            .find_child_ex("CameraOrigin".into())
            .recursive(true)
            .done()
        {
            Some(child) => {
                let node: Option<Gd<Node3D>> = child.try_cast();
                node
            }
            None => None,
        };
        camera_origin
    }

    fn find_camera(&self) -> Option<Gd<Camera3D>> {
        let camera_origin: Option<Gd<Camera3D>> = match self
            .base
            .find_child_ex("Camera3D".into())
            .recursive(true)
            .done()
        {
            Some(child) => {
                let node: Option<Gd<Camera3D>> = child.try_cast();
                node
            }
            None => None,
        };
        camera_origin
    }

    fn find_character_body(&self) -> Option<Gd<CharacterBody3D>> {
        let camera_origin: Option<Gd<CharacterBody3D>> = match self
            .base
            .find_child_ex("CharacterBody3D".into())
            .recursive(true)
            .done()
        {
            Some(child) => {
                let node: Option<Gd<CharacterBody3D>> = child.try_cast();
                node
            }
            None => None,
        };
        camera_origin
    }

    fn handle_mouse_movement_event(&mut self, event: Gd<InputEventMouseMotion>) {
        // Y Rot
        if let Some(mut character_body) = self.find_character_body() {
            character_body.rotate_y(deg_to_rad(
                (-event.get_relative().x * self.mouse_sensitivity) as f64,
            ) as f32);
        } else {
            godot_warn!("Character Body missing!");
        }

        let pivot = self.find_camera_origin(); // TODO: Optimize
        if pivot.is_none() {
            godot_warn!("Pivot/Camera Origin is missing!");
            return;
        }

        let mut camera_origin = pivot.unwrap();

        // X Rot
        camera_origin
            .rotate_x(deg_to_rad((-event.get_relative().y * self.mouse_sensitivity) as f64) as f32);

        // CLAMP
        let current_rotation = camera_origin.get_rotation();
        camera_origin.set_rotation(Vector3::new(
            clampf(
                current_rotation.x as f64,
                deg_to_rad(-45.0),
                deg_to_rad(45.0),
            ) as f32,
            current_rotation.y,
            current_rotation.z,
        ));
    }

    fn handle_movement(&mut self) {
        if let Some(mut character_body) = self.find_character_body() {
            let input_vector = Input::singleton().get_vector(
                "move_left".into(),
                "move_right".into(),
                "move_forward".into(),
                "move_backward".into(),
            );
            let direction = (character_body.get_transform().basis
                * Vector3::new(input_vector.x, 0.0, input_vector.y))
            .normalized();

            character_body.set_velocity(Vector3::new(
                direction.x * self.movement_speed,
                0.0,
                direction.z * self.movement_speed,
            ));
        } else {
            godot_warn!("CharacterBody Origin is missing!");
        }
    }
}

#[godot_api]
impl Node3DVirtual for Player3D {
    fn init(base: Base<Node3D>) -> Self {
        Self {
            base,
            movement_speed: 5.0,
            mouse_sensitivity: 0.5,
        }
    }

    fn ready(&mut self) {
        let camera_origin = self.find_camera_origin();
        if camera_origin.is_none() {
            godot_error!("Camera Origin is missing!");
        }

        let mut input = Input::singleton();
        input.set_mouse_mode(MouseMode::MOUSE_MODE_CAPTURED);
    }

    fn input(&mut self, event: Gd<InputEvent>) {
        if let Some(mouse_event) = event.clone().try_cast::<InputEventMouseMotion>() {
            self.handle_mouse_movement_event(mouse_event);
        }
    }

    fn physics_process(&mut self, delta: f64) {
        if Input::singleton().is_action_pressed("quit".into()) {
            self.base.get_tree().unwrap().quit();
            return;
        }

        let character_body = self.find_character_body();
        if character_body.is_none() {
            godot_warn!("Character Body is missing!");
            return;
        }

        self.handle_movement();
        character_body.unwrap().move_and_slide();

        // TODO: Gravity
        // TODO: Jump
    }
}
