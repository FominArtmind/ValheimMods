using System.Linq;
using HarmonyLib;
using UnityEngine;
using Raido;

namespace EpicLoot
{
    //public void AddDefaultItems()
    [HarmonyPatch(typeof(Container), nameof(Container.AddDefaultItems))]
    public static class Container_AddDefaultItems_Patch
    {
        // already created (on Awake call) default items should be removed first 
        public static void Prefix(Container __instance)
        {
            if (__instance == null || __instance.m_piece == null)
            {
                return;
            }

            __instance.m_inventory.RemoveAll();
        }

        public static void Postfix(Container __instance)
        {
            static string GetContainerCleanName(Container container)
            {
                return container.m_piece.name.Replace("(Clone)", "").Trim();
            }

            if (__instance == null || __instance.m_piece == null)
            {
                return;
            }

            __instance.m_inventory.RemoveAll();

            var containerName = GetContainerCleanName(__instance);

            var items = Raido.DropEngine.RollContainerDrop(containerName, __instance.transform.position);
            int distanceFromWorldCenter = (int)new Vector3(__instance.transform.position.x, 0, __instance.transform.position.z).magnitude;
            foreach (var item in items)
            {
                __instance.m_inventory.AddItem(item);
            }
        }
    }

    // Looks like there is no need to change this since all containers are not empty initially
    // [HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]

    [HarmonyPatch(typeof(Container), nameof(Container.RPC_RequestOpen))]
    public static class Container_RPC_RequestOpen_Patch
    {
        public static void Prefix(Container __instance, long uid, long playerID)
        {
            if (__instance == null || __instance.m_piece == null)
            {
                return;
            }

            if (__instance.m_nview.IsOwner() && !__instance.m_nview.GetZDO().GetBool("EL_container_items_rolled".GetStableHashCode()))
            {
                __instance.AddDefaultItems();
                __instance.m_nview.GetZDO().Set("EL_container_items_rolled".GetStableHashCode(), value: true);
            }
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.OnDestroyed))]
    public static class Container_OnDestroyed_Patch
    {
        public static void Prefix(Container __instance)
        {
            if (__instance == null || __instance.m_piece == null)
            {
                return;
            }

            if (__instance.m_nview.IsOwner() && !__instance.m_nview.GetZDO().GetBool("EL_container_items_rolled".GetStableHashCode()))
            {
                __instance.AddDefaultItems();
                __instance.m_nview.GetZDO().Set("EL_container_items_rolled".GetStableHashCode(), value: true);
            }
        }
    }
}
