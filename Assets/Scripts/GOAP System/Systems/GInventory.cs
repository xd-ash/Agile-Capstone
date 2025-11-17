using UnityEngine;
using System.Collections.Generic;

// 
public class GoapInventory
{
    List<GameObject> items = new List<GameObject>();

    public void AddItem(GameObject i)
    {
        items.Add(i);
    }
    public GameObject FindItemWithTag(string tag)
    {
        foreach (GameObject i in items)
            if (i.tag == tag)
                return i;
        return null;
    }
    public void RemoveItem(GameObject i)
    {
        //items.Remove(i); Can this work?

        int indexToRemove = -1;
        foreach(GameObject g in items)
        {
            indexToRemove++;
            if (g == i)
                break;
        }
        if (indexToRemove >= -1)
            items.RemoveAt(indexToRemove);
    }
}
