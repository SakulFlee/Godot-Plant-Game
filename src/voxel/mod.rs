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
    Farmland,
}

impl Voxel {
    pub fn from_index(index: i32) -> Self {
        if index == 0 {
            Self::Selector
        } else if index == 1 {
            Self::Dirt
        } else if index == 2 {
            Self::Grass
        } else if index == 3 {
            Self::Stone
        } else if index == 4 {
            Self::Water
        } else if index == 5 {
            Self::Farmland
        } else {
            Self::Air
        }
    }

    pub fn to_index(&self) -> i32 {
        match self {
            Self::Air => GridMap::INVALID_CELL_ITEM,
            Self::Selector => 0,
            Self::Grass => 1,
            Self::Dirt => 2,
            Self::Stone => 3,
            Self::Water => 4,
            Self::Farmland => 5,
        }
    }

    pub fn count_as_neighbour(&self) -> bool {
        match self {
            Self::Water | Self::Air | Self::Selector => false,
            _ => true,
        }
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
        formatter.write_fmt(format_args!("Voxel@{:?}", self))
    }
}
