use godot::engine::GridMap;

pub enum Voxel {
    None,
    Air,
    Grass,
    Dirt,
    Stone,
    Water,
}

impl Voxel {
    pub fn to_index(&self) -> i32 {
        match self {
            Voxel::None | Voxel::Air => GridMap::INVALID_CELL_ITEM,
            Voxel::Grass => 0,
            Voxel::Dirt => 1,
            Voxel::Stone => 2,
            Voxel::Water => 3,
        }
    }
}
