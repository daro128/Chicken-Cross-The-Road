using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace ChickenRush
{
    /// <summary>
    /// Single place that reads the keyboard. Works with both the old Input Manager
    /// and the new Input System package (Unity 6 default), so the game runs in either setup.
    /// Grid direction convention: x = left/right, y = forward (+) / backward (-) along world Z.
    /// </summary>
    public static class InputReader
    {
        public static Vector2Int ReadMoveDown()
        {
            Vector2Int move = Vector2Int.zero;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Keyboard k = Keyboard.current;
            if (k != null)
            {
                if (k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame) move = Vector2Int.up;
                else if (k.sKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame) move = Vector2Int.down;
                else if (k.aKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame) move = Vector2Int.left;
                else if (k.dKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame) move = Vector2Int.right;
            }
#else
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) move = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) move = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) move = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) move = Vector2Int.right;
#endif
            return move;
        }

        public static bool HopPressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Space);
#endif
        }

        public static bool ConfirmPressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Keyboard k = Keyboard.current;
            return k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
        }

        public static bool RestartPressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Keyboard k = Keyboard.current;
            return k != null && (k.rKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return);
#endif
        }
    }
}
