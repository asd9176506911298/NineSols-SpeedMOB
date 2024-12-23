using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using UnityEngine;

namespace SpeedMOB;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class SpeedMOB : BaseUnityPlugin {
    private ConfigEntry<bool> enableSomethingConfig = null!;

    private Harmony harmony = null!;

    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);

        // Load patches from any class annotated with @HarmonyPatch
        harmony = Harmony.CreateAndPatchAll(typeof(SpeedMOB).Assembly);

        enableSomethingConfig = Config.Bind("General.Something", "Enable", true, "Enable the thing");

        Log.Info($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void OnDestroy() {
        // Make sure to clean up resources here to support hot reloading

        harmony.UnpatchSelf();
    }
}