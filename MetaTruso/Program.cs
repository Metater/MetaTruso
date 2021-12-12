var builder = WebApplication.CreateBuilder();

string accessKey = args[0];

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
    Console.WriteLine(ctx.Request.Query["file"]);
    using (var ms = new MemoryStream())
    {
        try
        {
            string output = args[1] + ctx.Request.Query["file"];
            Console.WriteLine(output);
            await ctx.Request.Body.CopyToAsync(ms);
            byte[] data = ms.ToArray();
            Console.WriteLine(data.Length);
            await File.WriteAllBytesAsync(args[1] + ctx.Request.Query["file"], data);
            Console.WriteLine("Done!??");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }
    }
    Console.WriteLine("done?!@#123");
    await ctx.Response.WriteAsync("200");
});

app.MapGet("/delete", async ctx =>
{
    if (!ctx.Request.Query.ContainsKey("folder"))
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.WriteAsync("400");
        return;
    }
    Directory.Delete(args[1] + ctx.Request.Query["folder"], true);
});

HttpClient client = new();

app.MapGet("/run", async ctx =>
{
    //var content = new ByteArrayContent(new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x2c, 0x20, 0x57, 0x6f, 0x72, 0x6c, 0x64, 0x21 });
    var content = new StringContent(ctx.Request.Query["data"]);
    var res = await client.PostAsync("https://localhost:5001/post", content);
    string strRes = await res.Content.ReadAsStringAsync();
    await ctx.Response.WriteAsync(strRes);
});

await app.RunAsync();