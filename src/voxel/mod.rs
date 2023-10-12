use godot::{
    engine::MeshLibrary,
    prelude::{load, Gd},
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
        match self {
            Self::Farmland => true,
            _ => false,
        }
    }

    pub fn can_become_farmland(&self) -> bool {
        match self {
            Self::Grass | Self::Dirt => true,
            _ => false,
        }
    }
}

impl Display for Voxel {
    fn fmt(&self, formatter: &mut Formatter<'_>) -> Result {
        formatter.write_fmt(format_args!(
            "Voxel@{:?} (Plantable: {}) (Farmable: {})",
            self,
            self.plantable(),
            self.can_become_farmland()
        ))
    }
}

#[derive(Debug, Clone)]
pub struct VoxelLibrary {
    mesh_library: Gd<MeshLibrary>,
}

impl VoxelLibrary {
    const NO_NEIGHBOUR: &'static [&'static str] = &["Water", "Air", "Selector"];
    const HARVESTABLE: &'static [&'static str] = &["Radish"];
    const PLOWABLE: &'static [&'static str] = &["Grass", "Dirt"];

    pub fn singleton() -> &'static Self {
        unsafe {
            if INSTANCE.is_none() {
                let mesh_library: Gd<MeshLibrary> = load("res:///mesh_library/voxels.tres");

                let voxel_library = VoxelLibrary::new(mesh_library);

                INSTANCE = Some(voxel_library);
            }

            INSTANCE.as_ref().unwrap()
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
        }
    }

    pub fn by_name(&self, name: &str) -> Voxel {
        // TODO: Test what happens if the label doesn't exist?
        let id = self.mesh_library.find_item_by_name(name.into());

        self.by_id(id)
    }

    pub fn by_id(&self, id: i32) -> Voxel {
        let name = self.mesh_library.get_item_name(id).to_string();

        Voxel {
            id,
            name: name.clone(),
            is_neighbour: Self::NO_NEIGHBOUR.contains(&name.as_str()),
            can_be_harvested: Self::HARVESTABLE.contains(&name.as_str()),
            can_be_plowed: Self::PLOWABLE.contains(&name.as_str()),
        }
    }
}
