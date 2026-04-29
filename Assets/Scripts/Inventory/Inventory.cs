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

    // =========================
    // 🔊 ADDED (SAFE)
    // =========================
    [Header("Inventory Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip openInventoryClip;

    public float pickupRange = 3f;
    private Item lookedAtItem = null;
    public Material highlightMaterial;
    private List<Material> originalMaterials = new List<Material>();
    private List<Renderer> lookedAtItemRenderers = new List<Renderer>();

    private int activeHotbarIndex = 0; //0-3
    public float activeHotbarOpacity = .9f;
    public float normalHotbarOpacity = .5f;

    [Header("Hand Settings")]
    public Transform hand;
    public Vector3 handOffset = new Vector3(0.3f, -0.3f, 1.0f); // adjustable distance
    private GameObject equippedHandItem;

    [Header("Item Description UI")]
    public GameObject itemDescriptionParent;
    public Image itemDescriptionIcon;
    public TextMeshProUGUI itemDescriptionNameText;
    public TextMeshProUGUI itemDescriptionText;

    //Crafting
    public List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();
    public Transform craftingGrid;
    public GameObject craftingButtonPrefab;
    public GameObject itemNeededUIPrefab;

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

        PopulateCraftingGrid();
    }

    private void Start()
    {
        // Attach hand to camera
        if (hand != null && Camera.main != null)
        {
            hand.SetParent(Camera.main.transform);
            hand.localPosition = handOffset;
            hand.localRotation = Quaternion.identity;
        }
    }

    void Update()
    {
        // =========================
        // 🔧 ONLY MODIFIED SECTION
        // =========================
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool isOpening = !container.activeInHierarchy;

            container.SetActive(isOpening);

            Cursor.lockState = isOpening ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpening;

            // 🔊 play sound ONLY on open
            if (isOpening && uiAudioSource != null && openInventoryClip != null)
            {
                uiAudioSource.pitch = Random.Range(0.98f, 1.02f);
                uiAudioSource.PlayOneShot(openInventoryClip);
            }

            // 🎧 FIXED LOWPASS CONNECTION
            AudioLowpassController.inventoryOpen = isOpening;
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

    // -----------------------------
    // EVERYTHING BELOW IS UNTOUCHED
    // -----------------------------

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
                    {
                        PopulateCraftingGrid();
                        return;
                    }

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
                {
                    PopulateCraftingGrid();
                    return;
                }
            }
        }

        if (remainingCount > 0)
        {
            Debug.Log("Not enough space to add all items. " + remainingCount + " items could not be added.");
        }
        PopulateCraftingGrid();
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
        if (lookedAtItem != null && Input.GetKeyDown(KeyCode.E))
        {
            Item item = lookedAtItem.GetComponent<Item>();

            if (item == null)
                item = lookedAtItem.GetComponentInParent<Item>();

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
        for (int i = 0; i < lookedAtItemRenderers.Count; i++)
        {
            if (lookedAtItemRenderers[i] != null && i < originalMaterials.Count)
            {
                lookedAtItemRenderers[i].material = originalMaterials[i];
            }
        }

        lookedAtItemRenderers.Clear();
        originalMaterials.Clear();
        lookedAtItem = null;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            Item item = hit.collider.GetComponent<Item>();

            if (item == null)
                item = hit.collider.GetComponentInParent<Item>();

            if (item != null)
            {
                Renderer[] renderers = item.GetComponentsInChildren<Renderer>();

                foreach (Renderer renderer in renderers)
                {
                    originalMaterials.Add(renderer.material);
                    renderer.material = highlightMaterial;
                    lookedAtItemRenderers.Add(renderer);
                }

                lookedAtItem = item;
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
        PopulateCraftingGrid();
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

        Transform t = equippedHandItem.transform;
        t.localPosition = item.handLocalPosition;
        t.localRotation = Quaternion.Euler(item.handLocalRotation);
        t.localScale = item.handLocalScale;
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

    private void PopulateCraftingGrid()
    {
        for (int i = craftingGrid.childCount - 1; i >= 0; i--)
        {
            Destroy(craftingGrid.GetChild(i).gameObject);
        }

        foreach (CraftingRecipe recipe in craftingRecipes)
        {
            GameObject button = Instantiate(craftingButtonPrefab, craftingGrid);

            Image image = button.transform.GetChild(0).GetComponent<Image>();
            image.sprite = recipe.result.itemIcon;

            Button btn = button.GetComponent<Button>();

            btn.interactable = CanCraft(recipe);
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => Craft(recipe));

            foreach (Ingredient ingredient in recipe.ingredients)
            {
                GameObject neededItem = Instantiate(itemNeededUIPrefab, button.transform.GetChild(1));
                neededItem.GetComponent<Image>().sprite = ingredient.item.itemIcon;
                neededItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "x" + ingredient.amount.ToString();
            }
        }
    }

    public void Craft(CraftingRecipe recipe)
    {
        if (!CanCraft(recipe))
        {
            return;
        }

        ConsumeIngredients(recipe);
        AddItem(recipe.result, recipe.resultAmount);

        PopulateCraftingGrid();
    }

    public void ConsumeIngredients(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            int remaining = ingredient.amount;

            foreach (Slot slot in allSlots)
            {
                if (!slot.HasItem()) continue;
                if (!slot.GetHeldItem().Equals(ingredient.item)) continue;

                int take = Mathf.Min(slot.GetHeldItemCount(), remaining);
                slot.SetHeldItem(slot.GetHeldItem(), slot.GetHeldItemCount() - take);

                if (slot.GetHeldItemCount() <= 0)
                {
                    slot.ClearSlot();
                }

                remaining -= take;
                if (remaining <= 0) break;
            }
        }
    }

    public bool CanCraft(CraftingRecipe recipe)
    {
        foreach (Ingredient ingredient in recipe.ingredients)
        {
            int totalFound = 0;

            foreach (Slot slot in allSlots)
            {
                if (slot.HasItem() && slot.GetHeldItem() == ingredient.item)
                {
                    totalFound += slot.GetHeldItemCount();
                }
            }

            if (totalFound < ingredient.amount)
            {
                return false;
            }

        }
        return true;
    }
}
