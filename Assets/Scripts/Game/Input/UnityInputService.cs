using System;
using UnityEngine;

namespace LinkMatch.Game.Inputs
{
    public sealed class UnityInputService : MonoBehaviour, IInputService
    {
        public event Action<Vector3> PressedWorld;
        public event Action<Vector3> DraggedWorld;
        public event Action<Vector3> ReleasedWorld;

        [Header("References")]
        [SerializeField] private Camera cam;

        private bool _isDown;
        private int _activeTouchId = -1;

        private void Awake()
        {
            if (cam == null)
                cam = Camera.main;
        }

        private void Update()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouse();
#else
            HandleTouch();
#endif
        }

        // --- Mouse ---
        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _isDown = true;
                PressedWorld?.Invoke(ScreenToWorld(Input.mousePosition));
            }

            if (_isDown && Input.GetMouseButton(0))
            {
                DraggedWorld?.Invoke(ScreenToWorld(Input.mousePosition));
            }

            if (_isDown && Input.GetMouseButtonUp(0))
            {
                _isDown = false;
                ReleasedWorld?.Invoke(ScreenToWorld(Input.mousePosition));
            }
        }

        // --- Touch (tek parmak) ---
        private void HandleTouch()
        {
            if (Input.touchCount == 0) return;

            // aktif parmak yoksa ilk parmağı seç
            if (_activeTouchId == -1)
            {
                var t0 = Input.GetTouch(0);
                _activeTouchId = t0.fingerId;
            }

            // aktif parmağı bul
            Touch? active = null;
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.fingerId == _activeTouchId) { active = t; break; }
            }
            if (active == null) return;

            var touch = active.Value;
            var world = ScreenToWorld(touch.position);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _isDown = true;
                    PressedWorld?.Invoke(world);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (_isDown) DraggedWorld?.Invoke(world);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (_isDown) ReleasedWorld?.Invoke(world);
                    _isDown = false;
                    _activeTouchId = -1;
                    break;
            }
        }

        private Vector3 ScreenToWorld(Vector3 screenPos)
        {
            if (cam == null)
            {
                Debug.LogWarning("Camera reference is null in UnityInputService");
                return screenPos;
            }

            var worldPos = cam.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;
            return worldPos;
        }

#if UNITY_EDITOR
        // Oyun sırasında tıklanan son noktayı küçük bir gizmo ile gösterme opsiyonu
        private Vector3 _lastWorld;
        private void OnEnable()
        {
            PressedWorld  += Save;
            DraggedWorld  += Save;
            ReleasedWorld += Save;
        }
        private void OnDisable()
        {
            PressedWorld  -= Save;
            DraggedWorld  -= Save;
            ReleasedWorld -= Save;
        }
        private void Save(Vector3 w) => _lastWorld = w;
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(_lastWorld, 0.1f);
        }
#endif
    }
}