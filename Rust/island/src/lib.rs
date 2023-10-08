use godot::{
    engine::{GridMap, GridMapVirtual},
    prelude::*,
};
use noise::{
    utils::{NoiseMap, NoiseMapBuilder, PlaneMapBuilder},
    Billow, Perlin,
};

mod voxels;
use voxels::*;

struct MyExtension;

#[gdextension]
unsafe impl ExtensionLibrary for MyExtension {}

#[derive(GodotClass)]
#[class(tool, base=GridMap)]
struct Island {
    needs_update: bool,

    #[base]
    base: Base<GridMap>,

    // General Terrain
    #[export(range = (5.0, 100.0))]
    #[var(get = radius, set = set_radius)]
    pub radius: u32,
    #[export]
    #[var(get = below_ground_factor, set = set_below_ground_factor)]
    pub below_ground_factor: f64,
    #[export]
    #[var(get = terrain_indent_factor, set = set_terrain_indent_factor)]
    pub terrain_indent_factor: f64,

    // Terrain A
    #[export]
    #[var(get = terrain_seed_a, set = set_terrain_seed_a)]
    pub terrain_seed_a: u32,
    #[export(range = (1.0, 6.0))]
    #[var(get = octaves_a, set = set_octaves_a)]
    pub octaves_a: u32,
    #[export]
    #[var(get = frequency_a, set = set_frequency_a)]
    pub frequency_a: f64,
    #[export]
    #[var(get = lacunarity_a, set = set_lacunarity_a)]
    pub lacunarity_a: f64,
    #[export]
    #[var(get = persistence_a, set = set_persistence_a)]
    pub persistence_a: f64,

    // Terrain B
    #[export]
    #[var(get = terrain_seed_b, set = set_terrain_seed_b)]
    pub terrain_seed_b: u32,
    #[export(range = (1.0, 6.0))]
    #[var(get = octaves_b, set = set_octaves_b)]
    pub octaves_b: u32,
    #[export]
    #[var(get = frequency_b, set = set_frequency_b)]
    pub frequency_b: f64,
    #[export]
    #[var(get = lacunarity_b, set = set_lacunarity_b)]
    pub lacunarity_b: f64,
    #[export]
    #[var(get = persistence_b, set = set_persistence_b)]
    pub persistence_b: f64,
}

#[godot_api]
impl Island {
    fn clear(&mut self) {
        godot_print!("Clear");
        self.base.clear();
    }

    fn setup(&mut self) {
        godot_print!("Setup");
        self.base.set_cell_size(Vector3::new(1.0, 1.0, 1.0));
        self.base.set_cell_scale(1.0);

        let mesh_library = match try_load("res://Mesh Library/Voxels.tres") {
            Some(mesh_library) => mesh_library,
            None => {
                return;
            }
        };
        self.base.set_mesh_library(mesh_library);
    }

    fn make_noise_map(
        seed: u32,
        radius: u32,
        octaves: usize,
        frequency: f64,
        lacunarity: f64,
        persistence: f64,
    ) -> NoiseMap {
        let mut billow = Billow::<Billow<Perlin>>::new(seed);
        billow.octaves = octaves;
        billow.frequency = frequency;
        billow.lacunarity = lacunarity;
        billow.persistence = persistence;
        PlaneMapBuilder::<_, 2>::new(billow)
            .set_size((radius * 2) as usize, (radius * 2) as usize)
            .set_x_bounds(-1.0, 1.0)
            .set_y_bounds(-1.0, 1.0)
            .build()
    }

