using Godot;
using System;

public partial class Inventory : GridContainer
{
	private Control[][] inventorySlots;
	private InventoryItem[][] inventoryItems;

	[Signal]
	public delegate void InventoryChangeEventHandler(uint row, uint column, uint itemId, uint amount, InventoryAction action);

	public override void _Ready()
	{
		inventorySlots = new Control[][] {
			new Control[] {
				GetNode<Control>("InventoryRow1/InventorySlot1"),
				GetNode<Control>("InventoryRow1/InventorySlot2"),
				GetNode<Control>("InventoryRow1/InventorySlot3"),
				GetNode<Control>("InventoryRow1/InventorySlot4"),
				GetNode<Control>("InventoryRow1/InventorySlot5"),
				GetNode<Control>("InventoryRow1/InventorySlot6"),
				GetNode<Control>("InventoryRow1/InventorySlot7"),
				GetNode<Control>("InventoryRow1/InventorySlot8"),
				GetNode<Control>("InventoryRow1/InventorySlot9"),
			},
			new Control[] {
				GetNode<Control>("InventoryRow2/InventorySlot1"),
				GetNode<Control>("InventoryRow2/InventorySlot2"),
				GetNode<Control>("InventoryRow2/InventorySlot3"),
				GetNode<Control>("InventoryRow2/InventorySlot4"),
				GetNode<Control>("InventoryRow2/InventorySlot5"),
				GetNode<Control>("InventoryRow2/InventorySlot6"),
				GetNode<Control>("InventoryRow2/InventorySlot7"),
				GetNode<Control>("InventoryRow2/InventorySlot8"),
				GetNode<Control>("InventoryRow2/InventorySlot9"),
			},
			new Control[] {
				GetNode<Control>("InventoryRow3/InventorySlot1"),
				GetNode<Control>("InventoryRow3/InventorySlot2"),
				GetNode<Control>("InventoryRow3/InventorySlot3"),
				GetNode<Control>("InventoryRow3/InventorySlot4"),
				GetNode<Control>("InventoryRow3/InventorySlot5"),
				GetNode<Control>("InventoryRow3/InventorySlot6"),
				GetNode<Control>("InventoryRow3/InventorySlot7"),
				GetNode<Control>("InventoryRow3/InventorySlot8"),
				GetNode<Control>("InventoryRow3/InventorySlot9"),
			},
		};

		inventoryItems = new InventoryItem[][] {
			new InventoryItem[] {
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
			},
			new InventoryItem[] {
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
			},
			new InventoryItem[] {
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
				InventoryItem.Empty(),
			}
		};
	}

	public bool SetItem(uint row, uint column, InventoryItem item, bool overwrite)
	{
		if (row > inventoryItems.Length)
		{
			// If the row index is out of range
			return false;
		}

		if (column > inventoryItems[row].Length)
		{
			// If the column index is out of range
			return false;
		}

		var item_in_slot = inventoryItems[row][column];
		if (item_in_slot.itemID != InventoryItem.EMPTY_ID && !overwrite)
		{
			// If there is an item already and overwrite is not set to be allowed
			return false;
		}

		// Set the item
		inventoryItems[row][column] = item;
		return true;

		// TODO: Update slots (maybe async or in process?)
	}

#nullable enable
	public InventoryItem? GetItem(uint row, uint column)
	{
		if (row > inventoryItems.Length)
		{
			// If the row index is out of range
			return null;
		}

		if (column > inventoryItems[row].Length)
		{
			// If the column index is out of range
			return null;
		}

		return inventoryItems[row][column];
	}

	public bool AddItem(InventoryItem item)
	{
		var empty_spot = FindEmptySpot();
		if (empty_spot == null)
		{
			// If there is no empty spot
			return false;
		}

		return SetItem(empty_spot.Value.Item1, empty_spot.Value.Item2, item, false);
	}

	public (uint, uint)? FindEmptySpot()
	{
		for (uint row = 0; row < inventoryItems.Length; row++)
		{
			for (uint column = 0; column < inventoryItems[row].Length; column++)
			{
				var item = inventoryItems[row][column];
				if (item.itemID == InventoryItem.EMPTY_ID)
				{
					return (row, column);
				}
			}
		}

		return null;
	}
}
