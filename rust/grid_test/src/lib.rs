use godot::{
    engine::{GridMap, GridMapVirtual, MeshLibrary, QuadMesh},
    prelude::*,
};

struct MyExtension;

#[gdextension]
unsafe impl ExtensionLibrary for MyExtension {}

#[derive(GodotClass)]
#[class(base=GridMap)]
struct GridTest {
    #[base]
    base: Base<GridMap>,
}

#[godot_api]
impl GridTest {}

#[godot_api]
impl GridMapVirtual for GridTest {
    fn init(base: Base<GridMap>) -> Self {
        godot_print!("Grid Init");

        let mut s = Self { base };

        let mut mesh_library = MeshLibrary::new();

        mesh_library.set_item_mesh(1, QuadMesh::new().to_variant().to());
        mesh_library.set_item_name(1, "Test".into());

        s.base.set_mesh_library(mesh_library);

        s
    }
}