    fn island(&mut self) {
        let terrain_noise_a = Self::make_noise_map(
            self.terrain_seed_a(),
            self.radius(),
            self.octaves_a() as usize,
            self.frequency_a(),
            self.lacunarity_a(),
            self.persistence_a(),
        );
        let terrain_noise_b = Self::make_noise_map(
            self.terrain_seed_b(),
            self.radius(),
            self.octaves_b() as usize,
            self.frequency_b(),
            self.lacunarity_b(),
            self.persistence_b(),
        );

        let size = self.radius as i32;
        let range = (self.radius as f32).powf(2.0);

        for x in -size..=size {
            for z in -size..=size {
                let distance =
                    Vector3::new(x as f32, 0.0, z as f32).distance_squared_to(Vector3::ZERO);

                if distance > range {
                    // If the distance is not within (<=) range, skip!
                    continue;
                }

                let terrain_noise_a =
                    terrain_noise_a.get_value((x + size) as usize, (z + size) as usize);
                let terrain_noise_b =
                    terrain_noise_b.get_value((x + size) as usize, (z + size) as usize);
                let noise_below_ground =
                    (terrain_noise_a * terrain_noise_b) * self.below_ground_factor();
                let noise_terrain_indent =
                    (terrain_noise_b - terrain_noise_a) * self.terrain_indent_factor();

                for y in -(noise_below_ground as i32)..=(noise_terrain_indent as i32) {
                    let y_distance = Vector3::new(x as f32, y as f32, z as f32)
                        .distance_squared_to(Vector3::ZERO);

                    if y_distance > range {
                        // If the distance is not within (<=) range, skip!
                        continue;
                    }

                    let mesh_index: Voxel;
                    if y == 0 {
                        mesh_index = Voxel::Grass;
                    } else if y < 0 && y >= -3 {
                        mesh_index = Voxel::Dirt;
                    } else {
                        mesh_index = Voxel::Stone;
                    }

                    // TODO: Add water AFTER this based on a water level height level!

                    let cell_position = Vector3i::new(x, y, z);
                    self.base
                        .set_cell_item(cell_position, mesh_index.to_index());
                }
            }
        }
    }

    fn cull(&mut self) {
        godot_print!("Cull");

        let mut to_be_removed: Vec<Vector3i> = Vec::new();

        let used_cells = self.base.get_used_cells();
        let count_before = used_cells.len();
        for index in 0..count_before {
            let position = used_cells.get(index);

            let mut neighbours = 0;

            if self
                .base
                .get_cell_item(Vector3i::new(position.x + 1, position.y, position.z))
                != GridMap::INVALID_CELL_ITEM
            {
                neighbours += 1;
            }
            if self
                .base
                .get_cell_item(Vector3i::new(position.x - 1, position.y, position.z))
                != GridMap::INVALID_CELL_ITEM
            {
                neighbours += 1;
            }
            if self
                .base
                .get_cell_item(Vector3i::new(position.x, position.y + 1, position.z))
                != GridMap::INVALID_CELL_ITEM
            {
                neighbours += 1;
            }
            if self
                .base
                .get_cell_item(Vector3i::new(position.x, position.y - 1, position.z))
                != GridMap::INVALID_CELL_ITEM
            {
                neighbours += 1;
            }
            if self
                .base
                .get_cell_item(Vector3i::new(position.x, position.y, position.z + 1))
                != GridMap::INVALID_CELL_ITEM
            {
                neighbours += 1;
            }
            if self
                .base
                .get_cell_item(Vector3i::new(position.x, position.y, position.z - 1))
                != GridMap::INVALID_CELL_ITEM
            {
                neighbours += 1;
            }

            if neighbours == 6 {
                to_be_removed.push(position);
            }
        }

        godot_print!("To be removed: {}", to_be_removed.len());
        godot_print!("Before: {count_before}");

        for position in to_be_removed {
            self.base.set_cell_item(position, Voxel::Air.to_index());
        }

        godot_print!("After: {}", self.base.get_used_cells().len());
    }

    #[func]
    pub fn below_ground_factor(&self) -> f64 {
        self.below_ground_factor
    }

    #[func]
    pub fn set_below_ground_factor(&mut self, factor: f64) {
        self.needs_update = true;
        self.below_ground_factor = factor;
    }

    #[func]
    pub fn terrain_indent_factor(&self) -> f64 {
        self.terrain_indent_factor
    }

