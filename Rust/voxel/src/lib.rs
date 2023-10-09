use std::fmt::{Display, Formatter, Result};
use godot::engine::GridMap;

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord)]
pub enum Voxel {
    Air,
    Grass,
    Dirt,
    Stone,
    Water,
}

impl Voxel {
    pub fn from_index(index: i32) -> Self {
        if index == 0 {
            Voxel::Grass
        } else if index == 1 {
            Voxel::Dirt
        } else if index == 2 {
            Voxel::Stone
        } else if index == 3 {
            Voxel::Water
        } else {
            Voxel::Air
        }
    }

    pub fn to_index(&self) -> i32 {
        match self {
            Voxel::Air => GridMap::INVALID_CELL_ITEM,
            Voxel::Grass => 0,
            Voxel::Dirt => 1,
            Voxel::Stone => 2,
            Voxel::Water => 3,
        }
    }

    pub fn count_as_neighbour(&self) -> bool {
        match self {
            Voxel::Water | Voxel::Air => false,
            _ => true,
        }
    }
}

impl Display for Voxel {
    fn fmt(&self, formatter: &mut Formatter<'_>) -> Result {
        formatter.write_fmt(format_args!("Voxel@{:?}", self))
    }
}
