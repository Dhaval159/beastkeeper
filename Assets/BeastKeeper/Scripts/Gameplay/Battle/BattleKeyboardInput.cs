using UnityEngine;
using UnityEngine.InputSystem;

namespace BeastKeeper.Gameplay.Battle
{
    /// <summary>
    /// Maps keyboard input to battle actions. Used as a fallback when UI buttons are missing or unavailable.
    /// </summary>
    public static class BattleKeyboardInput
    {
        public static BattleAction KeyToAction(Key key)
        {
            switch (key)
            {
                case Key.Digit1: return BattleAction.Attack;
                case Key.Digit2: return BattleAction.Observe;
                case Key.Digit3: return BattleAction.Item;
                case Key.Digit4: return BattleAction.Run;
                default: return BattleAction.None;
            }
        }

        public static BattleAction ReadAction(Keyboard keyboard)
        {
            if (keyboard == null) return BattleAction.None;
            if (keyboard.digit1Key.wasPressedThisFrame) return BattleAction.Attack;
            if (keyboard.digit2Key.wasPressedThisFrame) return BattleAction.Observe;
            if (keyboard.digit3Key.wasPressedThisFrame) return BattleAction.Item;
            if (keyboard.digit4Key.wasPressedThisFrame) return BattleAction.Run;
            return BattleAction.None;
        }

        /// <summary>
        /// True when the player requests leaving the end-of-battle screen (Enter/Space/Escape).
        /// </summary>
        public static bool IsEndSequenceAdvancePressed(Keyboard keyboard)
        {
            if (keyboard == null) return false;
            return keyboard.enterKey.wasPressedThisFrame ||
                   keyboard.numpadEnterKey.wasPressedThisFrame ||
                   keyboard.spaceKey.wasPressedThisFrame ||
                   keyboard.escapeKey.wasPressedThisFrame;
        }
    }
}