    #[func]
    pub fn set_terrain_indent_factor(&mut self, factor: f64) {
        self.needs_update = true;
        self.terrain_indent_factor = factor;
    }

    #[func]
    pub fn terrain_seed_a(&self) -> u32 {
        self.terrain_seed_a
    }

    #[func]
    pub fn set_terrain_seed_a(&mut self, terrain_seed_a: u32) {
        self.needs_update = true;
        self.terrain_seed_a = terrain_seed_a;
    }

    #[func]
    pub fn terrain_seed_b(&self) -> u32 {
        self.terrain_seed_b
    }

    #[func]
    pub fn set_terrain_seed_b(&mut self, terrain_seed_b: u32) {
        self.needs_update = true;
        self.terrain_seed_b = terrain_seed_b;
    }

    #[func]
    pub fn radius(&self) -> u32 {
        self.radius
    }

    #[func]
    pub fn set_radius(&mut self, size: u32) {
        self.needs_update = true;
        self.radius = size;
    }

    #[func]
    pub fn octaves_a(&self) -> u32 {
        self.octaves_a
    }

    #[func]
    pub fn set_octaves_a(&mut self, octaves_a: u32) {
        self.needs_update = true;
        self.octaves_a = octaves_a;
    }

    #[func]
    pub fn frequency_a(&self) -> f64 {
        self.frequency_a
    }

    #[func]
    pub fn set_frequency_a(&mut self, frequency_a: f64) {
        self.needs_update = true;
        self.frequency_a = frequency_a;
    }

    #[func]
    pub fn lacunarity_a(&self) -> f64 {
        self.lacunarity_a
    }

    #[func]
    pub fn set_lacunarity_a(&mut self, lacunarity_a: f64) {
        self.needs_update = true;
        self.lacunarity_a = lacunarity_a;
    }

    #[func]
    pub fn persistence_a(&self) -> f64 {
        self.persistence_a
    }

    #[func]
    pub fn set_persistence_a(&mut self, persistence_a: f64) {
        self.needs_update = true;
        self.persistence_a = persistence_a;
    }

    #[func]
    pub fn octaves_b(&self) -> u32 {
        self.octaves_b
    }

    #[func]
    pub fn set_octaves_b(&mut self, octaves_b: u32) {
        self.needs_update = true;
        self.octaves_b = octaves_b;
    }

    #[func]
    pub fn frequency_b(&self) -> f64 {
        self.frequency_b
    }

    #[func]
    pub fn set_frequency_b(&mut self, frequency_b: f64) {
        self.needs_update = true;
        self.frequency_b = frequency_b;
    }

    #[func]
    pub fn lacunarity_b(&self) -> f64 {
        self.lacunarity_b
    }

    #[func]
    pub fn set_lacunarity_b(&mut self, lacunarity_b: f64) {
        self.needs_update = true;
        self.lacunarity_b = lacunarity_b;
    }

    #[func]
    pub fn persistence_b(&self) -> f64 {
        self.persistence_b
    }

    #[func]
    pub fn set_persistence_b(&mut self, persistence_b: f64) {
        self.needs_update = true;
        self.persistence_b = persistence_b;
    }
}

#[godot_api]
impl GridMapVirtual for Island {
    fn init(base: Base<GridMap>) -> Self {
        godot_print!("Grid Init");

        let mut s = Self {
            base,
            needs_update: true,
            terrain_seed_a: 12345,
            octaves_a: 6,
            frequency_a: 0.04,
            lacunarity_a: 3.0,
            persistence_a: 0.5,
            terrain_seed_b: 54321,
            octaves_b: 6,
            frequency_b: 0.025,
            lacunarity_b: 6.0,
            persistence_b: 0.5,
            radius: 50,
            below_ground_factor: 2.0,
            terrain_indent_factor: 2.0,
        };

        s.clear();
        s.setup();

        s
    }

    fn process(&mut self, _delta: f64) {
        if self.needs_update {
            godot_print!("Update!");
            self.needs_update = false;

            self.clear();
            self.island();
            self.cull();
        }
    }
}
