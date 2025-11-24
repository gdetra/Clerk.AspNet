using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.Run();

// Make Program class public for WebApplicationFactory
public partial class Program { }