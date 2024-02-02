using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Image[] itemSlots; // Assign these in the Inspector
    public Image selectionIndicator; // Assign a selection indicator GameObject

    private int selectedItemIndex = -1;

    public TextMeshProUGUI ammoText;


    private void Awake(){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    // When a shot is taken
    public void UpdateAmmoDisplay(int currentAmmo)
    {
        ammoText.text = currentAmmo.ToString();
    }

    // Call this method to update the UI when the inventory changes
    public void UpdateInventoryUI(int selectedIndex)
    {
        UpdateSelectionIndicator(selectedIndex);
    }

    private void UpdateSelectionIndicator(int selectedIndex)
    {
        if (selectedIndex >= 0 && selectedIndex < itemSlots.Length)
        {
            selectionIndicator.transform.position = itemSlots[selectedIndex].transform.position + new Vector3(0f, -50f, 0f);
            selectedItemIndex = selectedIndex;
        }
    }

    
}
