using System.Collections.Generic;
using UnityEngine;

namespace LinkMatch.Game.Chips
{
    /// <summary>
    /// Centralized manager for all chip operations to avoid per-chip MonoBehaviour overhead
    /// </summary>
    public class ChipManager : MonoBehaviour
    {
        [SerializeField] private float scaleOnSelect = 1.1f;
        [SerializeField] private Color highlightColor = Color.white;

        private readonly Dictionary<int, ChipData> _chips = new();
        private readonly HashSet<int> _selectedChips = new();

        /// <summary>
        /// Create and register a new chip GameObject
        /// </summary>
        public GameObject CreateChip(ChipType type, Sprite sprite, Vector3 position)
        {
            var chipGO = new GameObject($"Chip_{type}");
            chipGO.transform.position = position;
            chipGO.transform.localScale = Vector3.one; // Ensure correct initial scale

            var spriteRenderer = chipGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.sortingOrder = 1; // Ensure chips render above tiles

            var chipId = chipGO.GetInstanceID();
            var chipData = new ChipData(type, chipGO.transform, spriteRenderer);
            _chips[chipId] = chipData;

            return chipGO;
        }

        /// <summary>
        /// Register an existing GameObject as a chip
        /// </summary>
        public void RegisterExistingChip(GameObject chipGO, ChipType type)
        {
            var spriteRenderer = chipGO.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = chipGO.AddComponent<SpriteRenderer>();

            spriteRenderer.sortingOrder = 1; // Ensure chips render above tiles

            var chipId = chipGO.GetInstanceID();
            var chipData = new ChipData(type, chipGO.transform, spriteRenderer);
            _chips[chipId] = chipData;
            ResetChipVisuals(chipId);
        }

        /// <summary>
        /// Destroy and unregister a chip
        /// </summary>
        public void DestroyChip(GameObject chipGO)
        {
            if (chipGO == null) return;

            var chipId = chipGO.GetInstanceID();
            _selectedChips.Remove(chipId);
            _chips.Remove(chipId);

            Destroy(chipGO);
        }

        /// <summary>
        /// Unregister a chip from the manager (without destroying)
        /// </summary>
        public void UnregisterChip(int chipId)
        {
            _selectedChips.Remove(chipId);
            _chips.Remove(chipId);
        }

        /// <summary>
        /// Unregister a chip by GameObject
        /// </summary>
        public void UnregisterChip(GameObject chipGO)
        {
            if (chipGO != null)
                UnregisterChip(chipGO.GetInstanceID());
        }

        /// <summary>
        /// Set chip selection state by GameObject
        /// </summary>
        public void SetChipSelected(GameObject chipGO, bool selected)
        {
            if (chipGO != null)
                SetChipSelected(chipGO.GetInstanceID(), selected);
        }

        /// <summary>
        /// Set chip selection state by ID
        /// </summary>
        public void SetChipSelected(int chipId, bool selected)
        {
            if (!_chips.TryGetValue(chipId, out var chipData) || !chipData.IsValid)
                return;

            if (selected)
            {
                _selectedChips.Add(chipId);
                ApplySelectedVisuals(chipData);
            }
            else
            {
                _selectedChips.Remove(chipId);
                ApplyNormalVisuals(chipData);
            }

            // Update the selection state
            chipData.IsSelected = selected;
        }

        /// <summary>
        /// Clear all selections
        /// </summary>
        public void ClearAllSelections()
        {
            foreach (var chipId in _selectedChips)
            {
                if (_chips.TryGetValue(chipId, out var chipData) && chipData.IsValid)
                {
                    ApplyNormalVisuals(chipData);
                    chipData.IsSelected = false;
                }
            }
            _selectedChips.Clear();
        }

        /// <summary>
        /// Update chip type and sprite
        /// </summary>
        public void SetChipType(int chipId, ChipType type, Sprite sprite)
        {
            if (!_chips.TryGetValue(chipId, out var chipData) || !chipData.IsValid)
            {
                Debug.LogWarning($"ChipManager: Cannot find chip with ID {chipId} to set type {type}");
                return;
            }

            Debug.Log($"ChipManager: Setting chip {chipId} to type {type}");
            chipData.SetType(type, sprite);
        }

        /// <summary>
        /// Reset chip to default visuals
        /// </summary>
        public void ResetChipVisuals(int chipId)
        {
            if (!_chips.TryGetValue(chipId, out var chipData) || !chipData.IsValid)
                return;

            ApplyNormalVisuals(chipData);
            chipData.Transform.rotation = Quaternion.identity;
            // Use BaseScale instead of Vector3.one to maintain original scale

            chipData.IsSelected = false;
        }

        /// <summary>
        /// Get chip data by ID
        /// </summary>
        public ChipData GetChipData(int chipId)
        {
            return _chips.TryGetValue(chipId, out var chipData) ? chipData : null;
        }

        /// <summary>
        /// Check if chip is selected
        /// </summary>
        public bool IsChipSelected(int chipId)
        {
            return _selectedChips.Contains(chipId);
        }

        private void ApplySelectedVisuals(ChipData chipData)
        {
            chipData.Transform.localScale = chipData.BaseScale * scaleOnSelect;
            chipData.SpriteRenderer.color = highlightColor;
        }

        private void ApplyNormalVisuals(ChipData chipData)
        {
            chipData.Transform.localScale = chipData.BaseScale;
            chipData.SpriteRenderer.color = chipData.BaseColor;
        }

        private void OnDestroy()
        {
            _chips.Clear();
            _selectedChips.Clear();
        }
    }
}