use godot::{
    engine::{MeshLibrary, ResourceLoader},
    prelude::{godot_warn, Gd},
};
use std::fmt::{Display, Formatter, Result};

static mut INSTANCE: Option<VoxelLibrary> = None;

// TODO: Move out
#[derive(Debug, Clone)]
pub struct Voxel {
    id: i32,
    name: String,
    is_neighbour: bool,
    can_be_harvested: bool,
    can_be_plowed: bool,
    plantable: bool,
}

impl Voxel {
    pub fn id(&self) -> i32 {
        self.id
    }

    pub fn name(&self) -> &str {
        self.name.as_ref()
    }

    pub fn is_neighbour(&self) -> bool {
        self.is_neighbour
    }

    pub fn can_be_harvested(&self) -> bool {
        self.can_be_harvested
    }

    pub fn can_be_plowed(&self) -> bool {
        self.can_be_plowed
    }

    pub fn plantable(&self) -> bool {
        self.plantable
    }
}

impl Display for Voxel {
    fn fmt(&self, formatter: &mut Formatter<'_>) -> Result {
        formatter.write_fmt(format_args!(
            "Voxel@{} #{} (Neighbour: {}, Harvestable: {}, Plowable: {})",
            self.name, self.id, self.is_neighbour, self.can_be_harvested, self.can_be_plowed,
        ))
    }
}

#[derive(Debug, Clone)]
pub struct VoxelLibrary {
    mesh_library: Gd<MeshLibrary>,
}

impl VoxelLibrary {
    const VOXEL_LIBRARY_PATH: &'static str = "res:///mesh_library/voxels.tres";
    const NO_NEIGHBOUR: &'static [&'static str] = &["Water", "Air", "Selector"];
    const HARVESTABLE: &'static [&'static str] = &["Radish"];
    const PLOWABLE: &'static [&'static str] = &["Grass", "Dirt"];
    const PLANTABLE: &'static [&'static str] = &["Farmland"];

    pub fn singleton() -> &'static Self {
        unsafe {
            if INSTANCE.is_none() {
                if let Some(mesh_library) = ResourceLoader::singleton()
                    .load_ex(Self::VOXEL_LIBRARY_PATH.into())
                    .done()
                    .and_then(|res| res.try_cast::<MeshLibrary>())
                {
                    let voxel_library = VoxelLibrary::new(mesh_library);

                    INSTANCE = Some(voxel_library);
                } else {
                    godot_warn!("Failed loading Voxel Library!");
                }
            }

            INSTANCE
                .as_ref()
                .expect("Failed constructing singleton VoxelLibrary!")
        }
    }

    fn new(mesh_library: Gd<MeshLibrary>) -> Self {
        Self { mesh_library }
    }

    pub fn empty_id() -> i32 {
        -1
    }

    pub fn empty_voxel() -> Voxel {
        Voxel {
            id: Self::empty_id(),
            name: "EMPTY".into(),
            is_neighbour: false,
            can_be_harvested: false,
            can_be_plowed: false,
            plantable: false,
        }
    }

    pub fn by_name(&self, name: &str) -> Voxel {
        // TODO: Test what happens if the label doesn't exist?
        let id = self.mesh_library.find_item_by_name(name.into());

        self.by_id(id)
    }

    pub fn by_id(&self, id: i32) -> Voxel {
        if id < 0 {
            return Self::empty_voxel();
        }

        let name = self.mesh_library.get_item_name(id).to_string();

        Voxel {
            id,
            name: name.clone(),
            is_neighbour: !Self::NO_NEIGHBOUR.contains(&name.as_str()),
            can_be_harvested: Self::HARVESTABLE.contains(&name.as_str()),
            can_be_plowed: Self::PLOWABLE.contains(&name.as_str()),
            plantable: Self::PLANTABLE.contains(&name.as_str()),
        }
    }
}
