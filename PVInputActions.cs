using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using PiggyVarietyMod.Patches;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using LethalLib.Modules;
using BepInEx.Bootstrap;
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
