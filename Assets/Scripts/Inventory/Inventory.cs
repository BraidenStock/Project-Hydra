using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Inventory : MonoBehaviour
{
    public ItemSO testItem;

    public GameObject hotbarObject;
    public GameObject inventorySlotParent;

    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    private void Awake()
    {
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObject.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            AddItem(testItem, 10);
        }
    }

    public void AddItem(ItemSO itemToAdd, int count)
    {
        int remainingCount = count;

        foreach (Slot slot in allSlots)
        {
            if (slot.HasItem() && slot.GetHeldItem() == itemToAdd)
            {
                int currentAmount = slot.GetHeldItemCount();
                int maxStackSize = itemToAdd.maxStackSize;

                if (currentAmount < maxStackSize)
                {
                    int spaceLeft = maxStackSize - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remainingCount);
                    slot.SetHeldItem(itemToAdd, currentAmount + amountToAdd);

                    remainingCount -= amountToAdd;

                    if (remainingCount <= 0)
                        return;
                }
            }
        }

        foreach (Slot slot in allSlots)
        {
            if (!slot.HasItem())
            {
                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remainingCount);
                slot.SetHeldItem(itemToAdd, amountToPlace);

                remainingCount -= amountToPlace;

                if (remainingCount <= 0)
                    return;
            }
        }

        if (remainingCount > 0)
        {
            Debug.Log("Not enough space to add all items. " + remainingCount + " items could not be added.");
        }
    }
}