namespace CedMod.Addons.Events.Interfaces
{
    public interface IEndConditionBehaviour
    {
        bool CanRoundEnd(bool baseGameConditionsSatisfied);
    }
}