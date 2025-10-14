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

app.MapPost("/session/{id}/roll", async (SessionService sessionService, DiceService diceService, IHubContext<GameHub> hub, string id, RollRequest req) =>
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

app.MapPost("/session/{id}/leave", async (SessionService sessionService, IHubContext<GameHub> Hub, string id, Player player) =>
{
    var session = sessionService.GetSession(id);
    if (session is null)
        return Results.NotFound("Sessão não encontrada.");

    sessionService.RemovePlayer(id, player.Name);
    await Hub.Clients.Group(id).SendAsync("PlayerLeft", new
    {
        player = player.Name,
        message = $"{player.Name} saiu da sessão.",
    });

    if (!sessionService.SessionExists(id))
    {
        await Hub.Clients.Group(id).SendAsync("SessionEnded", new
        {
            sessionId = id,
            message = $"Sessão {session.Name} encerrada (sem jogadores).",
        });

        return Results.Ok(new
        {
            message = $"jogador {player.Name} saiu. Sessão encerrada automagicamente."
        });
    }

    return Results.Ok(new
    {
        message = $"jogador {player.Name} saiu."
    });
});

app.MapDelete("session/{id}", async (SessionService sessionService, IHubContext<GameHub> Hub, string id) =>
{
    var session = sessionService.GetSession(id);
    if (session is null)
        return Results.NotFound("Sessão não encontrada.");

    sessionService.RemoveSession(id);
    await Hub.Clients.Group(id).SendAsync("SessionEnded", new
    {
        sessionId = id,
        message = $"Sessão {session.Name} encerrada manualmente.",
    });

    return Results.NoContent();
});

app.MapHub<GameHub>("/hub/game");

app.Run();
