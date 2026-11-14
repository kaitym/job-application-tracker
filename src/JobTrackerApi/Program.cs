var builder = WebApplication.CreateBuilder(args);
var jobs = new List<Job>();


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Web address for tracker
app.MapGet ("/jobs", () => jobs);

app.Run();

// Lets the test project see Program
public partial class Program { }
