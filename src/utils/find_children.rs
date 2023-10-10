use godot::prelude::{GodotString, Node, Gd, Array};

pub trait FindChildInScene {
    fn find_child_in_scene(&self, pattern: GodotString, recursive: bool) -> Option<Gd<Node>>;
    fn find_children_in_scene(
        &self,
        pattern: GodotString,
        recursive: bool,
    ) -> Option<Array<Gd<Node>>>;
}

impl FindChildInScene for Node {
    fn find_child_in_scene(&self, pattern: GodotString, recursive: bool) -> Option<Gd<Node>> {
        let tree = self.get_tree()?;
        let current_scene = tree.get_current_scene()?;
        current_scene
            .find_child_ex(pattern)
            .recursive(recursive)
            .done()
    }

    fn find_children_in_scene(
        &self,
        pattern: GodotString,
        recursive: bool,
    ) -> Option<Array<Gd<Node>>> {
        let tree = self.get_tree()?;
        let current_scene = tree.get_current_scene()?;
        let array = current_scene
            .find_children_ex(pattern)
            .recursive(recursive)
            .done();

        if array.is_empty() {
            None
        } else {
            Some(array)
        }
    }
}
