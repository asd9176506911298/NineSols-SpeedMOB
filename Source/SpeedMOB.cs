using BepInEx;
using BepInEx.Configuration;
using Com.LuisPedroFonseca.ProCamera2D;
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

        KeybindManager.Add(this, test, () => new KeyboardShortcut(KeyCode.E, KeyCode.LeftControl));

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

    public GameObject b = null!;
    public GameObject c = null!;

    private IEnumerator PreloadSceneObjects(string sceneName, string objectPath, Vector3 pos) {
        // Load the scene asynchronously in the background.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Wait until the scene is fully loaded.
        while (!asyncLoad.isDone) {
            yield return null;
        }

        // Scene is now loaded, find the target object.
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        GameObject targetObject = null;

        if (loadedScene.IsValid()) {
            // Temporarily activate the loaded scene.
            SceneManager.SetActiveScene(loadedScene);

            // Find the target GameObject.
            targetObject = GameObject.Find(objectPath);

            if (targetObject != null) {
                // Detach the object to make it a root GameObject.
                //targetObject.SetActive(true);
                //targetObject.transform.SetParent(null);
                if (sceneName == "A7_S5_Boss_ButterFly")
                    b = targetObject;
                else if (sceneName == "A7_S5_Boss_ButterFly")
                    c = targetObject;
                // Ensure it's now a root object before making it persistent.
                if (targetObject.transform.parent == null) {
                    //horse
                    //Vector3 v = new Vector3(targetObject.transform.position.x, targetObject.transform.position.y + 100f, targetObject.transform.position.z);
                    //Vector3 v = new Vector3(targetObject.transform.position.x, targetObject.transform.position.y - 500f, targetObject.transform.position.z);
                    //Vector3 v = new Vector3(targetObject.transform.position.x + 100f, targetObject.transform.position.y + 95f, targetObject.transform.position.z);
                    targetObject.transform.position = pos;
                    //ToastManager.Toast(GetGameObjectPath(targetObject.gameObject));
                    RCGLifeCycle.DontDestroyForever(targetObject);
                    //var levelAwakeList = targetObject.GetComponentsInChildren<ILevelAwake>(true);
                    //for (var i = levelAwakeList.Length - 1; i >= 0; i--) {
                    //    var context = levelAwakeList[i];
                    //    try { context.EnterLevelAwake(); } catch (Exception ex) { Log.Error(ex.StackTrace); }
                    //}
                    Log.Info($"Found and persisted GameObject: {targetObject.name}");
                } else {
                    Log.Warning($"Failed to detach GameObject: {targetObject.name}");
                }
            } else {
                Log.Warning($"GameObject with path '{objectPath}' not found in scene '{sceneName}'.");
            }
        } else {
            Log.Warning($"Scene '{sceneName}' is not valid or failed to load.");
        }

        // Unload the scene.
        SceneManager.UnloadSceneAsync(sceneName);

        Log.Info("Scene unloaded and active scene reverted.");

        // Reload the current scene to reset the state if needed.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void test() {
        //ToastManager.Toast(GameObject.Find("GameLevel_VR_Challenge/CameraCore").GetComponent<ProCamera2DNumericBoundaries>());
        //GameObject.Find("GameLevel_VR_Challenge/CameraCore").GetComponent<ProCamera2DNumericBoundaries>().enabled = false;
        StartCoroutine(PreloadSceneObjectsChain());
    }
    IEnumerator PreloadSceneObjectsChain() {
        // Wait for the first coroutine to finish
        //yield return StartCoroutine(PreloadSceneObjects("A2_S5_BossHorseman_Final", "A2_S5_ BossHorseman_GameLevel", new Vector3(100f, -500f, 0f)));

        // Wait for 2 seconds before starting the next coroutine
        //yield return new WaitForSeconds(2f);

        //// Run the second coroutine after the first one completes
        yield return StartCoroutine(PreloadSceneObjects("A3_S5_BossGouMang_Final", "A3_S5_BossGouMang_GameLevel", new Vector3(100f, -1000f, 0f)));
        yield return StartCoroutine(PreloadSceneObjects("A5_S5_JieChuanHall", "A5_S5", new Vector3(100f, -1500f, 0f)));
        //yield return StartCoroutine(PreloadSceneObjects("A7_S5_Boss_ButterFly", "P2_R22_Savepoint_GameLevel", new Vector3(0, 0, 0f)));
        yield return StartCoroutine(PreloadSceneObjects("A9_S5_風氏", "P2_R22_Savepoint_GameLevel", new Vector3(100f, -3000f, 0f)));
        yield return StartCoroutine(PreloadSceneObjects("A10_S5_Boss_Jee", "A10S5", new Vector3(100f, -3500f, 0f)));

        //yield return new WaitForSeconds(2f);

        ToastManager.Toast(GameObject.Find("A2_S5_ BossHorseman_GameLevel/CameraCore"));
        GameObject.Find("A2_S5_ BossHorseman_GameLevel/CameraCore").GetComponent<ProCamera2DNumericBoundaries>().enabled = false;
        GameObject.Find("A3_S5_BossGouMang_GameLevel (RCGLifeCycle)").transform.Find("CameraCore").gameObject.SetActive(false);
        GameObject.Find("A5_S5 (RCGLifeCycle)").transform.Find("CameraCore").gameObject.SetActive(false);
        //b.transform.Find("CameraCore").gameObject.SetActive(false);
        //b.SetActive(false);
        c.transform.Find("CameraCore").gameObject.SetActive(false);
        GameObject.Find("A10S5 (RCGLifeCycle)").transform.Find("CameraCore").gameObject.SetActive(false);
        GameObject.Find("GameLevel (RCGLifeCycle)").transform.Find("CameraCore").gameObject.SetActive(false);
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

    public TeleportPointData CreateTeleportPointData(string sceneName, Vector3 position) {
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
