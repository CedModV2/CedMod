using EXILED;
using EXILED.Extensions;

namespace CedMod.GameMode.GangWar
{
	public class Commands
	{
		private readonly GangWar _plugin;
		public Commands(GangWar plugin) => _plugin = plugin;

		public void OnRaCommand(ref RACommandEvent ev)
		{
			if (!ev.Command.StartsWith("gang"))
				return;

			string[] args = ev.Command.Split(' ');

			switch (args[1].ToLower())
			{
				case "enable":
					ev.Allow = false;
					_plugin.Functions.EnableGamemode();
					ev.Sender.RAMessage("Gamemode enabled. c:");
					break;
				case "disable":
					ev.Allow = false;
					_plugin.Functions.DisableGamemode();
					ev.Sender.RAMessage("Gamemode disabled. :c");
					break;
			}
		}
	}
}