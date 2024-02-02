using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
   public string itemName = "New Item";
    public GameObject itemPrefab;
}
