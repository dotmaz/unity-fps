using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Transform weaponPosition;

    private Dictionary<string, GameObject> instantiatedWeapons = new Dictionary<string, GameObject>();

    public void EquipWeapon(string weaponName, Item weaponData)
    {
        if (!instantiatedWeapons.ContainsKey(weaponName))
        {
            GameObject weaponPrefab = Instantiate(weaponData.itemPrefab);
            
            weaponPrefab.transform.SetParent(weaponPosition, false);

            weaponPrefab.transform.localRotation = Quaternion.Euler(0, 0, 0);

            instantiatedWeapons[weaponName] = weaponPrefab;


            instantiatedWeapons[weaponName] = weaponPrefab;

            if(weaponName == "Pistol"){
                weaponPrefab.GetComponent<PistolBehavior>().Initialize(weaponData);
            }else if(weaponName == "Big Pistol"){
                weaponPrefab.GetComponent<RifleBehavior>().Initialize(weaponData);
            }
            
        }

        instantiatedWeapons[weaponName].SetActive(true);
    }

    public void UnequipWeapon(string weaponName)
    {
        if (instantiatedWeapons.ContainsKey(weaponName))
        {
            instantiatedWeapons[weaponName].SetActive(false);
        }
    }
}
