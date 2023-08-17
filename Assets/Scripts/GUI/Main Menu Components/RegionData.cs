using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionData : MonoBehaviour
{
    public string regionName = "Unassigned";
    public decimal regionCost = 10000000;
    public int regionIndex = 0;
    public bool unlocked = false;

    public LevelData[] levels;
    public RegionSelect regionSelector;

    public void RegionSelected()
    {
        regionSelector.OnRegionSelected(this);
    }
}

[System.Serializable]
public class LevelData
{
    public string LevelName;
    public string SceneName;
}
