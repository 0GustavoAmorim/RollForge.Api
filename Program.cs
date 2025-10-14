using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RollForge.Api.DTOs;
using RollForge.Api.Hubs;
using RollForge.Api.Models;
using RollForge.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<DiceService>();

builder.Services.AddCors(op =>
{
    op.AddDefaultPolicy(p =>
    {
        p.AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()
         .WithOrigins("http://localhost:5180");
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapGet("/", () => "RollForge API is running...");

//ENDPOINTS de Sessão
app.MapPost("/session", (SessionService sessionService, Session session) =>
{
    var created = sessionService.CreateSession(session.Name);
    return Results.Ok(created);
});

app.MapGet("/session/{id}", (SessionService sessionService, string id) =>
{
    var session = sessionService.GetSession(id);
    return session is null ? Results.NotFound() : Results.Ok(session);
});

app.MapPost("/session/{id}/join", (SessionService sessionService, string id, Player player) =>
{
    var session = sessionService.GetSession(id);
    if (session is null)
        return Results.NotFound("Sessão não encontrada.");

    sessionService.AddPlayer(id, player.Name);
    return Results.Ok(session);
});

app.MapPost("/session/{id}/roll", async(SessionService sessionService,DiceService diceService, IHubContext<GameHub> hub, string id, RollRequest req) =>
{
    var session = sessionService.GetSession(id);
    if (session is null)
        return Results.NotFound("Sessão não encontrada.");

    if (string.IsNullOrWhiteSpace(req.Player))
        return Results.BadRequest("Nome do jogador é obrigatório.");

    var roll = await diceService.RollDice(id, req.Player, req.Dice);

    await hub.Clients.Group(id).SendAsync("RollResult", roll);

    return Results.Ok(roll);
});


app.MapHub<GameHub>("/hub/game");

app.Run();
