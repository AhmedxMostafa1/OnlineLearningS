using Microsoft.EntityFrameworkCore;
using OnlineLearning.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register controller support
builder.Services.AddControllersWithViews();

// ✅ Optional: Authorization support
builder.Services.AddAuthorization();

// ✅ Register your DbContext
builder.Services.AddDbContext<OnlineLearningContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Add session BEFORE builder.Build()
builder.Services.AddSession();

var app = builder.Build();

// ✅ Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Enable session BEFORE authorization (important order)
app.UseSession();

app.UseAuthorization();

// ✅ Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


app.Run();
