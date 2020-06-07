using EXILED;
using EXILED.Extensions;

namespace CedMod.GameMode.Survival
{
	public class Commands
	{
		private readonly Player plugin;
		public Commands(Player plugin) => this.plugin = plugin;

		public void OnRaCommand(ref RACommandEvent ev)
		{
			if (!ev.Command.StartsWith("surv") && !ev.Command.StartsWith("survival"))
				return;
			
			string[] args = ev.Command.Split(' ');

			switch (args[1].ToLower())
			{
				case "enable":
					ev.Allow = false;
					plugin.Functions.EnableGamemode();
					ev.Sender.RAMessage("Gamemode enabled for the next round.");
					return;
				case "disable":
					ev.Allow = false;
					plugin.Functions.DisableGamemode();
					ev.Sender.RAMessage("Gamemode disabled.");
					return;
			}
		}
	}
}