﻿using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Building/Building", order = 0)]
public class BuildingScriptableObject : ScriptableObject
{
    public Building buildingPrefab;
    public Vector2 buildingSize;
    public Sprite sprite;
    public int popularity;
}