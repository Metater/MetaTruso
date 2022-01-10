namespace MetaTrusoClient;

public class MetaTruso
{
    private readonly static HttpClient HttpClient = new();
    private readonly string trusoKey;

    public MetaTruso(string trusoKey)
    {
        this.trusoKey = trusoKey;
    }

    public async Task<HttpResponseMessage> Delete(string files)
    {
        return await HttpClient.GetAsync(GetUri("delete", $"file={files}"));
    }

    public async Task<HttpResponseMessage> Execute(string script)
    {
        return await HttpClient.GetAsync(GetUri("execute", $"script={script}"));
    }

    public async Task<HttpResponseMessage> Upload(string filePath, string dest)
    {
        using FileStream fi = File.OpenRead(filePath);
        using MemoryStream fo = new();
        using (GZipStream fg = new(fo, CompressionMode.Compress))
        {
            await fi.CopyToAsync(fg);
        }
        return await HttpClient.PostAsync(GetUri("upload", $"file={dest}"), new ByteArrayContent(fo.ToArray()));
    }

    private string GetUri(string ep, string query)
    {
        return $"https://api.team3489.tk:8443/{ep}?key={trusoKey}&{query}";
    }
}