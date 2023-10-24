using Godot;

[GlobalClass]
public partial class ItemDefinition : Resource
{
    [Export]
    public string Name = "INVALID";

    [Export]
    public uint MaxStackSize = 1;

    [Export]
    public Texture2D Icon = GD.Load<Texture2D>("res:///Items/Definitions/Icons/_Missing.png");

    [Export]
    public ItemType Type = ItemType.Generic;
}
