using InventorySystem.Items.Firearms.Modules;
using LabApi.Events.Arguments.PlayerEvents;
using UnityEngine;

namespace CedMod.Addons.Events.Interfaces
{
    public interface IBulletHoleBehaviour
    {
        bool CanPlaceBulletHole(PlayerPlacingBulletHoleEventArgs ev);
    }
}