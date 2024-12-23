using HarmonyLib;
using NineSolsAPI;

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
}
