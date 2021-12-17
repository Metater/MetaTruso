var builder = WebApplication.CreateBuilder();

string accessKey = args[0];
string filesPath = args[1];

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (ctx, next) =>
{
    if (ctx.Request.Host.ToString() != "db.team3489.tk:8443")
    {
        ctx.Response.StatusCode = 403;
        await ctx.Response.WriteAsync("403");
        return;
    }
    if (!ctx.Request.Query.ContainsKey("key"))
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsync("400");
        return;
    }
    if (ctx.Request.Query["key"] != accessKey)
    {
        ctx.Response.StatusCode = 401;
        await ctx.Response.WriteAsync("401");
        return;
    }
    ctx.Features.Get<IHttpMaxRequestBodySizeFeature>()!.MaxRequestBodySize = null;
    await next(ctx);
});

app.MapPost("/upload", async ctx =>
{
    if (!ctx.Request.Query.ContainsKey("file"))
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsync("400");
        return;
    }
    Console.WriteLine($"[MetaTruso] Got upload request for {ctx.Request.Query["file"]}");
    using GZipStream decompressor = new(ctx.Request.Body, CompressionMode.Decompress);
    string path = filesPath + ctx.Request.Query["file"];
    using (FileStream output = File.Create(path))
    {
        await decompressor.CopyToAsync(output);
    }
    await ctx.Response.WriteAsync("200");
});

app.MapGet("/delete", async ctx =>
{
    if (!ctx.Request.Query.ContainsKey("file"))
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsync("400");
        return;
    }
    Console.WriteLine($"[MetaTruso] Got delete request");
    string[] files = ctx.Request.Query["file"].ToString().Split(',');
    foreach (string file in files)
    {
        string path = filesPath + file;
        if (File.Exists(path))
        {
            File.Delete(path);
            Console.WriteLine($"[MetaTruso] Deleting file at {path}");
        }
        else
        {
            Console.WriteLine($"[MetaTruso] Tried to delete file at {path}");
        }
    }
    await ctx.Response.WriteAsync("200");
});

app.MapGet("/execute", async ctx =>
{
    if (!ctx.Request.Query.ContainsKey("script"))
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsync("400");
        return;
    }
    string path = filesPath + ctx.Request.Query["script"];
    Console.WriteLine($"[MetaTruso] Got execute request for {path}");
    if (!File.Exists(path))
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsync("400");
        return;
    }
    Process? process = Process.Start(new ProcessStartInfo()
    {
        UseShellExecute = false,
        Arguments = path
    });
    await process!.WaitForExitAsync();
    await ctx.Response.WriteAsync("200");
});

await app.RunAsync();