using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace CedMod.QuerySystem
{
    public class PlayerEvents
    {
        public void OnPlayerLeave(LeftEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Player.Nickname} - {ev.Player.UserId} has left the server.");
            }
        }
        public void OnElevatorInteraction(InteractingElevatorEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Player.Nickname} - {ev.Player.UserId} has interacted with elevator.");
            }
        }
        
        public void OnPocketEnter(EnteringPocketDimensionEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Player.Nickname} - {ev.Player.Role} (<color={ev.Player.Role.GetColor().ToHex()}>{ev.Player.Role}</color>) has entered the pocket dimension.");
            }
        }
        
        public void OnPocketEscape(EscapingPocketDimensionEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Player.Nickname} - {ev.Player.Role} (<color={ev.Player.Role.GetColor().ToHex()}>{ev.Player.Role}</color>) has escaped the pocket dimension.");
            }	
        }
        
        public void On079Tesla(InteractingTeslaEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Player.Nickname} - {ev.Player.UserId} (<color={ev.Player.Role.GetColor().ToHex()}>{ev.Player.Role}</color>) has activated the tesla as 079.");
            }
        }
        
        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.HitInformations.Attacker} damaged {ev.Target.Nickname} - {ev.Target.UserId} (<color={ev.Target.Role.GetColor().ToHex()}>{ev.Target.Role}</color>) ammount {ev.Amount} with {DamageTypes.FromIndex(ev.Tool).name}.");
            }
        }
        
        public void OnPlayerDeath(DyingEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                {
                    if (FriendlyFireAutoban.IsTeakill(ev))
                        wss.SendMessage(
                            $"Teamkill ⚠: {ev.Killer.Nickname} - {ev.Killer.UserId} (<color={ev.Killer.Role.GetColor().ToHex()}>{ev.Killer.Role}</color>) killed {ev.Target.Nickname} - {ev.Target.UserId} (<color={ev.Target.Role.GetColor().ToHex()}>{ev.Target.Role}</color>) with {DamageTypes.FromIndex(ev.HitInformation.Tool).name}.");
                    else
                        wss.SendMessage(
                            $"{ev.Killer.Nickname} - {ev.Killer.UserId} (<color={ev.Killer.Role.GetColor().ToHex()}>{ev.Killer.Role}</color>) killed {ev.Target.Nickname} - {ev.Target.UserId} (<color={ev.Target.Role.GetColor().ToHex()}>{ev.Target.Role}</color>) with {DamageTypes.FromIndex(ev.HitInformation.Tool).name}.");
                }
            }
        }
        
        public void OnGrenadeThrown(ThrowingGrenadeEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Player.Nickname} - {ev.Player.UserId} (<color={ev.Player.Role.GetColor().ToHex()}>{ev.Player.Role}</color>) threw a grenade.");
            }
        }
        
        public void OnMedicalItem(UsedMedicalItemEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Player.Nickname} - {ev.Player.UserId} (<color={ev.Player.Role.GetColor().ToHex()}>{ev.Player.Role}</color>) Used a {ev.Item}.");
            }
        }
        
        public void OnSetClass(ChangingRoleEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Player.Nickname} - {ev.Player.UserId}'s role has been changed to <color={ev.NewRole.GetColor().ToHex()}>{ev.NewRole}</color>.");
            }
        }

        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"({ev.Player.Id}) {ev.Player.Nickname} - {ev.Player.UserId} joined the game.");
            }
        }
        
        public void OnPlayerFreed(RemovingHandcuffsEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Target.Nickname} - {ev.Target.UserId} (<color={ev.Target.Role.GetColor().ToHex()}>{ev.Target.Role}</color>) has been freed by {ev.Cuffer.Nickname} - {ev.Cuffer.UserId} (<color={ev.Cuffer.Role.GetColor().ToHex()}>{ev.Cuffer.Role}</color>).");
            }
        }

        public void OnPlayerHandcuffed(HandcuffingEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Target.Nickname} - {ev.Target.UserId} (<color={ev.Target.Role.GetColor().ToHex()}>{ev.Target.Role}</color>) has been cuffed by {ev.Cuffer.Nickname} - {ev.Cuffer.UserId} (<color={ev.Cuffer.Role.GetColor().ToHex()}>{ev.Cuffer.Role}</color>).");
            }
        }
    }
}