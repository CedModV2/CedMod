namespace CedMod.Handlers
{
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    public class Player
    {
        /// <inheritdoc cref="Events.Handlers.Player.OnDied(DiedEventArgs)"/>
        public void OnDied(DiedEventArgs ev)
        {
            Log.Info($"{ev.Target?.Nickname} died from {ev.HitInformations.GetDamageName()}! {ev.Killer?.Nickname} killed him!");
        }

        /// <inheritdoc cref="Events.Handlers.Player.OnChangingRole(ChangingRoleEventArgs)"/>
        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            Log.Info($"{ev.Player?.Nickname} ({ev.Player?.Role}) is changing his role! The new role will be {ev?.NewRole}!");
        }

        /// <inheritdoc cref="Events.Handlers.Player.OnChangingItem(ChangingItemEventArgs)"/>
        public void OnChangingItem(ChangingItemEventArgs ev)
        {
            Log.Info($"{ev.Player?.Nickname} is changing his {ev.OldItem.id} item to {ev.NewItem.id}!");
        }
    }
}