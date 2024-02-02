using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public GameObject menuPanel; // Assign your menu panel in the Inspector
    public bool isMenuActive = false;


    public InputActionAsset inputActions; // Assign this in the Inspector
    private InputActionMap actionMap;


    void Awake()
    {
        // Initialize the action map
        actionMap = inputActions.FindActionMap("MenuControls");

        // Bind actions
        actionMap.FindAction("ToggleBuyMenu").performed += ctx => ToggleBuyMenu();

        actionMap.Enable();
    }


    void ToggleBuyMenu(){
        isMenuActive = !isMenuActive;
        menuPanel.SetActive(isMenuActive);
        ToggleCursorState(isMenuActive);
    }

    void ToggleCursorState(bool isActive)
    {
        if (isActive)
        {
            // Show the cursor and unlock it
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Optionally, disable camera movement or other controls here
        }
        else
        {
            // Hide the cursor and lock it back
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Optionally, enable camera movement or other controls here
        }
    }
}
