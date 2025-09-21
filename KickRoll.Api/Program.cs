using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

// 使用 serviceAccount.json 初始化
var credentialPath = Path.Combine(AppContext.BaseDirectory, "serviceAccount.json");
var credential = GoogleCredential.FromFile(credentialPath);

var dbBuilder = new FirestoreDbBuilder
{
   ProjectId = builder.Configuration["GoogleProjectId"],
   Credential = credential
};

var db = dbBuilder.Build();
builder.Services.AddSingleton(db);

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();