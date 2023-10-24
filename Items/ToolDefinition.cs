using Godot;

[GlobalClass]
public partial class ToolDefinition : Resource
{
    [Export]
    public string Name = "INVALID";

    [Export]
    public bool CanPlow = false;
}
