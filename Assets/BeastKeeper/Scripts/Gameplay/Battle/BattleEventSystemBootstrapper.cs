using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace BeastKeeper.Gameplay.Battle
{
    /// <summary>
    /// Ensures a battle-local EventSystem exists so UI interaction works even if the scene has none.
    /// Never creates a duplicate when one already exists, and never persists into the exploration scene.
    /// </summary>
    public static class BattleEventSystemBootstrapper
    {
        public static EventSystem EnsureEventSystem(Transform parent)
        {
            var existing = Object.FindFirstObjectByType<EventSystem>();
            if (existing != null) return existing;

            var go = new GameObject("BattleEventSystem");
            if (parent != null) go.transform.SetParent(parent, false);
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            return go.GetComponent<EventSystem>();
        }
    }
}
