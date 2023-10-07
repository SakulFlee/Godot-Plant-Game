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
    base: Base<GridMap>,
}

#[godot_api]
impl IslandGen {}

#[godot_api]
impl GridMapVirtual for IslandGen {
    fn init(mut base: Base<GridMap>) -> Self {
        godot_print!("Grid Init");

        base.set_cell_size(Vector3::new(1.0, 1.0, 1.0));
        base.set_cell_scale(0.5);
        base.clear();

        let mesh_library = load("res://Mesh Library/Voxels.tres");
        base.set_mesh_library(mesh_library);

        for x in 0..10 {
            for y in 0..10 {
                for z in 0..10 {
                    let position = Vector3i::new(x, y, z);
                    let mesh_index = base
                        .get_mesh_library()
                        .unwrap()
                        .find_item_by_name("Default".into());

                    godot_print!("Index: {}", mesh_index);

                    base.set_cell_item(position, mesh_index);
                }
            }
        }

        Self { base }
    }
}
