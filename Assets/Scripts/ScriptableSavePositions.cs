using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SavePoints",fileName = "SavePositionsData")]
public class ScriptableSavePositions : ScriptableObject
{
    public Vector3[] Points;
    public int current_SavePoint_Index;

    public float playerHP;
    public int playerRemainedCartridge;

    public void DataReset()
    {
        playerHP = 100;
        playerRemainedCartridge = 3;
        current_SavePoint_Index = 0;
    }
}
