using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

namespace CedMod.Addons.Events.Interfaces
{
    public interface IBulletHoleBehaviour
    {
        bool CanPlaceBulletHole(ImpactEffectsModule reg, Vector3 ray, RaycastHit hit);
    }
}