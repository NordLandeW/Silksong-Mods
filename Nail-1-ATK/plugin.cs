using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using System.Reflection;
using BepInEx.Configuration;

namespace nord.Nail1ATK
{
    public class PluginConfig
    {
        public static ConfigEntry<int> NailAttackSetter { get; private set; }
        public static ConfigEntry<double> NailAttackMultiplier { get; private set; }

        public PluginConfig(ConfigFile config)
        {
            var modVersion = config.Bind("Base", "ModVersion", Plugin.ModVersion, "Don't change.");
            
            bool needsSave = modVersion.Value != Plugin.ModVersion;
            modVersion.Value = Plugin.ModVersion;

            NailAttackSetter = config.Bind("Settings", "NailAttackSetter", 1, "Nail attack power. If 0, it is not affected.");
            NailAttackMultiplier = config.Bind("Settings", "NailAttackMultiplier", 1.0, "Multiplier for nail attack power. If 0, it is not affected.");

            if (needsSave)
            {
                config.Save();
                LogManager.LogInfo("Config file updated and saved.");
            }
        }
    }

    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "nord.Nail1ATK";
        public const string ModName = "Nail1ATK";
        public const string ModVersion = "1.0.0";

        public static PluginConfig settings;
        private Harmony harmony;

        public void Awake()
        {
            LogManager.Logger = base.Logger;

            settings = new PluginConfig(base.Config);

            LogManager.LogInfo("Patching...");
            harmony = new Harmony($"{ModGuid}.Patch");
            harmony.PatchAll(typeof(PluginPatches));
            LogManager.LogInfo($"Nail1ATK Patched. Settings: NailAttackSetter={PluginConfig.NailAttackSetter.Value}, NailAttackMultiplier={PluginConfig.NailAttackMultiplier.Value}");
        }

        public void OnDestroy()
        {
            harmony.UnpatchSelf();
        }
    }

    public class PluginPatches
    {
        [HarmonyPatch(typeof(PlayerData), "nailDamage", MethodType.Getter)]
        public static void Postfix(ref int __result)
        {
            int setterValue = PluginConfig.NailAttackSetter.Value;
            double multiplierValue = PluginConfig.NailAttackMultiplier.Value;

            if (setterValue != 0)
            {
                __result = setterValue;
            }
            if (multiplierValue != 0)
            {
                __result = (int)Math.Round(__result * multiplierValue);
            }
        }
    }
}
