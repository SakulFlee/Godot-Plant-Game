use godot::{
    engine::{GridMap, GridMapVirtual},
    prelude::*,
};

struct MyExtension;

#[gdextension]
unsafe impl ExtensionLibrary for MyExtension {}

#[derive(GodotClass)]
#[class(base=GridMap)]
struct IslandGen {
    #[base]
    _base: Base<GridMap>,
}

#[godot_api]
impl IslandGen {}

#[godot_api]
impl GridMapVirtual for IslandGen {
    fn init(mut base: Base<GridMap>) -> Self {
        godot_print!("Grid Init");

        base.set_cell_size(Vector3::new(0.5, 0.5, 0.5));
        base.set_cell_scale(0.5);
        base.clear();

        let mesh_library = match try_load("res://Mesh Library/Voxels.tres") {
            Some(mesh_library) => mesh_library,
            None => return Self { _base: base },
        };
        base.set_mesh_library(mesh_library);

        // let mesh_index = base
        //     .get_mesh_library()
        //     .unwrap()
        //     .find_item_by_name("Default".into());
        
        for x in -10..=10 {
            for y in -10..=0 {
                for z in -10..=10 {
                    let cell_position = Vector3i::new(x, y, z);

                    let distance = Vector3::new(x as f32, y as f32, z as f32)
                        .distance_squared_to(Vector3::ZERO);

                    if distance <= 10.0f32.powf(2.0) {
                        let mesh_index: i32;

                        if y == 0 {
                            mesh_index = 0;
                        } else if y < 0 && y >= -5 {
                            mesh_index = 1;
                        } else {
                            mesh_index = 2;
                        }

                        base.set_cell_item(cell_position, mesh_index);
                    }
                }
            }
        }

        Self { _base: base }
    }
}
