using CedMod.CedMod.Logging;
using GameCore;

namespace CedMod.CedMod.INIT
{
    // Token: 0x020006C9 RID: 1737
    public class CedModLogger : Logger
    {
        // Token: 0x06002501 RID: 9473 RVA: 0x000B82A8 File Offset: 0x000B64A8
        public override void Debug(string tag, string message)
        {
            bool @bool = ConfigFile.ServerConfig.GetBool("cm_debug");
            if (@bool)
            {
                Write("DEBUG", tag, message);
            }
        }

        // Token: 0x06002502 RID: 9474 RVA: 0x00020DAE File Offset: 0x0001EFAE
        public override void Error(string tag, string message)
        {
            Write("ERROR", tag, message);
        }

        // Token: 0x06002503 RID: 9475 RVA: 0x00020DBF File Offset: 0x0001EFBF
        public override void Info(string tag, string message)
        {
            Write("INFO", tag, message);
        }

        // Token: 0x06002504 RID: 9476 RVA: 0x00020DD0 File Offset: 0x0001EFD0
        public override void Warn(string tag, string message)
        {
            Write("WARN", tag, message);
        }

        // Token: 0x06002505 RID: 9477 RVA: 0x00020DE1 File Offset: 0x0001EFE1
        private void Write(string level, string tag, string message)
        {
            ServerConsole.AddLog(string.Format("[{0}] [{1}] {2}", level, tag, message));
        }
    }
}
