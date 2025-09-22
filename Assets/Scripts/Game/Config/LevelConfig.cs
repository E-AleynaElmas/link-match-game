using LinkMatch.Game.Chips;
using UnityEngine;

[CreateAssetMenu(menuName = "LinkMatch/LevelConfig", fileName = "LevelConfig")]
public class LevelConfig : ScriptableObject 
{
    [Min(3)] public int rows = 8;
    [Min(3)] public int cols = 8;
    [Min(1)] public int initialMoves = 20;
    [Min(1)] public int targetScore = 100;
    public bool useFixedSeed = false;
    public int seed = 12345;
    public ChipType[] enabledTypes = new[]{ ChipType.Yellow, ChipType.Blue, ChipType.Green, ChipType.Red };
}