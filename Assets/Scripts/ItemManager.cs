using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[System.Serializable]
public struct ItemCountPair
{
    public Item item;
    public int count;

    public ItemCountPair(Item item, int count)
    {
        this.item = item;
        this.count = count;
    }
}

public class ItemManager : MonoBehaviour
{
    public List<ItemCountPair> materials = new List<ItemCountPair>();
    public List<ItemCountPair> machines = new List<ItemCountPair>();
    public List<Item> items = new List<Item>();
    private WeaponManager weaponManager;
    private Item currentlyEquippedItem;

    public Pistol pistol;
    public Pistol bigPistol;

    public Drill drill;
    public TextMeshProUGUI drillCountText;

    public Metal metal;
    public TextMeshProUGUI metalCountText;

    // INPUTS

    public InputActionAsset inputActions; // Assign this in the Inspector
    private InputActionMap actionMap;

    // UI

    private int metalCost = 80;

    private InventoryUI inventoryUI;

    void Awake()
    {
        inventoryUI = FindObjectOfType<InventoryUI>();
        weaponManager = FindObjectOfType<WeaponManager>();

        // Initialize the action map
        actionMap = inputActions.FindActionMap("InventoryControls");

        // Bind actions
        actionMap.FindAction("SelectItem1").performed += ctx => SwitchWeapon(0);
        actionMap.FindAction("SelectItem2").performed += ctx => SwitchWeapon(1);
        // Add more binds for additional items as needed
    }

    private void OnEnable()
    {
        actionMap.Enable();
    }

    private void OnDisable()
    {
        actionMap.Disable();
    }

    ////////

    void Start()
    {
        items.Add(pistol); // Add pistol to inventory
        items.Add(bigPistol); // Add pistol to inventory

        machines.Add(new ItemCountPair(drill, 1)); // Add drill to machine inventory
        drillCountText.text = "1"; // Update drill count text

        materials.Add(new ItemCountPair(metal, 0)); // Add drill to machine inventory
        metalCountText.text = "0"; // Update drill count text

        EquipWeapon(items[0]); // Automatically equip pistol
    }

    public void AddItem(Item newItem)
    {
        items.Add(newItem);
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
    }

    public void AddDrill(){
        if(materials.Count < 1 || materials[0].count < metalCost) return;
        UseMetal(metalCost);
        metalCountText.text = materials[0].count.ToString();
        if(machines.Count > 0){ // If there are machines in your inventory
            var machine = machines[0]; // Drills are first machine
            machine.count = machine.count + 1;
            machines[0] = machine;
            drillCountText.text = machine.count.ToString();
        }
        
        // Update machine count UI
    }

    public void UseDrill(){
        if(machines.Count > 0 && machines[0].count > 0){ // If there are drills in your inventory (drills are first item)
            var machine = machines[0];
            machine.count = machine.count - 1;
            machines[0] = machine;
            drillCountText.text = machine.count.ToString();
        }
    }

    public void AddMetal(){
        if(materials.Count > 0){ // If there are materials in your inventory
            var material = materials[0]; // Metal is first material
            material.count = material.count + 1;
            materials[0] = material;
            metalCountText.text = material.count.ToString();
        }
    }

    public void UseMetal(int amount){
        if(materials.Count > 0 && materials[0].count >= amount){ // If there is enough metal in your inventory (should never run since we check before running)
            var material = materials[0]; // Metal is first material
            material.count = material.count - amount;
            materials[0] = material;
            metalCountText.text = material.count.ToString();
        }
    }

    public void EquipWeapon(Item weaponItem)
    {
        if (weaponItem is Pistol)
        {
            // Unequip the currently equipped weapon
            if (currentlyEquippedItem != null)
            {
                UnequipWeapon(currentlyEquippedItem);
            }

            weaponManager.EquipWeapon(weaponItem.itemName, weaponItem);
            currentlyEquippedItem = weaponItem;
        }
    }

    public void UnequipWeapon(Item weaponItem)
    {
        if (weaponItem is Pistol)
        {
            weaponManager.UnequipWeapon(weaponItem.itemName);
            if (currentlyEquippedItem == weaponItem)
            {
                currentlyEquippedItem = null;
            }
        }
    }

    // Call this method to switch weapons
    public void SwitchWeapon(int itemIndex)
    {
        if (itemIndex >= 0 && itemIndex < items.Count)
        {
            EquipWeapon(items[itemIndex]);
            UpdateInventoryDisplay(itemIndex);
        }
    }

    private void UpdateInventoryDisplay(int itemIndex)
    {
        if (inventoryUI != null)
        {
            inventoryUI.UpdateInventoryUI(itemIndex);
        }
    }
}
