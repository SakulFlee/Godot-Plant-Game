use godot::prelude::{gdextension, ExtensionLibrary};

pub mod nodes;
pub mod utils;
pub mod voxel;

struct GodotRustExtension;

#[gdextension]
unsafe impl ExtensionLibrary for GodotRustExtension {}
