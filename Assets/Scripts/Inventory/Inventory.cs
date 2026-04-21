using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    public ItemSO testItem;

    public GameObject hotbarObject;
    public GameObject inventorySlotParent;
    public GameObject container;

    public Image draggedItemIcon;

    public float pickupRange = 3f;
    private Item lookedAtItem = null;
    public Material highlightMaterial;
    private Material originalMaterial;
    private Renderer lookedAtItemRenderer;

    private int activeHotbarIndex = 0; //0-3
    public float activeHotbarOpacity = .9f;
    public float normalHotbarOpacity = .5f;

    [Header("Hand Settings")]
    public Transform hand;
    public Vector3 handOffset = new Vector3(0.3f, -0.3f, 1.0f); // 👈 adjustable distance
    private GameObject equippedHandItem;

    [Header("Item Description UI")]
    public GameObject itemDescriptionParent;
    public Image itemDescriptionIcon;
    public TextMeshProUGUI itemDescriptionNameText;
    public TextMeshProUGUI itemDescriptionText;

    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    private Slot draggedSlot = null;
    private bool isDragging = false;

    private void Awake()
    {
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObject.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);
    }

    private void Start()
    {
        // ✅ Attach hand to camera and apply offset
        if (hand != null && Camera.main != null)
        {
            hand.SetParent(Camera.main.transform);
            hand.localPosition = handOffset;
            hand.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            container.SetActive(!container.activeInHierarchy);
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }

        DetectLookedAtItem();
        PickupItem();

        StartDrag();
        UpdateDraggedItemPosition();
        EndDrag();

        HandleHotbarInput();
        HandleDropItemInWorld();
        UpdateHotbarOpacity();

        UpdateItemDescription();
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

    private void StartDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Slot hoveredSlot = GetHoveredSlot();

            if (hoveredSlot != null && hoveredSlot.HasItem())
            {
                draggedSlot = hoveredSlot;
                isDragging = true;

                draggedItemIcon.sprite = hoveredSlot.GetHeldItem().itemIcon;
                draggedItemIcon.color = new Color(1, 1, 1, 1);
                draggedItemIcon.enabled = true;
            }
        }
    }

    private void EndDrag()
    {
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Slot hoveredSlot = GetHoveredSlot();

            if (hoveredSlot != null)
            {
                HandleDrop(draggedSlot, hoveredSlot);

                draggedItemIcon.enabled = false;

                draggedSlot = null;
                isDragging = false;
            }
        }
    }

    private Slot GetHoveredSlot()
    {
        foreach (Slot slot in allSlots)
        {
            if (slot.hovering)
                return slot;
        }
        return null;
    }

    private void HandleDrop(Slot fromSlot, Slot toSlot)
    {
        if (toSlot == fromSlot)
            return;

        if (toSlot.HasItem() && toSlot.GetHeldItem() == fromSlot.GetHeldItem())
        {
            int max = toSlot.GetHeldItem().maxStackSize;
            int space = max - toSlot.GetHeldItemCount();

            if (space > 0)
            {
                int amountToMove = Mathf.Min(space, fromSlot.GetHeldItemCount());
                toSlot.SetHeldItem(toSlot.GetHeldItem(), toSlot.GetHeldItemCount() + amountToMove);
                fromSlot.SetHeldItem(fromSlot.GetHeldItem(), fromSlot.GetHeldItemCount() - amountToMove);

                if (fromSlot.GetHeldItemCount() <= 0)
                    fromSlot.ClearSlot();

                return;
            }
        }

        if (toSlot.HasItem())
        {
            ItemSO tempItem = toSlot.GetHeldItem();
            int tempCount = toSlot.GetHeldItemCount();

            toSlot.SetHeldItem(fromSlot.GetHeldItem(), fromSlot.GetHeldItemCount());
            fromSlot.SetHeldItem(tempItem, tempCount);
            return;
        }

        toSlot.SetHeldItem(fromSlot.GetHeldItem(), fromSlot.GetHeldItemCount());
        fromSlot.ClearSlot();
    }

    private void UpdateDraggedItemPosition()
    {
        if (isDragging)
            draggedItemIcon.transform.position = Input.mousePosition;
    }

    private void PickupItem()
    {
        if (lookedAtItemRenderer != null && Input.GetKeyDown(KeyCode.E))
        {
            Item item = lookedAtItem.GetComponent<Item>();
            if (item != null)
            {
                AddItem(item.item, item.count);
                Destroy(item.gameObject);
                EquipHandItem();
            }
        }
    }

    private void DetectLookedAtItem()
    {
        if (lookedAtItemRenderer != null)
        {
            lookedAtItemRenderer.material = originalMaterial;
            lookedAtItemRenderer = null;
            originalMaterial = null;
            lookedAtItem = null;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            Item item = hit.collider.GetComponent<Item>();
            if (item != null)
            {
                Renderer renderer = item.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalMaterial = renderer.material;
                    renderer.material = highlightMaterial;
                    lookedAtItemRenderer = renderer;
                    lookedAtItem = item;
                }
            }
        }
    }

    private void UpdateHotbarOpacity()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            Image slotImage = hotbarSlots[i].GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.color = new Color(1, 1, 1, i == activeHotbarIndex ? activeHotbarOpacity : normalHotbarOpacity);
            }
        }
    }

    private void HandleHotbarInput()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                activeHotbarIndex = i;
                UpdateHotbarOpacity();
                EquipHandItem();
            }
        }
    }

    private void HandleDropItemInWorld()
    {
        if (!Input.GetKeyDown(KeyCode.Q))
            return;

        Slot activeSlot = hotbarSlots[activeHotbarIndex];

        if (!activeSlot.HasItem()) return;

        ItemSO itemToDrop = activeSlot.GetHeldItem();
        GameObject prefab = itemToDrop.itemPrefab;

        if (prefab == null) return;

        GameObject droppedItem = Instantiate(
            prefab,
            Camera.main.transform.position + Camera.main.transform.forward,
            Quaternion.identity
        );

        Item item = droppedItem.GetComponent<Item>();
        if (item != null)
        {
            item.item = itemToDrop;
            item.count = activeSlot.GetHeldItemCount();
        }

        activeSlot.ClearSlot();
        EquipHandItem();
    }

    private void EquipHandItem()
{
    if (equippedHandItem != null)
        Destroy(equippedHandItem);

    Slot activeSlot = hotbarSlots[activeHotbarIndex];

    if (!activeSlot.HasItem())
        return;

    ItemSO item = activeSlot.GetHeldItem();

    if (item.handItemPrefab == null)
        return;

    equippedHandItem = Instantiate(item.handItemPrefab, hand);

    // Clean base transform
    Transform t = equippedHandItem.transform;
    t.localPosition = Vector3.zero;
    t.localRotation = Quaternion.identity;
    t.localScale = Vector3.one;

    // Apply offsets
    t.localPosition = item.handLocalPosition;
    t.localRotation = Quaternion.Euler(item.handLocalRotation);
}

    private void UpdateItemDescription()
    {
        Slot hoveredSlot = GetHoveredSlot();

        if (hoveredSlot != null)
        {
            ItemSO hoveredItem = hoveredSlot.GetHeldItem();

            if (hoveredItem != null)
            {
                itemDescriptionParent.SetActive(true);
                itemDescriptionIcon.sprite = hoveredItem.itemIcon;
                itemDescriptionNameText.text = hoveredItem.itemName;
                itemDescriptionText.text = hoveredItem.description;
                return;
            }
        }

        itemDescriptionParent.SetActive(false);
    }

    public ItemSO GetActiveHotbarItem()
    {
        if (hotbarSlots.Count == 0 || activeHotbarIndex >= hotbarSlots.Count)
            return null;

        Slot activeSlot = hotbarSlots[activeHotbarIndex];
        if (activeSlot.HasItem())
            return activeSlot.GetHeldItem();

        return null;
    }
}