using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ResourceNodeTriggerZone : MonoBehaviour
{
    public TextMeshProUGUI resouceNodeMessage;

    public InputActionAsset inputActions; // Assign this in the Inspector
    private InputActionMap actionMap;

    public GameObject resourceDrill;

    private bool canPlaceDrill = false;
    private bool nodeOccupied = false;


    public ItemManager itemManager;
    private int count = 0;
    void Update(){
        if(nodeOccupied && count%100 == 0){
            itemManager.AddMetal();
        }
        count++;
        if(count >= 1000000){
            count = count % 1000000;
        }
    }

    void Awake()
    {
        // Initialize the action map
        actionMap = inputActions.FindActionMap("BuildControls");

        // Bind actions
        actionMap.FindAction("BuildResourceDrill").performed += ctx => BuildResourceDrill();

        actionMap.Enable();
    }

    private void BuildResourceDrill(){
        if(!canPlaceDrill || nodeOccupied) return; // Exit if node is taken
        if(itemManager.machines.Count < 1 || itemManager.machines[0].count < 1) return; // Exit if user doesn't have enough drills
        itemManager.UseDrill();
        resourceDrill.SetActive(true);
        canPlaceDrill = false;
        nodeOccupied = true;
        resouceNodeMessage.text = "";
    }

    private void OnTriggerEnter(Collider other)
    {
        if(nodeOccupied) return;
        if (other.CompareTag("Player"))
        {
            canPlaceDrill = true;
            resouceNodeMessage.text = "press E to place a drill";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(nodeOccupied) return;
        if (other.CompareTag("Player"))
        {
            canPlaceDrill = false;
            resouceNodeMessage.text = "";
        }
    }
}