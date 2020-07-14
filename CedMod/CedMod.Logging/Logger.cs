namespace CedMod.CedMod.Logging
{
    // Token: 0x020006CB RID: 1739
    public abstract class Logger
    {
        // Token: 0x0600250C RID: 9484
        public abstract void Debug(string tag, string message);

        // Token: 0x0600250D RID: 9485
        public abstract void Info(string tag, string message);

        // Token: 0x0600250E RID: 9486
        public abstract void Warn(string tag, string message);

        // Token: 0x0600250F RID: 9487
        public abstract void Error(string tag, string message);
    }
}