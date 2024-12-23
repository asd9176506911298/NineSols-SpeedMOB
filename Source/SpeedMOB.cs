using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedMOB;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class SpeedMOB : BaseUnityPlugin {
    public static SpeedMOB Instance { get; private set; }
    public ConfigEntry<bool> isSitAtSavePoint = null!;
    public ConfigEntry<bool> isNoRecoveryHealth = null!;
    public ConfigEntry<bool> gotoChallengeHub = null!;

    private Harmony harmony = null!;

    // Teleport data dictionary to map scene names to teleport destinations.
    private static readonly Dictionary<string, (string sceneName, Vector3 position)> teleportPoints = new Dictionary<string, (string, Vector3)> {
        { "A2_S5_BossHorseman_Final", ("A3_S5_BossGouMang_Final", new Vector3(-4430, -2288f, 0f)) },
        { "A3_S5_BossGouMang_Final", ("A4_S5_DaoTrapHouse_Final", new Vector3(1833f, -3744f, 0f)) },
        { "A4_S5_DaoTrapHouse_Final", ("A5_S5_JieChuanHall", new Vector3(-4784, -2288f, 0f)) },
        { "A5_S5_JieChuanHall", ("A7_S5_Boss_ButterFly", new Vector3(-2640f, -1104f, 0f)) },
        { "A7_S5_Boss_ButterFly", ("A9_S5_風氏", new Vector3(-2370f, -1264f, 0f)) },
        { "A9_S5_風氏", ("A10_S5_Boss_Jee", new Vector3(-48f, -64f, 0f)) },
        { "A10_S5_Boss_Jee", ("A11_S0_Boss_YiGung", new Vector3(-2686f, -1104f, 0f)) },
        { "A11_S0_Boss_YiGung", ("VR_Challenge_Hub", new Vector3(-4280f, -2192f, 0f)) }
    };

    private void Awake() {
        Instance = this;
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);

        // Load patches from any class annotated with @HarmonyPatch
        harmony = Harmony.CreateAndPatchAll(typeof(SpeedMOB).Assembly);

        // Config bindings
        isSitAtSavePoint = Config.Bind("", "NotAtSavePointUseJade", true);
        isNoRecoveryHealth = Config.Bind("", "NoRecoveryHealth", false);
        gotoChallengeHub = Config.Bind("", "gotoChallengeHub", false);

        Log.Info($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        // Subscribe to setting change
        gotoChallengeHub.SettingChanged += (_, _) => {
            if (gotoChallengeHub.Value) {
                GotoChallengeHub();
                gotoChallengeHub.Value = false;
            }
        };

        // Scene load event subscription
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Start a coroutine to add a delay
        StartCoroutine(HandleSceneLoadedWithDelay());
    }

    private void GotoChallengeHub() {
        var data = CreateTeleportPointData("VR_Challenge_Hub", new Vector3(-4280f, -2192f, 0));
        GameCore.Instance?.TeleportToSavePoint(data);  // Ensure GameCore.Instance is not null
    }

    private IEnumerator HandleSceneLoadedWithDelay() {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Find all TeleportToSavePointAction objects and modify them
        foreach (var x in GameObject.FindObjectsOfType<TeleportToSavePointAction>()) {
            if (x == null || x.transform == null || x.transform.parent == null) continue; // Null check

            var delayActionModifier = x.transform.parent.GetComponent<DelayActionModifier>();
            if (delayActionModifier == null) continue; // Null check

            delayActionModifier.delayTime = 0f;

            // Check if the current scene has teleport data
            if (teleportPoints.TryGetValue(SceneManager.GetActiveScene().name, out var teleportData)) {
                x.teleportPointData.sceneName = teleportData.sceneName;
                x.teleportPointData.TeleportPosition = teleportData.position;
            }
        }
    }

    private TeleportPointData CreateTeleportPointData(string sceneName, Vector3 position) {
        TeleportPointData teleportPointData = ScriptableObject.CreateInstance<TeleportPointData>();
        teleportPointData.sceneName = sceneName;
        teleportPointData.TeleportPosition = position;
        return teleportPointData;
    }

    private void OnDestroy() {
        // Make sure to clean up resources here to support hot reloading
        SceneManager.sceneLoaded -= OnSceneLoaded;
        harmony.UnpatchSelf();
    }
}
