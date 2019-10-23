namespace Kingdom.Roslyn.Compilation.Services
{
    public delegate void LogWriteLineFormatCallback(string format, params object[] args);

    public delegate void LogWriteLineMessageCallback(string message);
}
