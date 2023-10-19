using Godot;
using System;

public partial class InventoryItem : GridContainer
{
	public static uint EMPTY_ID = 0;

	public uint itemID { get; set; }
	public uint amount { get; set; }

	public static InventoryItem Empty()
	{
		return new InventoryItem(EMPTY_ID, 0);
	}

	public InventoryItem(uint itemID, uint amount)
	{
		this.itemID = itemID;
		this.amount = amount;
	}
}
