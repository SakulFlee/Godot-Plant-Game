using Godot;

[GlobalClass]
public partial class PlantDefinition : Resource
{
    [Export]
    public string Name = "INVALID";

    [Export]
    public bool CanBePlanted = false;
}
