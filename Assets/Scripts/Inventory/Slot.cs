using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hovering;

    private ItemSO heldItem;
    private int itemCount;

    private Image itemIcon;
    private TextMeshProUGUI itemCountText;

    private void Awake()
    {
        itemIcon = transform.GetChild(0).GetComponent<Image>();
        itemCountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public ItemSO GetHeldItem()
    {
        return heldItem;
    }

    public int GetHeldItemCount()
    {
        return itemCount;
    }

    public void SetHeldItem(ItemSO item, int count = 1)
    {
        heldItem = item;
        itemCount = count;

        UpdateSlot();
    }
    public void UpdateSlot()
    {
        if(itemIcon == null)
        {
            itemIcon = transform.GetChild(0).GetComponent<Image>();
            itemCountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        }
        if (heldItem != null)
        {
            itemIcon.sprite = heldItem.itemIcon;
            itemIcon.enabled = true;
            itemCountText.text = itemCount > 1 ? itemCount.ToString() : "";
        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            itemCountText.text = "";
        }
    }

    public int AddToStack(int amountToAdd)
    {
        itemCount += amountToAdd;
        UpdateSlot();
        return itemCount;
    }

    public int RemoveFromStack(int amountToRemove)
    {
        itemCount -= amountToRemove;
        if (itemCount <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateSlot();
        }
        return itemCount;
    }

    public void ClearSlot()
    {
        heldItem = null;
        itemCount = 0;
        UpdateSlot();
    }

    public bool HasItem()
    {
        return heldItem != null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}