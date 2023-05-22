using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

namespace CedMod.Addons.Events.Interfaces
{
    public interface IBulletHoleBehaviour
    {
        bool CanPlaceBulletHole(StandardHitregBase reg, Ray ray, RaycastHit hit);
    }
}