using UnityEngine;
using LinkMatch.Core.Utils;

namespace LinkMatch.Game.Board
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Tile : MonoBehaviour
    {
        public Coord Coord { get; private set; }

        public void Init(Coord c)
        {
            Coord = c;
            name = $"Tile({c.Row},{c.Col})";
        }
    }
}