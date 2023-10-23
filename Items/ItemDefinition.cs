using Godot;

[GlobalClass]
public partial class ItemDefinition : Resource
{
    [Export]
    public string Name = "INVALID";

    [Export]
    public uint MaxStackSize = 1;

    [Export]
    public Texture2D Icon = GD.Load<Texture2D>("res:///Items/Defs/_Missing.png");

    [Export]
    public bool CanPlow = false;

    [Export]
    public bool Plantable = false;
}
