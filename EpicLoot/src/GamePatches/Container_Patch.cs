using HarmonyLib;

namespace EpicLoot
{
    public static class ContainerFiller
    {
        public static string GetContainerCleanName(Container __instance)
        {
            return __instance.m_piece.name.Replace("(Clone)", "").Trim();
        }

        public static void FillItems(Container __instance)
        {
            if (__instance == null || __instance.m_piece == null)
            {
                return;
            }

            if (__instance.m_nview.IsOwner() && !__instance.m_nview.GetZDO().GetBool("EL_container_items_rolled".GetStableHashCode()))
            {
                __instance.m_inventory.RemoveAll();

                var containerName = GetContainerCleanName(__instance);

                var items = Raido.DropEngine.RollContainerDrop(containerName, __instance.transform.position);

                foreach (var item in items)
                {
                    __instance.m_inventory.AddItem(item);
                }

                __instance.m_nview.GetZDO().Set("EL_container_items_rolled".GetStableHashCode(), value: true);
            }
        }
    }

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

            var containerName = ContainerFiller.GetContainerCleanName(__instance);
            var drop = Raido.DropEngine.Config.Chests.Find(value => value.Name == containerName);

            if (drop != null)
            {
                __instance.m_inventory.RemoveAll();

                var torchPrefab = ObjectDB.instance.GetItemPrefab("Torch");

                if (torchPrefab != null)
                {
                    var obj = LootRoller.SpawnLootForDrop(torchPrefab, __instance.transform.position, false);
                    var item = obj.GetComponent<ItemDrop>().m_itemData.Clone();
                    ZNetScene.instance.Destroy(obj);
                    __instance.m_inventory.AddItem(item);
                    EpicLoot.Log($"Added Torch for {__instance.m_piece.name} at {__instance.transform.position}");
                }
            }
        }

        public static void Postfix(Container __instance)
        {

        }
    }

    // Looks like there is no need to change this since all containers are not empty initially
    // [HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]

    [HarmonyPatch(typeof(Container), nameof(Container.RPC_RequestOpen))]
    public static class Container_RPC_RequestOpen_Patch
    {
        public static void Prefix(Container __instance, long uid, long playerID)
        {
            ContainerFiller.FillItems(__instance);
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.OnDestroyed))]
    public static class Container_OnDestroyed_Patch
    {
        public static void Prefix(Container __instance)
        {
            ContainerFiller.FillItems(__instance);
        }
    }
}
