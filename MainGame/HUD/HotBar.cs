using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public partial class HotBar : Control
{
	[Export]
	public uint DefaultSlot = 8;

	[Export]
	public uint MaxSlots = 15;

	[Export]
	public Vector2 StandardSize = new Vector2(64, 64);

	[Export]
	public Vector2 SelectedSize = new Vector2(84, 84);

	[Export]
	public Vector2 SpacerSize = new Vector2(32, 32);

	private uint SelectedSlot;

	private bool DoUpdate = true;

	private Label? HotBarTooltip;

	private Panel[]? UISlots;

	[Export]
	private InventoryItem?[]? ItemSlots;

	[Export]
	public PackedScene? SlotTemplate;

	[Signal]
	public delegate void HotBarSelectionChangedEventHandler(InventoryItem? inventoryItem);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Find label
		HotBarTooltip = GetNode<Label>("LabelHotBarContainer/HotBarTooltip");
		HotBarTooltip.Text = "";

		// Select the default slot
		SelectedSlot = DefaultSlot;

		// Add our slots
		var slotContainer = GetNode<HBoxContainer>("ActualHotBarContainer");
		UISlots = new Panel[MaxSlots];
		for (uint i = 0; i < MaxSlots; i++)
		{
			if (i > 0 && i % 3 == 0)
			{
				slotContainer.AddChild(new HSplitContainer
				{
					CustomMinimumSize = SpacerSize,
				});
			}

			var slot = SlotTemplate!.Instantiate<Panel>();
			slot.Name = $"Slot{i}";

			slotContainer.AddChild(slot);
			UISlots[i] = slot;
		}

		ItemSlots = new InventoryItem?[MaxSlots];
	}

	public override void _Process(double delta)
	{
		if (DoUpdate)
		{
			DoUpdate = false;

			for (uint i = 0; i < MaxSlots; i++)
			{
				var item = ItemSlots![i];
				var uiSlot = UISlots![i];

				var icon = uiSlot.GetNode<TextureRect>("Icon");
				if (item == null)
				{
					icon.Texture = null;
				}
				else
				{
					var itemDefinition = ItemDatabase.Instance.FindItem(item);
					icon.Texture = itemDefinition!.Icon;
				}
			}

			// Set all slots back to their standard size
			foreach (var slot in UISlots!)
			{
				slot.CustomMinimumSize = StandardSize;
			}

			// Set the selected slot to the selected size
			UISlots![SelectedSlot - 1].CustomMinimumSize = SelectedSize;

			// If the slot is not empty, display the name
			var selectedItem = ItemSlots![SelectedSlot - 1];
			if (selectedItem != null)
			{
				HotBarTooltip!.Text = $"{selectedItem.Name} ({selectedItem.Amount}x)";

				// Send Signal
				EmitSignal(SignalName.HotBarSelectionChanged, selectedItem);
			}
			else
			{
				HotBarTooltip!.Text = "";

				// Send Signal
				EmitSignal(SignalName.HotBarSelectionChanged, new Variant());
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("hotbar_up"))
		{
			SelectedSlot += 1;

			if (SelectedSlot > MaxSlots)
			{
				SelectedSlot = 1;
			}

			DoUpdate = true;
		}

		if (@event.IsActionPressed("hotbar_down"))
		{
			SelectedSlot -= 1;

			if (SelectedSlot < 1)
			{
				SelectedSlot = MaxSlots;
			}

			DoUpdate = true;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			// Ray cast mouse position into world
			var mouse_position = GetViewport().GetMousePosition();
			var space_state = GetWorld2D().DirectSpaceState;

			var query = PhysicsRayQueryParameters2D.Create(Vector2.Zero, mouse_position);
			query.CollideWithAreas = true;

			// Ray cast and skip if there is no hit
			var result = space_state.IntersectRay(query);
			if (result.Count == 0)
			{
				return;
			}

			var collider = result["collider"];
			try
			{
				var slotArea2D = (Area2D)collider;
				var slot = slotArea2D.GetParent<Panel>();

				var slotName = slot.Name.ToString();
				if (slotName.StartsWith("Slot"))
				{
					var numberString = slotName.Substr(4 /* Slot */, slotName.Length);
					var actualNumber = uint.Parse(numberString);

					GD.Print("Slot Number: " + actualNumber);
					SelectedSlot = actualNumber + 1;
					DoUpdate = true;
				}
			}
			catch (InvalidCastException) { }
		}
	}

	public (uint, InventoryItem)[]? FindItems(string name)
	{
		var list = new LinkedList<(uint, InventoryItem)>();
		for (uint i = 0; i < MaxSlots; i++)
		{
			var item = ItemSlots![i];
			if (item != null && item.Name == name)
			{
				list.AddLast((i, item));
			}
		}

		if (list.Count == 0)
		{
			return null;
		}
		else
		{
			return list.ToArray();
		}
	}

	public uint? FindEmptySlot()
	{
		for (uint i = 0; i < MaxSlots; i++)
		{
			var item = ItemSlots![i];
			if (item == null)
			{
				return i;
			}
		}

		return null;
	}

	public uint AddItem(InventoryItem item)
	{
		var itemDefinition = ItemDatabase.Instance.FindItem(item);
		if (itemDefinition == null)
		{
			GD.PrintErr($"Trying to add invalid Item: {item.Name} ({item})");
			return item.Amount;
		}

		var amountLeft = item.Amount;

		// Check if there are similar items already in the inventory
		var foundItems = FindItems(item.Name);
		if (foundItems != null)
		{
			if (amountLeft <= 0)
			{
				return 0;
			}

			DoUpdate = true;
			foreach (var (index, foundItem) in foundItems)
			{
				var differenceToMax = itemDefinition.MaxStackSize - foundItem.Amount;

				if (amountLeft <= differenceToMax)
				{
					ItemSlots![index]!.Amount += amountLeft;
					amountLeft = 0;
				}
				else
				{
					ItemSlots![index]!.Amount += differenceToMax;
					amountLeft -= differenceToMax;
				}
			}
		}

		// Check if any remain
		if (amountLeft <= 0)
		{
			return 0;
		}

		// Check if there is an empty slot
		var emptySlot = FindEmptySlot();
		while (emptySlot != null && amountLeft > 0)
		{
			DoUpdate = true;
			if (amountLeft <= itemDefinition.MaxStackSize)
			{
				var slotItem = item;
				slotItem.Amount = amountLeft;
				ItemSlots![(int)emptySlot] = slotItem;
				amountLeft = 0;
			}
			else
			{
				ItemSlots![(int)emptySlot] = new InventoryItem
				{
					Name = item.Name,
					Amount = itemDefinition.MaxStackSize,
				};

				amountLeft -= itemDefinition.MaxStackSize;
			}

			emptySlot = FindEmptySlot();
		}

		return amountLeft;
	}

	/// <summary>
	/// Removes an item from the selected slot.
	/// </summary>
	/// <param name="amount">Amount to be removed</param>
	public void RemoveItem(uint amount)
	{
		var item = ItemSlots![SelectedSlot - 1];
		if (item == null)
		{
			return;
		}

		DoUpdate = true;

		var amountLeft = item.Amount - amount;
		if (amountLeft <= 0)
		{
			// Remove the whole item
			RemoveItem();
		}
		else
		{
			// Change the amount
			ItemSlots![SelectedSlot - 1]!.Amount = amountLeft;
		}
	}

	/// <summary>
	/// Removes the item in the current item slot, no matter how many items are present
	/// </summary>
	public void RemoveItem()
	{
		var item = ItemSlots![SelectedSlot - 1];
		if (item == null)
		{
			return;
		}

		ItemSlots![SelectedSlot - 1] = null;
		DoUpdate = true;
	}

	public InventoryItem? GetItem()
	{
		return ItemSlots![SelectedSlot - 1];
	}

	public bool IsEmpty()
	{
		foreach (var item in ItemSlots!)
		{
			if (item != null)
			{
				return false;
			}
		}

		return true;
	}
}
