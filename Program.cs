using RaceDay_Part2.Data;
using RaceDay_Part2.Helpers;

var builder = WebApplication.CreateBuilder(args);

// MVC controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Session (for login state)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
//adding databasehelper and passwordhasher so the a user can be able to register in the system
builder.Services.AddSingleton<DatabaseHelper>();
builder.Services.AddScoped<PasswordHasher>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession();
app.MapControllers();

app.Run();

