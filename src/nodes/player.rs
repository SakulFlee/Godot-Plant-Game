use crate::voxel::Voxel;
use godot::{
    engine::{
        input::MouseMode, CharacterBody3D, GridMap, InputEvent, InputEventMouseMotion,
        ProjectSettings, RayCast3D,
    },
    prelude::{
        utilities::{clampf, deg_to_rad},
        *,
    },
};

#[derive(GodotClass)]
#[class(base=Node3D)]
struct Player {
    #[base]
    base: Base<Node3D>,

    #[export]
    pub movement_speed: f32,

    #[export]
    pub mouse_sensitivity: f32,

    #[export]
    pub controller_sensitivity: f32,

    #[export]
    pub jump_velocity: f32,

    #[export]
    pub sanity_y_cutoff: f32,

    camera_origin: Option<Gd<Node3D>>,
    character_body: Option<Gd<CharacterBody3D>>,
    ray_cast_front: Option<Gd<RayCast3D>>,

    can_jump: bool,

    last_selector_cell_position: Option<Vector3i>,
}

#[godot_api]
impl Player {
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

    fn handle_joypad_camera(&mut self) {
        let joypad_sensitivity = self.controller_sensitivity;

        let axis_horizontal = Input::singleton().get_axis("look_up".into(), "look_down".into());
        let axis_vertical = Input::singleton().get_axis("look_left".into(), "look_right".into());

        if let Some(character_body) = self.character_body_mut() {
            character_body
                .rotate_y(deg_to_rad((-axis_vertical * joypad_sensitivity) as f64) as f32);
        }

        if let Some(camera_origin) = self.camera_origin_mut() {
            // X Rot
            camera_origin
                .rotate_x(deg_to_rad((-axis_horizontal * joypad_sensitivity) as f64) as f32);

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

        let can_jump = self.can_jump;
        let mut restrict_jump_after = false;

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
            if Input::singleton().is_action_just_pressed("jump".into()) && can_jump {
                y = jump_velocity;
                restrict_jump_after = true;
            }

            character_body.set_velocity(Vector3::new(
                direction.x * movement_speed,
                y,
                direction.z * movement_speed,
            ));
        }

        if restrict_jump_after {
            self.can_jump = false;
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
            } else {
                self.can_jump = true;
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

    #[allow(unused)]
    pub fn ray_cast_front(&self) -> Option<&Gd<RayCast3D>> {
        if self.ray_cast_front.is_none() {
            godot_warn!("RayCast Front is missing!");
        }
        self.ray_cast_front.as_ref()
    }

    #[allow(unused)]
    pub fn ray_cast_front_mut(&mut self) -> Option<&mut Gd<RayCast3D>> {
        if self.ray_cast_front.is_none() {
            godot_warn!("RayCast Front is missing!");
        }
        self.ray_cast_front.as_mut()
    }
}

#[godot_api]
impl Node3DVirtual for Player {
    // TODO: Init instead ...
    fn init(base: Base<Node3D>) -> Self {
        Self {
            base,
            movement_speed: 5.0,
            mouse_sensitivity: 0.35,
            controller_sensitivity: 5.0,
            jump_velocity: 5.0,
            sanity_y_cutoff: -250.0,
            camera_origin: None,
            character_body: None,
            ray_cast_front: None,
            can_jump: true,
            last_selector_cell_position: None,
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

        // Find and store RayCastFront
        self.ray_cast_front = self
            .base
            .find_child_ex("RayCastFront".into())
            .recursive(true)
            .done()
            .and_then(|x| x.try_cast::<RayCast3D>());
        if self.character_body.is_none() {
            godot_warn!("RayCastFront missing!");
        }

        // Capture mouse
        Input::singleton().set_mouse_mode(MouseMode::MOUSE_MODE_CAPTURED);
    }

    fn input(&mut self, event: Gd<InputEvent>) {
        if let Some(mouse_event) = event.try_cast::<InputEventMouseMotion>() {
            self.handle_mouse_movement_event(mouse_event);
        }
    }

    fn process(&mut self, _delta: f64) {
        if let Some(ray) = self.ray_cast_front() {
            if ray.is_colliding() {
                if let Some(mut grid_map) = ray.get_collider().unwrap().try_cast::<GridMap>() {
                    let collision_point = ray.get_collision_point();

                    let hit_point = grid_map.local_to_map(collision_point);
                    let below_hit_point =
                        grid_map.local_to_map(collision_point) + Vector3i::new(0, -1, 0);

                    let voxel_below_hit =
                        Voxel::from_index(grid_map.get_cell_item(below_hit_point));

                    if voxel_below_hit != Voxel::Air {
                        if let Some(last_point) = self.last_selector_cell_position {
                            grid_map.set_cell_item(last_point, Voxel::Air.to_index());
                        }

                        self.last_selector_cell_position = Some(hit_point);

                        grid_map.set_cell_item(hit_point, Voxel::Selector.to_index());

                        // TODO: Store this as a variable
                        // TODO: Do some interaction with "can_become_farmland" and "can_be_planted" based on RMB or something
                        // TODO: Move into it's own functions, call from here
                    }
                }
            }
        }
    }

    fn physics_process(&mut self, delta: f64) {
        // Check if we should quit
        if Input::singleton().is_action_pressed("quit".into()) {
            self.base.get_tree().unwrap().quit();
            return;
        }

        self.handle_movement();
        self.handle_joypad_camera();
        self.handle_gravity(delta);

        if let Some(character_body) = self.character_body_mut() {
            character_body.move_and_slide();
        }

        self.sanity_check();
    }
}
