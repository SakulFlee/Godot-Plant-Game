use godot::{
    engine::{
        input::MouseMode, CharacterBody3D, InputEvent, InputEventMouseMotion, ProjectSettings,
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

    #[export]
    pub jump_velocity: f32,

    #[export]
    pub sanity_y_cutoff: f32,

    camera_origin: Option<Gd<Node3D>>,
    character_body: Option<Gd<CharacterBody3D>>,
}

#[godot_api]
impl Player3D {
    fn handle_mouse_movement_event(&mut self, event: Gd<InputEventMouseMotion>) {
        let mouse_sensitivity = self.mouse_sensitivity;

        // Y Rot
        if let Some(character_body) = self.character_body_mut() {
            character_body
                .rotate_y(deg_to_rad((-event.get_relative().x * mouse_sensitivity) as f64) as f32);
        }

        if let Some(camera_origin) = self.camera_origin_mut() {
            // X Rot
            camera_origin
                .rotate_x(deg_to_rad((-event.get_relative().y * mouse_sensitivity) as f64) as f32);

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
    }

    fn handle_movement(&mut self) {
        let jump_velocity = self.jump_velocity;
        let movement_speed = self.movement_speed;

        if let Some(character_body) = self.character_body_mut() {
            let input_vector = Input::singleton().get_vector(
                "move_left".into(),
                "move_right".into(),
                "move_forward".into(),
                "move_backward".into(),
            );
            let direction = (character_body.get_transform().basis
                * Vector3::new(input_vector.x, 0.0, input_vector.y))
            .normalized();

            let mut y = character_body.get_velocity().y;
            if Input::singleton().is_action_just_pressed("jump".into()) {
                y = jump_velocity;
            }

            character_body.set_velocity(Vector3::new(
                direction.x * movement_speed,
                y,
                direction.z * movement_speed,
            ));
        }
    }

    fn handle_gravity(&mut self, delta: f64) {
        let gravity: f64 = ProjectSettings::singleton()
            .get_setting("physics/3d/default_gravity".into())
            .to();

        if let Some(character_body) = self.character_body_mut() {
            if !character_body.is_on_floor() {
                let current_velocity = character_body.get_velocity();

                character_body.set_velocity(Vector3::new(
                    current_velocity.x,
                    current_velocity.y - (gravity * delta) as f32,
                    current_velocity.z,
                ));
            }
        }
    }

    fn sanity_check(&mut self) {
        let sanity_y_cutoff = self.sanity_y_cutoff;
        if let Some(character_body) = self.character_body_mut() {
            if character_body.get_position().y <= sanity_y_cutoff {
                godot_warn!("Sanity Check :: Returning player to island center!");

                character_body.set_position(Vector3::new(0.0, 5.0, 0.0));
            }
        }
    }

    #[allow(unused)]
    pub fn camera_origin(&self) -> Option<&Gd<Node3D>> {
        if self.camera_origin.is_none() {
            godot_warn!("Camera Origin is missing!");
        }
        self.camera_origin.as_ref()
    }

    #[allow(unused)]
    pub fn camera_origin_mut(&mut self) -> Option<&mut Gd<Node3D>> {
        if self.camera_origin.is_none() {
            godot_warn!("Camera Origin is missing!");
        }
        self.camera_origin.as_mut()
    }

    #[allow(unused)]
    pub fn character_body(&self) -> Option<&Gd<CharacterBody3D>> {
        if self.character_body.is_none() {
            godot_warn!("Character Body is missing!");
        }
        self.character_body.as_ref()
    }

    #[allow(unused)]
    pub fn character_body_mut(&mut self) -> Option<&mut Gd<CharacterBody3D>> {
        if self.character_body.is_none() {
            godot_warn!("Character Body is missing!");
        }
        self.character_body.as_mut()
    }
}

#[godot_api]
impl Node3DVirtual for Player3D {
    fn init(base: Base<Node3D>) -> Self {
        Self {
            base,
            movement_speed: 5.0,
            mouse_sensitivity: 0.35,
            jump_velocity: 5.0,
            sanity_y_cutoff: -250.0,
            camera_origin: None,
            character_body: None,
        }
    }

    fn ready(&mut self) {
        // Find and store Camera Origin
        self.camera_origin = self
            .base
            .find_child_ex("CameraOrigin".into())
            .recursive(true)
            .done()
            .and_then(|x| x.try_cast::<Node3D>());
        if self.camera_origin.is_none() {
            godot_warn!("Camera Origin missing!");
        }

        // Find and store Character Body
        self.character_body = self
            .base
            .find_child_ex("CharacterBody3D".into())
            .recursive(true)
            .done()
            .and_then(|x| x.try_cast::<CharacterBody3D>());
        if self.character_body.is_none() {
            godot_warn!("Character Body missing!");
        }

        // Capture mouse
        Input::singleton().set_mouse_mode(MouseMode::MOUSE_MODE_CAPTURED);
    }

    fn input(&mut self, event: Gd<InputEvent>) {
        if let Some(mouse_event) = event.clone().try_cast::<InputEventMouseMotion>() {
            self.handle_mouse_movement_event(mouse_event);
        }

        // TODO: Controller camera movement?
    }

    fn physics_process(&mut self, delta: f64) {
        // Check if we should quit
        if Input::singleton().is_action_pressed("quit".into()) {
            self.base.get_tree().unwrap().quit();
            return;
        }

        self.handle_movement();
        self.handle_gravity(delta);

        if let Some(character_body) = self.character_body_mut() {
            character_body.move_and_slide();
        }

        self.sanity_check();
    }
}
