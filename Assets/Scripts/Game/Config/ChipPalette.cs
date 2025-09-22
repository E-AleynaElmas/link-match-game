
using LinkMatch.Game.Chips;
using UnityEngine;

[CreateAssetMenu(menuName = "LinkMatch/ChipPalette", fileName = "ChipPalette")]
public class ChipPalette : ScriptableObject 
{
    public Sprite yellow; public Sprite blue; public Sprite green; public Sprite red;
    public Color selectionTint = new Color(1f,1f,1f,0.2f);
    public Sprite GetSprite(ChipType t) => t switch
    {
        ChipType.Yellow => yellow,
        ChipType.Blue => blue,
        ChipType.Green => green,
        ChipType.Red => red,
        _ => null
    };
}