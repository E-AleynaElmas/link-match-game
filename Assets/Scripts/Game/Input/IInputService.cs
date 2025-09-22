using System;
using UnityEngine;

namespace LinkMatch.Game.Inputs
{
    public interface IInputService
    {
        event Action<Vector3> PressedWorld;
        event Action<Vector3> DraggedWorld;
        event Action<Vector3> ReleasedWorld;
    }
}