using UnityEngine;
using UnityEngine.EventSystems;

namespace BeeHarvest.Tests
{
    /// <summary>Feeds held-touch counts to the installed uGUI module in focused regression tests.</summary>
    public sealed class SettingsTouchInput : BaseInput
    {
        public int Count;
        public Touch Value;
        public override int touchCount => Count;
        public override Touch GetTouch(int index) { return Value; }
        public override bool GetMouseButton(int button) { return false; }
    }
}
