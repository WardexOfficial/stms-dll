
namespace stms;

public class Logger
{
    public static void Log(string message)
    {
        try
        {
            string path = Path.Combine(Environment.CurrentDirectory, "dl17.log");
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}{Environment.NewLine}";
            File.AppendAllText(path, logMessage);
        }
        catch
        {
            Environment.Exit(0);
        }
    }
}