using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ItemDatabase : Node
{
	private static readonly string ITEM_PATH = "res://Items/Defs";

	public static readonly ItemDatabase Instance = new ItemDatabase();

	private LinkedList<ItemDefinition> Items = new LinkedList<ItemDefinition>();

	// Called when the node enters the scene tree for the first time.
	private ItemDatabase()
	{
		var dir = DirAccess.Open(ITEM_PATH);
		if (dir == null)
		{
			GD.PushError("Failed accessing Item Definitions");
			return;
		}

		foreach (var file in dir.GetFiles())
		{
			if (!file.EndsWith(".tres"))
			{
				// Skip any non-definitions (such as icons)
				continue;
			}

			var filePath = $"{ITEM_PATH}/{file}";

			var itemDefItemDefinition = GD.Load<ItemDefinition>(filePath);
			Items.AddLast(itemDefItemDefinition);
		}
	}

	public ItemDefinition? FindItem(string name)
	{
		var query = (from Item in Items
					 where Item.Name.ToLower() == name.ToLower()
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
}
