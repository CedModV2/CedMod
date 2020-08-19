using System;
using Sentry;

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

        public override void LogException(Exception ex,string classname, string methodname)
        {
            //SentrySdk.CaptureMessage("Exception thrown at: " + classname + "." + methodname + " Exception" + ex.ToString());
            SentrySdk.CaptureException(ex);
            Initializer.Logger.Debug("CedModERRORREPORTING", "Reporting exception");
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