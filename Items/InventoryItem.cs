using Godot;

[GlobalClass]
public partial class InventoryItem : Resource
{
    [Export]
    public string Name = "INVALID";

    [Export]
    public uint Amount = 0;
}
