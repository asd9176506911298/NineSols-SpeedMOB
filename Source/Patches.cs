using Com.LuisPedroFonseca.ProCamera2D;
using HarmonyLib;
using NineSolsAPI;
using UnityEngine;

namespace SpeedMOB;

[HarmonyPatch]  // This attribute ensures Harmony will patch methods from this class
public class Patches {

    // Prefix patch for the "get_isValid" method of PlayerSitAtSavePointCondition.
    [HarmonyPrefix, HarmonyPatch(typeof(PlayerSitAtSavePointCondition), "get_isValid")]
    public static bool PatchIsSitAtSavePoint(ref PlayerSitAtSavePointCondition __instance, ref bool __result) {
        // Check if the configuration to block sitting at save points is enabled.
        if (SpeedMOB.Instance.isSitAtSavePoint.Value) {
            // If the instance name contains "[Condition]", set isValid to true and prevent the original method from running.
            if (__instance.name.Contains("[Condition]")) {
                __result = true;
                return false;  // Return false to prevent the original method from running.
            }
        }
        return true;  // Allow the original method to run if conditions are not met.
    }

    // Prefix patch for the "RestoreEverything" method of the Player class.
    [HarmonyPrefix, HarmonyPatch(typeof(Player), "RestoreEverything")]
    public static bool PatchRestoreEverything(ref Player __instance) {
        // Check if the no recovery health configuration is enabled.
        if (SpeedMOB.Instance.isNoRecoveryHealth.Value) {
            // Prevent health recovery if the player's health is already greater than zero.
            if (__instance.health.currentValue > 0) {
                return false;  // Return false to skip the original method and prevent health restoration.
            }
        }
        return true;  // Allow the original method to run if conditions are not met.
    }

    // Prefix patch for the "RestoreEverything" method of the Player class.
    [HarmonyPrefix, HarmonyPatch(typeof(TeleportToSavePointAction), "OnStateEnterImplement")]
    public static bool PatchOnStateEnterImplement(ref TeleportToSavePointAction __instance) {
        ToastManager.Toast(__instance.transform.root.name);
        switch (__instance.transform.root.name) {
            case "A2_S5_ BossHorseman_GameLevel":
                Player.i.transform.position = new UnityEngine.Vector3(-4400f, -3288f, 0f);
                GameObject.Find("A2_S5_ BossHorseman_GameLevel/CameraCore").GetComponent<ProCamera2DNumericBoundaries>().enabled = false;
                break;
            case "A3_S5_BossGouMang_GameLevel (RCGLifeCycle)":
                Player.i.transform.position = new UnityEngine.Vector3(-4462f, -3788f, 0f);
                GameObject.Find("A2_S5_ BossHorseman_GameLevel/CameraCore").GetComponent<ProCamera2DNumericBoundaries>().enabled = false;
                break;
            case "A5_S5 (RCGLifeCycle)":
                ToastManager.Toast(SpeedMOB.Instance.b);
                Player.i.transform.position = new UnityEngine.Vector3(-2298f, -4264f, 0f);
                GameObject.Find("P2_R22_Savepoint_GameLevel (RCGLifeCycle)/Room/Prefab/EventBinder (Boss Fight 相關)").SetActive(true);
                GameObject.Find("A2_S5_ BossHorseman_GameLevel/CameraCore").GetComponent<ProCamera2DNumericBoundaries>().enabled = false;
                break;
            case "P2_R22_Savepoint_GameLevel (RCGLifeCycle)":
                //SpeedMOB.Instance.b.SetActive(false);
                Player.i.transform.position = new UnityEngine.Vector3(-21f, -3564f, 0f);
                GameObject.Find("A2_S5_ BossHorseman_GameLevel/CameraCore").GetComponent<ProCamera2DNumericBoundaries>().enabled = false;
                GameObject.Find("A10S5 (RCGLifeCycle)/Room/Boss And Environment Binder").SetActive(true);
                break;
            case "A10S5 (RCGLifeCycle)":
                var x = SpeedMOB.Instance.CreateTeleportPointData("A11_S0_Boss_YiGung", new Vector3(-2686f, -1104f, 0f));
                GameCore.Instance.TeleportToSavePoint(x);
                break;
        }
        return false;
    }
}
