using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

namespace CedMod.Addons.Events.Interfaces
{
    public interface IEndConditionBehaviour
    {
        bool CanRoundEnd(bool baseGameConditionsSatisfied);
    }
}