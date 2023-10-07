use godot::{
    engine::{GridMap, GridMapVirtual},
    prelude::*,
};
use noise::{
    utils::{NoiseMap, NoiseMapBuilder, PlaneMapBuilder},
    Billow, Perlin,
};

struct MyExtension;

#[gdextension]
unsafe impl ExtensionLibrary for MyExtension {}

#[derive(GodotClass)]
#[class(tool, base=GridMap)]
struct Island {
    needs_update: bool,
    noise_map: Option<NoiseMap>,

    #[base]
    base: Base<GridMap>,

    #[export]
    #[var(get = seed, set = set_seed)]
    pub seed: u32,

    #[export(range = (5.0, 100.0))]
    #[var(get = size, set = set_size)]
    pub radius: u32,

    #[export]
    #[var(get = height_multiplier, set = set_height_multiplier)]
    pub height_multiplier: f64,
}

#[godot_api]
impl Island {
    fn clear(&mut self) {
        godot_print!("Clear");
        self.base.clear();
    }

    fn setup(&mut self) {
        godot_print!("Setup");
        self.base.set_cell_size(Vector3::new(0.5, 0.5, 0.5));
        self.base.set_cell_scale(0.5);

        let mesh_library = match try_load("res://Mesh Library/Voxels.tres") {
            Some(mesh_library) => mesh_library,
            None => {
                return;
            }
        };
        self.base.set_mesh_library(mesh_library);
    }

    fn island(&mut self) {
        let size = self.radius as i32;
        let range = (self.radius as f32).powf(2.0);

        for x in -size..=size {
            for z in -size..=size {
                let distance =
                    Vector3::new(x as f32, 0.0, z as f32).distance_squared_to(Vector3::ZERO);

                if distance <= range {
                    let n = self
                        .noise_map
                        .as_ref()
                        .unwrap()
                        .get_value((x + size) as usize, (z + size) as usize);

                    if n <= 1.0 {
                        continue;
                    }

                    let n = n * self.height_multiplier;
                    for y in -(n as i32)..=0 {
                        let cell_position = Vector3i::new(x, y, z);

                        let mesh_index: i32;
                        if y == 0 {
                            mesh_index = 0;
                        } else if y < 0 && y >= -3 {
                            mesh_index = 1;
                        } else {
                            mesh_index = 2;
                        }

                        self.base.set_cell_item(cell_position, mesh_index);
                    }
                }
            }
        }
    }

    #[func]
    pub fn height_multiplier(&self) -> f64 {
        self.height_multiplier
    }

    #[func]
    pub fn set_height_multiplier(&mut self, height_multiplier: f64) {
        self.needs_update = true;
        self.height_multiplier = height_multiplier;
    }

    #[func]
    pub fn seed(&self) -> u32 {
        self.seed
    }

    #[func]
    pub fn set_seed(&mut self, seed: u32) {
        self.needs_update = true;
        self.seed = seed;
    }

    #[func]
    pub fn size(&self) -> u32 {
        self.radius
    }

    #[func]
    pub fn set_size(&mut self, size: u32) {
        self.needs_update = true;
        self.radius = size;
    }
}

#[godot_api]
impl GridMapVirtual for Island {
    fn init(base: Base<GridMap>) -> Self {
        godot_print!("Grid Init");

        let mut s = Self {
            base,
            needs_update: true,
            seed: 12345,
            radius: 25,
            height_multiplier: 5.0,
            noise_map: None,
        };

        let billow = Billow::<Billow<Perlin>>::new(s.seed);
        let noise_map = PlaneMapBuilder::<_, 2>::new(billow)
            .set_size((s.radius * 2) as usize, (s.radius * 2) as usize)
            .set_x_bounds(-1.0, 1.0)
            .set_y_bounds(-1.0, 1.0)
            .build();

        s.noise_map = Some(noise_map);

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
        }
    }
}
