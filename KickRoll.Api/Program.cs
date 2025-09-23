using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Firestore 初始化
builder.Services.AddSingleton(provider =>
{
    try 
    {
        var env = builder.Environment;
        var keyPath = Path.Combine(env.ContentRootPath, "Keys", "serviceAccount.json");

        Console.WriteLine($"[DEBUG] Checking Firestore key path: {keyPath}");
        
        if (!File.Exists(keyPath))
        {
            Console.WriteLine($"[WARNING] Firestore service account file not found at: {keyPath}");
            Console.WriteLine($"[WARNING] Firestore will not be available. API will use mock data for testing.");
            return null; // Return null to indicate Firestore is not available
        }

        // 讀 JSON，取得 project_id
        var json = File.ReadAllText(keyPath);
        using var doc = JsonDocument.Parse(json);
        var projectId = doc.RootElement.GetProperty("project_id").GetString();

        Console.WriteLine($"[DEBUG] Loaded project_id: {projectId}");

        // 載入憑證
        GoogleCredential credential;
        using (var stream = new FileStream(keyPath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream);
        }

        Console.WriteLine($"[DEBUG] Loaded credential type: {credential?.UnderlyingCredential?.GetType().Name}");

        // 建立 Firestore Client
        var client = new FirestoreClientBuilder
        {
            Credential = credential
        }.Build();

        return FirestoreDb.Create(projectId, client);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Failed to initialize Firestore: {ex.Message}");
        Console.WriteLine($"[WARNING] API will continue without Firestore. Some features may not work.");
        return null;
    }
});

// 🔹 MVC Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        // Remove naming policy to let JsonPropertyName attributes work
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

var app = builder.Build();

// Add request logging middleware
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/courses") && context.Request.Method == "POST")
    {
        Console.WriteLine($"[MIDDLEWARE] Incoming POST to {context.Request.Path}");
        Console.WriteLine($"[MIDDLEWARE] Content-Type: {context.Request.ContentType}");
        Console.WriteLine($"[MIDDLEWARE] Content-Length: {context.Request.ContentLength}");
        
        // Enable buffering to read the body multiple times
        context.Request.EnableBuffering();
        var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        Console.WriteLine($"[MIDDLEWARE] Request body: {body}");
        context.Request.Body.Position = 0; // Reset for controller
    }
    
    await next();
});

// 🔹 註冊 Controllers
app.MapControllers();

// 🔹 啟動訊息
Console.WriteLine("✅ KickRoll.Api 已啟動，Firestore 應該初始化完成！");

app.Run();