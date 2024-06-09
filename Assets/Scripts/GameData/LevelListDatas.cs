using NLocalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum LevelDifficulty
{
    easy,
    medium,
    hard,
    impossible
}

[Serializable]
public class OneLevelDatas
{
    public string ID;
    public LocText name;
    public LocText description;
    public LevelScriptableObject level;
    public Sprite preview;
    public LevelDifficulty difficulty;
}

[Serializable]
public class LevelListDatas
{
    public List<OneLevelDatas> levels;
}