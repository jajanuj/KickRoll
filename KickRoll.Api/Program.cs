using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using KickRoll.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
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

// 🔹 Firebase Admin 初始化
builder.Services.AddSingleton(provider =>
{
   var env = builder.Environment;
   var keyPath = Path.Combine(env.ContentRootPath, "Keys", "serviceAccount.json");

   // Initialize Firebase Admin SDK
   if (FirebaseApp.DefaultInstance == null)
   {
      FirebaseApp.Create(new AppOptions()
      {
         Credential = GoogleCredential.FromFile(keyPath)
      });
   }

   return FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
});

// 🔹 Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{GetProjectIdFromServiceAccount()}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{GetProjectIdFromServiceAccount()}",
            ValidateAudience = true,
            ValidAudience = GetProjectIdFromServiceAccount(),
            ValidateLifetime = true
        };
        
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                // Add custom claims to context
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    var firebaseToken = context.SecurityToken as Microsoft.IdentityModel.JsonWebTokens.JsonWebToken;
                    if (firebaseToken != null)
                    {
                        // Add Firebase custom claims
                        if (firebaseToken.TryGetClaim("role", out var roleClaim))
                            claimsIdentity.AddClaim(new Claim("role", roleClaim.Value));
                            
                        if (firebaseToken.TryGetClaim("email", out var emailClaim))
                            claimsIdentity.AddClaim(new Claim("email", emailClaim.Value));
                            
                        if (firebaseToken.TryGetClaim("email_verified", out var emailVerifiedClaim))
                            claimsIdentity.AddClaim(new Claim("email_verified", emailVerifiedClaim.Value));
                            
                        if (firebaseToken.TryGetClaim("status", out var statusClaim))
                            claimsIdentity.AddClaim(new Claim("status", statusClaim.Value));
                            
                        // Add user_id from Firebase uid
                        claimsIdentity.AddClaim(new Claim("user_id", firebaseToken.Subject));
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

// 🔹 Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// 🔹 MVC Controllers
builder.Services.AddControllers();

// 🔹 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 🔹 Configure the HTTP request pipeline
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// 🔹 註冊 Controllers
app.MapControllers();

// 🔹 啟動訊息
Console.WriteLine("✅ KickRoll.Api 已啟動，Firebase Auth 與 Firestore 應該初始化完成！");

app.Run();

// Helper method to get project ID
static string GetProjectIdFromServiceAccount()
{
    try
    {
        var keyPath = Path.Combine(Directory.GetCurrentDirectory(), "Keys", "serviceAccount.json");
        var json = File.ReadAllText(keyPath);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("project_id").GetString() ?? "";
    }
    catch
    {
        return "";
    }
}

// Make the Program class public for testing
public partial class Program { }