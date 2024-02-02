using UnityEngine;
using UnityEngine.InputSystem;

public class PistolBehavior : MonoBehaviour
{
    public UIManager uiManager;

    private Animator animator;
    private InputAction shootAction;

    public ParticleSystem muzzleFlash;

    private InventoryUI inventoryUI;

    private int currentAmmo;
    private float damage;


    public ParticleSystem particleEffect;
    private RaycastHit hit;

    public void Initialize(Item weaponData)
    {
        if (weaponData is Pistol weapon)
        {
            currentAmmo = weapon.ammoCapacity;
            damage = weapon.damage;
        }

        if(inventoryUI != null){
            inventoryUI.UpdateAmmoDisplay(currentAmmo);
        }
    }

    void Awake()
    {  
        uiManager = FindObjectOfType<UIManager>();
        inventoryUI = FindObjectOfType<InventoryUI>();
        animator = GetComponent<Animator>();

        // Define the shoot action
        shootAction = new InputAction(binding: "<Mouse>/leftButton");
        shootAction.performed += ctx => TriggerRecoil();
        shootAction.Enable();
    }

    private void TriggerRecoil()
    {
        if(uiManager.isMenuActive) return;
        if(currentAmmo > 0){
            // Set the trigger to play the recoil animation
            animator.SetTrigger("RecoilTrigger");
            muzzleFlash.Play();
            currentAmmo--;
            if(inventoryUI != null){
                inventoryUI.UpdateAmmoDisplay(currentAmmo);
            }


            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity)){

                 // Instantiate the Particle System at the hit position
                ParticleSystem spawnedParticles = Instantiate(particleEffect, hit.point, Quaternion.identity);

                Quaternion rotationToNormal = Quaternion.FromToRotation(Vector3.forward, hit.normal);

                // Apply the rotation to the instantiated particle system
                spawnedParticles.transform.rotation = rotationToNormal;
                
                // Play the Particle System
                spawnedParticles.Play();

                // Destroy the Particle System after it has finished
                Destroy(spawnedParticles.gameObject, spawnedParticles.main.duration);
            }
            
        }
    }

    void OnEnable()
    {
        if(inventoryUI != null){
            inventoryUI.UpdateAmmoDisplay(currentAmmo);
        }
        shootAction.Enable();
    }

    void OnDisable()
    {
        shootAction.Disable();
    }
}
