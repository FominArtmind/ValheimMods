using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace EpicLoot_UnityLib
{
    public class EnchantUI : EnchantingTableUIPanelBase
    {
        public Text EnchantInfo;
        public Scrollbar EnchantInfoScrollbar;
        public List<Toggle> RarityButtons;
        public Button ClassSelection;
        public Text ClassSelectionText;

        [Header("Cost")]
        public Text CostLabel;
        public MultiSelectItemList CostList;

        public AudioClip[] EnchantCompleteSFX;

        public delegate List<InventoryItemListElement> GetEnchantableItemsDelegate();
        public delegate List<string> GetAvailableItemClassesDelegate(ItemDrop.ItemData item);
        public delegate string GetItemClassNameDelegate(string itemClass);
        public delegate string GetEnchantInfoDelegate(ItemDrop.ItemData item, string itemClass, MagicRarityUnity rarity);
        public delegate List<InventoryItemListElement> GetEnchantCostDelegate(ItemDrop.ItemData item, string itemClass, MagicRarityUnity rarity);
        // Returns the success dialog
        public delegate GameObject EnchantItemDelegate(ItemDrop.ItemData item, string itemClass, MagicRarityUnity rarity);

        public static GetEnchantableItemsDelegate GetEnchantableItems;
        public static GetAvailableItemClassesDelegate GetAvailableItemClasses;
        public static GetItemClassNameDelegate GetItemClassName;
        public static GetEnchantInfoDelegate GetEnchantInfo;
        public static GetEnchantCostDelegate GetEnchantCost;
        public static EnchantItemDelegate EnchantItem;

        private ToggleGroup _toggleGroup;
        private string _itemClass;
        private Dictionary<string, string> _itemLastSelectedClass = new Dictionary<string, string>();
        private MagicRarityUnity _rarity;
        private GameObject _successDialog;

        public override void Awake()
        {
            base.Awake();

            if (RarityButtons.Count > 0)
            {
                _toggleGroup = RarityButtons[0].group;
                _toggleGroup.EnsureValidState();
            }

            for (var index = 0; index < RarityButtons.Count; index++)
            {
                var rarityButton = RarityButtons[index];
                rarityButton.onValueChanged.AddListener((isOn) => {
                    if (isOn)
                        RefreshRarity();
                });
            }

            ClassSelection.onClick.AddListener(() =>
            {
                var selectedItem = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();
                var item = selectedItem?.Item1?.GetItem();
                if (item != null)
                {
                    ChangeItemClass(item);

                    ItemRarityOrClassChanged();
                }
            });
        }

        [UsedImplicitly]
        public void OnEnable()
        {
            var selectedItem = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();
            var item = selectedItem?.Item1?.GetItem();
            RestoreItemClass(item);
            _rarity = MagicRarityUnity.Magic;
            ItemRarityOrClassChanged();
            RarityButtons[0].isOn = true;
            var items = GetEnchantableItems();
            AvailableItems.SetItems(items.Cast<IListElement>().ToList());
        }

        public override void Update()
        {
            base.Update();

            if (!_locked && ZInput.IsGamepadActive())
            {
                if (ZInput.GetButtonDown("JoyButtonY"))
                {
                    var nextModeIndex = ((int)_rarity) % RarityButtons.Count;
                    RarityButtons[nextModeIndex].isOn = true;
                    ZInput.ResetButtonStatus("JoyButtonY");
                }

                if (EnchantInfoScrollbar != null)
                {
                    var rightStickAxis = ZInput.GetJoyRightStickY();
                    if (Mathf.Abs(rightStickAxis) > 0.5f)
                        EnchantInfoScrollbar.value = Mathf.Clamp01(EnchantInfoScrollbar.value + rightStickAxis * -0.1f);
                }
            }

            if (_successDialog != null && !_successDialog.activeSelf)
            {
                Unlock();
                Destroy(_successDialog);
                _successDialog = null;
            }
        }

        public void RefreshRarity()
        {
            var prevRarity = _rarity;
            for (var index = 0; index < RarityButtons.Count; index++)
            {
                var button = RarityButtons[index];
                if (button.isOn)
                {
                    _rarity = (MagicRarityUnity)index;
                }
            }

            if (prevRarity != _rarity)
                ItemRarityOrClassChanged();
        }

        public void ItemRarityOrClassChanged()
        {
            var selectedItem = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();
            var item = selectedItem?.Item1?.GetItem();
            if (item == null)
            {
                MainButton.interactable = false;
                EnchantInfo.text = "";
                CostLabel.enabled = false;
                CostList.SetItems(new List<IListElement>());
                return;
            }

            RestoreItemClass(item);

            var info = GetEnchantInfo(item, _itemClass, _rarity);

            EnchantInfo.text = info;
            ScrollEnchantInfoToTop();

            CostLabel.enabled = true;
            var cost = GetEnchantCost(item, _itemClass, _rarity);
            CostList.SetItems(cost.Cast<IListElement>().ToList());

            var canAfford = LocalPlayerCanAffordCost(cost);
            var featureUnlocked = EnchantingTableUI.instance.SourceTable.IsFeatureUnlocked(EnchantingFeature.Enchant);
            MainButton.interactable = featureUnlocked && canAfford;
        }

        private void ScrollEnchantInfoToTop()
        {
            EnchantInfoScrollbar.value = 1;
        }

        protected override void DoMainAction()
        {
            var selectedItem = AvailableItems.GetSelectedItems<InventoryItemListElement>().FirstOrDefault();

            Cancel();

            if (selectedItem?.Item1.GetItem() == null)
            {
                return;
            }

            var item = selectedItem.Item1.GetItem();
            var cost = GetEnchantCost(item, _itemClass, _rarity);

            var player = Player.m_localPlayer;
            if (!player.NoCostCheat())
            {
                if (!LocalPlayerCanAffordCost(cost))
                {
                    Debug.LogError("[Enchant Item] ERROR: Tried to enchant item but could not afford the cost. This should not happen!");
                    return;
                }

                foreach (var costElement in cost)
                {
                    InventoryManagement.Instance.RemoveItem(costElement.GetItem());
                }
            }

            if (_successDialog != null)
            {
                Destroy(_successDialog);
            }

            DeselectAll();
            Lock();

            _successDialog = EnchantItem(item, _itemClass, _rarity);

            RefreshAvailableItems();
        }

        protected override AudioClip GetCompleteAudioClip()
        {
            return EnchantCompleteSFX[(int)_rarity];
        }

        public void RefreshAvailableItems()
        {
            var items = GetEnchantableItems();
            AvailableItems.SetItems(items.Cast<IListElement>().ToList());
            AvailableItems.DeselectAll();
            OnSelectedItemsChanged();
        }

        protected override void OnSelectedItemsChanged()
        {
            ItemRarityOrClassChanged();
        }
        
        public override bool CanCancel()
        {
            return base.CanCancel() || (_successDialog != null && _successDialog.activeSelf);
        }

        public override void Cancel()
        {
            base.Cancel();

            if (_successDialog != null && _successDialog.activeSelf)
            {
                Destroy(_successDialog);
                _successDialog = null;
            }

            ItemRarityOrClassChanged();
        }

        public override void Lock()
        {
            base.Lock();

            foreach (var modeButton in RarityButtons)
            {
                modeButton.interactable = false;
            }
            ClassSelection.interactable = false;
        }

        public override void Unlock()
        {
            base.Unlock();
            
            for (var index = 0; index < RarityButtons.Count; index++)
            {
                RarityButtons[index].interactable = true;
            }
            ClassSelection.interactable = true;
        }

        public override void DeselectAll()
        {
            AvailableItems.DeselectAll();
        }

        public void ChangeItemClass(ItemDrop.ItemData item)
        {
            var itemClasses = GetAvailableItemClasses(item);
            if (itemClasses.Count == 0)
            {
                _itemClass = "Forgotten";
            }
            else
            {
                var index = itemClasses.FindIndex((value) => value == _itemClass);
                if (index == -1)
                {
                    _itemClass = itemClasses[0];
                    _itemLastSelectedClass[item.m_shared.m_name] = _itemClass;
                }
                else
                {
                    _itemClass = itemClasses[(index + 1) % itemClasses.Count()];
                    _itemLastSelectedClass[item.m_shared.m_name] = _itemClass;
                }
            }

            var name = GetItemClassName(_itemClass);
            if (name != null)
            {
                ClassSelectionText.text = name;
            }
        }

        public void RestoreItemClass(ItemDrop.ItemData item)
        {
            if (item == null)
            {
                _itemClass = "Forgotten";
            }
            else
            {
                var itemClasses = GetAvailableItemClasses(item);
                if (itemClasses.Count == 0)
                {
                    _itemClass = "Forgotten";
                }
                else
                {
                    if (_itemLastSelectedClass.TryGetValue(item.m_shared.m_name, out string lastClass))
                    {
                        if (itemClasses.Contains(lastClass))
                        {
                            _itemClass = lastClass;
                        }
                        else
                        {
                            _itemClass = itemClasses[0];
                            _itemLastSelectedClass[item.m_shared.m_name] = _itemClass;
                        }
                    }
                    else
                    {
                        _itemClass = itemClasses[0];
                        _itemLastSelectedClass[item.m_shared.m_name] = _itemClass;
                    }
                }
            }

            var name = GetItemClassName(_itemClass);
            if (name != null)
            {
                ClassSelectionText.text = name;
            }
        }
    }
}
