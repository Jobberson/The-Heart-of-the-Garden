using UnityEngine;
using System.Collections.Generic;

public class ItemsPuzzleChapter2 : MonoBehaviour
{
    public List<ItemsToPlace> AllItemsPuzzle = new();
}

[System.Serializable]
public class ItemsToPlace
{
    public string name;
    public GameObject obj;
}
