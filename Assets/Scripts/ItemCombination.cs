using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCombination : MonoBehaviour {

    public Interactable item1;
    public Interactable item2;
    public Interactable returnItem;
    public bool reversible;
    public string combineDescription;

    public Interactable getReturnItem()
    {
        return returnItem;
    }

    public string[] getCombineItems()
    {
        string[] items = new string[2];
        items[0] = item1.itemName.ToLower();
        items[1] = item2.itemName.ToLower();

        return items;
    }
}
