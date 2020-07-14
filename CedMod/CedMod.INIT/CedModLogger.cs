namespace CedMod.INIT
{
    public class CedModLogger : Logging.Logger
    {
        public override void Debug(string tag, string message)
        {
#if DEBUG
            Write("DEBUG", tag, message);
#endif
        }
        
        public override void Error(string tag, string message)
        {
            Write("ERROR", tag, message);
        }
        
        public override void Info(string tag, string message)
        {
            Write("INFO", tag, message);
        }
        
        public override void Warn(string tag, string message)
        {
            Write("WARN", tag, message);
        }
        
        private void Write(string level, string tag, string message)
        {
            ServerConsole.AddLog(string.Format("[{0}] [{1}] {2}", level, tag, message));
        }
    }
}