using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

string connectionString = "Data Source=UserData.db;";
using (var conn = new SqliteConnection(connectionString))
{
    conn.Open();
    string sql = @"CREATE TABLE IF NOT EXISTS Users (
                    Username TEXT PRIMARY KEY, 
                    PasswordHash TEXT)";
    using var cmd = new SqliteCommand(sql, conn);
    cmd.ExecuteNonQuery();
}

string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    var builder = new StringBuilder();
    foreach (var byteValue in bytes)
    {
        builder.Append(byteValue.ToString("x2"));
    }
    return builder.ToString();
}

app.MapGet("/", async context =>
{
    await context.Response.SendFileAsync("wwwroot/index.html");
});

app.MapPost("/createaccount", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();
    var confirmPassword = form["confirmpassword"].ToString();

    if (password != confirmPassword)
    {
        await context.Response.WriteAsync("Passwords do not match.");
        return;
    }

    using var conn = new SqliteConnection(connectionString);
    conn.Open();
    using var checkCmd = new SqliteCommand("SELECT 1 FROM Users WHERE Username = @Username", conn);
    checkCmd.Parameters.AddWithValue("@Username", username);
    var exists = checkCmd.ExecuteScalar();

    if (exists != null)
    {
        await context.Response.WriteAsync("Username already exists.");
        return;
    }

    var hashedPassword = HashPassword(password);
    using var insertCmd = new SqliteCommand("INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)", conn);
    insertCmd.Parameters.AddWithValue("@Username", username);
    insertCmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
    insertCmd.ExecuteNonQuery();

    context.Response.Cookies.Append("username", username);
    context.Response.Redirect("/home");
});

app.MapPost("/signin", async context =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();
    var hashedPassword = HashPassword(password);

    using var conn = new SqliteConnection(connectionString);
    conn.Open();
    using var cmd = new SqliteCommand("SELECT PasswordHash FROM Users WHERE Username = @Username", conn);
    cmd.Parameters.AddWithValue("@Username", username);
    var storedHashedPassword = cmd.ExecuteScalar() as string;

    if (storedHashedPassword != null && storedHashedPassword == hashedPassword)
    {
        context.Response.Cookies.Append("username", username);
        context.Response.Redirect("/home");
    }
    else
    {
        context.Response.Redirect("/signin.html?error=Invalid+username+or+password");
    }
});

app.MapGet("/home", async context =>
{
    if (context.Request.Cookies.ContainsKey("username"))
    {
        await context.Response.SendFileAsync("wwwroot/home.html");
    }
    else
    {
        context.Response.Redirect("/signin.html");
    }
});

app.MapGet("/signout", context =>
{
    context.Response.Cookies.Delete("username");
    context.Response.Redirect("/");
    return Task.CompletedTask;
});

app.Run();
