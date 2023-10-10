use godot::engine::GridMap;
use std::fmt::{Display, Formatter, Result};

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord)]
pub enum Voxel {
    Air,
    Selector,
    Grass,
    Dirt,
    Stone,
    Water,
}

impl Voxel {
    pub fn from_index(index: i32) -> Self {
        if index == 0 {
            Voxel::Selector
        } else if index == 1 {
            Voxel::Dirt
        } else if index == 2 {
            Voxel::Grass
        } else if index == 3 {
            Voxel::Stone
        } else if index == 4 {
            Voxel::Water
        } else {
            Voxel::Air
        }
    }

    pub fn to_index(&self) -> i32 {
        match self {
            Voxel::Air => GridMap::INVALID_CELL_ITEM,
            Self::Selector => 0,
            Voxel::Grass => 1,
            Voxel::Dirt => 2,
            Voxel::Stone => 3,
            Voxel::Water => 4,
        }
    }

    pub fn count_as_neighbour(&self) -> bool {
        match self {
            Voxel::Water | Voxel::Air | Voxel::Selector => false,
            _ => true,
        }
    }
}

impl Display for Voxel {
    fn fmt(&self, formatter: &mut Formatter<'_>) -> Result {
        formatter.write_fmt(format_args!("Voxel@{:?}", self))
    }
}
