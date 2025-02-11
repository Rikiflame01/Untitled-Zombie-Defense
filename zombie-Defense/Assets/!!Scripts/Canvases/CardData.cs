using UnityEngine;

[System.Serializable]
public class CardData
{
    public GameObject prefab;

    public int maxSpawns = 1;

    public const int INFINITE_SPAWNS = -1;

    [HideInInspector] public int currentSpawns = 0;
}