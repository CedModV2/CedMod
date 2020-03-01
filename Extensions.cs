using EXILED;
using EXILED.Extensions;

namespace CedMod
{
    public static class Extensions
    {

        public static void BC(uint time, string msg)
        {
            foreach (ReferenceHub p in Player.GetHubs())
                p.Broadcast(time, msg, false);
        }
    }
}
