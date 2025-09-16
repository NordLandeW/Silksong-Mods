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
    public class PluginConfig : ConfigManager
    {
        public PluginConfig(ConfigFile Config) : base(Config)
        {
        }

        protected override void CheckConfigImplements(Step step)
        {
            bool saveFlag = false;

            if (step == Step.AWAKE)
            {
                var modVersion = Bind("Base", "ModVersion", Plugin.ModVersion, "Don't change.");
                modVersion.Value = Plugin.ModVersion;

                var nailAttackSetter = Bind("Settings", "NailAttackSetter", 1, "Nail attack power. If 0, it is not affected.");
                var nailAttackMultiplier = Bind("Settings", "NailAttackMultiplier", 1, "Multiplier for nail attack power. If 0, it is not affected.");

                saveFlag = true;
            }
            if (saveFlag)
            {
                Save(false);
            }
        }
    }

    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "nord.Nail1ATK";
        public const string ModName = "Nail1ATK";
        public const string ModVersion = "1.0.0";

        public static int nailAttackSetter = 1;
        public static int nailAttackMultiplier = 1;
        private Harmony harmony;

        public void Awake()
        {
            LogManager.Logger = base.Logger;

            new PluginConfig(base.Config);
            ConfigManager.CheckConfig(ConfigManager.Step.AWAKE);

            nailAttackSetter = ConfigManager.GetValue<int>("Settings", "NailAttackSetter");
            nailAttackMultiplier = ConfigManager.GetValue<int>("Settings", "NailAttackMultiplier");

            LogManager.LogInfo("Patching...");
            harmony = new Harmony($"{ModGuid}.Patch");
            harmony.PatchAll(typeof(PluginPatches));
            LogManager.LogInfo($"Nail1ATK Patched. Settings: NailAttackSetter={nailAttackSetter}, NailAttackMultiplier={nailAttackMultiplier}");
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
            if (Plugin.nailAttackSetter != 0)
            {
                __result = Plugin.nailAttackSetter;
            }
            if (Plugin.nailAttackMultiplier != 0)
            {
                __result *= Plugin.nailAttackMultiplier;
            }
        }
    }
}
