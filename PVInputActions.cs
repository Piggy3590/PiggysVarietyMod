using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace PiggyVarietyMod
{
    public class PVInputActions : LcInputActions
    {
        [InputAction(KeyboardControl.R, Name = "Reload Rifle")]
        public InputAction RifleReloadKey { get; set; }
    }
}
