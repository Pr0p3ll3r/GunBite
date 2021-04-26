using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLibrary : MonoBehaviour
{
    public Weapon[] allWeapons;
    public static Weapon[] weapons;

    private void Awake()
    {
        weapons = allWeapons;
        foreach (Weapon weapon in weapons)
        {
            weapon.Initialize();
        }
    }

    public static Weapon FindGun(string name)
    {
        foreach (Weapon weapon in weapons)
        {
            if (weapon.name.Equals(name)) return weapon;
        }

        return weapons[0];
    }

    public static int GetIndex(string name)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].name == name)
                return i;
        }

        return -1;
    }
}