using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Transform cameraTransform;

    private Dictionary<string, GameObject> instantiatedWeapons = new Dictionary<string, GameObject>();

    public void EquipWeapon(string weaponName, Item weaponData)
    {
        if (!instantiatedWeapons.ContainsKey(weaponName))
        {
            GameObject weaponPrefab = Instantiate(weaponData.itemPrefab);
            
            weaponPrefab.transform.SetParent(cameraTransform, false);

            weaponPrefab.transform.localPosition = new Vector3(0.225f, -0.297f, 0.648f);
            weaponPrefab.transform.localRotation = Quaternion.Euler(0, 0, 0);

            instantiatedWeapons[weaponName] = weaponPrefab;


            instantiatedWeapons[weaponName] = weaponPrefab;

            weaponPrefab.GetComponent<PistolBehavior>().Initialize(weaponData);
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
