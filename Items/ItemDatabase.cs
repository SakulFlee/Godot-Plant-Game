using Godot;
using System.Collections.Generic;
using System.Linq;

class Populator<T> where T : Resource
{

}

public partial class ItemDatabase : Node
{
	private static readonly string ITEM_DEFINITIONS = "res://Items/Definitions/Items";
	private static readonly string TOOL_DEFINITIONS = "res://Items/Definitions/Tools";
	private static readonly string PLANT_DEFINITIONS = "res://Items/Definitions/Plants";

	public static readonly ItemDatabase Instance = new ItemDatabase();

	private LinkedList<ItemDefinition> Items = new LinkedList<ItemDefinition>();
	private LinkedList<ToolDefinition> Tools = new LinkedList<ToolDefinition>();
	private LinkedList<PlantDefinition> Plants = new LinkedList<PlantDefinition>();

	// Called when the node enters the scene tree for the first time.
	private ItemDatabase()
	{
		Items = PopulateDatabase<ItemDefinition>(ITEM_DEFINITIONS);
		Tools = PopulateDatabase<ToolDefinition>(TOOL_DEFINITIONS);
		Plants = PopulateDatabase<PlantDefinition>(PLANT_DEFINITIONS);
	}

	private static LinkedList<T> PopulateDatabase<T>(string folder)
	where T : Resource
	{
		var list = new LinkedList<T>();

		var dir = DirAccess.Open(folder);
		if (dir == null)
		{
			GD.PushError("Failed accessing Item Definitions");
			return list;
		}

		foreach (var file in dir.GetFiles())
		{
			if (!file.EndsWith(".tres"))
			{
				// Skip any non-definitions (such as icons)
				continue;
			}

			var filePath = $"{folder}/{file}";

			var itemDefItemDefinition = GD.Load<T>(filePath);
			list.AddLast(itemDefItemDefinition);
		}

		return list;
	}

	public ItemDefinition? FindItem(string name)
	{
		var query = (from Item in Items
					 where Item.Name == name
					 select Item).ToArray();

		if (query.Length >= 1)
		{
			if (query.Length > 1)
			{
				GD.PushWarning($"Multiple results for Item with name '{name}'!");
			}

			return query[0];
		}
		return null;
	}

	public ItemDefinition? FindItem(InventoryItem item)
	{
		return FindItem(item.Name);
	}

	public ToolDefinition? FindTool(string name)
	{
		var query = (from Tool in Tools
					 where Tool.Name == name
					 select Tool).ToArray();

		if (query.Length >= 1)
		{
			if (query.Length > 1)
			{
				GD.PushWarning($"Multiple results for Item with name '{name}'!");
			}

			return query[0];
		}
		return null;
	}
	public PlantDefinition? FindPlant(string name)
	{
		var query = (from Plant in Plants
					 where Plant.Name == name
					 select Plant).ToArray();

		if (query.Length >= 1)
		{
			if (query.Length > 1)
			{
				GD.PushWarning($"Multiple results for Item with name '{name}'!");
			}

			return query[0];
		}
		return null;
	}
}
