using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Firestore 初始化
builder.Services.AddSingleton(provider =>
{
   var env = builder.Environment;
   var keyPath = Path.Combine(env.ContentRootPath, "Keys", "serviceAccount.json");

   // 讀 JSON，取得 project_id
   var json = File.ReadAllText(keyPath);
   using var doc = JsonDocument.Parse(json);
   var projectId = doc.RootElement.GetProperty("project_id").GetString();

   Console.WriteLine($"[DEBUG] Firestore key path: {keyPath}");
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
});

// 🔹 MVC Controllers
builder.Services.AddControllers();

var app = builder.Build();

// 🔹 註冊 Controllers
app.MapControllers();

// 🔹 啟動訊息
Console.WriteLine("✅ KickRoll.Api 已啟動，Firestore 應該初始化完成！");

app.Run();

// Make the Program class public for testing
public partial class Program { }