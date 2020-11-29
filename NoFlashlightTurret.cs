using System;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("No Flashlight AutoTurret", "Malmo", "1.0.0")]
    [Description("Blocks flashlights to be attached to turrets")]
    class NoFlashlightTurret : RustPlugin
    {

        #region Constants
        
        const string atPrefabName = "assets/prefabs/npc/autoturret/autoturret_deployed.prefab";
        const string flashlightShortname = "weapon.mod.flashlight";

        #endregion

        #region Init
        private new void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["ProhibitedMessage"] = "Flashlight attachment on guns in auto turrets are prohibited!",
            }, this);
        }

        #endregion

        #region Helper functions

        private string GetLang(string langKey, string playerId = null, params object[] args)
        {
            return string.Format(lang.GetMessage(langKey, this, playerId), args);
        }

        private bool ItemHasFlashlight(Item item) {
            if(item == null)
            {
                return false;
            }

            if (item.contents == null)
            {
                return false;
            }

            foreach(var attachment in item.contents.itemList){
                if(attachment.info.shortname == flashlightShortname)
                {
                    return true;
                }
            }
            
            return false;
        }

        private bool ItemIsFlashlight(Item item) => item != null && item.info.shortname == flashlightShortname;

        private bool ContainerIsAT(ItemContainer itemContainer) => itemContainer.entityOwner != null && itemContainer.entityOwner.PrefabName == atPrefabName;

        private bool ContainerIsWeaponSlotInAT(ItemContainer container) => container.parent != null && container.parent.info.category == ItemCategory.Weapon && ContainerIsAT(container.parent.GetRootContainer());

        private void WarnUsingFlashlight(BasePlayer player)
        {
            player.IPlayer.Message(GetLang("ProhibitedMessage", player.UserIDString));
        }

        #endregion

        #region Hooks

        object CanMoveItem(Item item, PlayerInventory playerLoot, uint targetContainer, int targetSlot, int amount)
        {
            bool prohibit = false;
            try
            {
                ItemContainer container = playerLoot.FindContainer(targetContainer);
                ItemContainer originalContainer = item.GetRootContainer();

                var targetItem = container.GetSlot(targetSlot);

                // check if flashlight is tried to be put on a weapon inside a turret
                if (ItemIsFlashlight(item) && ContainerIsWeaponSlotInAT(container))
                {
                    prohibit = true;
                }

                // check if weapon with flashlight is tried to be put in a turret
                else if (ContainerIsAT(container) && ItemHasFlashlight(item))
                {
                    prohibit = true;
                }

                // prevent swapping weapon in turret for a weapon with flashlight
                else if(ItemHasFlashlight(targetItem) && ContainerIsAT(originalContainer))
                {
                    prohibit = true;
                }

                // prevent swapping attachment on weapon in a turret with a flashlight
                else if(ItemIsFlashlight(targetItem) && ContainerIsAT(originalContainer))
                {
                    prohibit = true;
                }

            } catch(Exception e) {
                throw e;
            }

            if (prohibit)
            {
                WarnUsingFlashlight(playerLoot._baseEntity);
                return false;
            }

            return null;
        }

        #endregion

    }
}
