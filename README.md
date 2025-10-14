# RollForge.Api
Backend em .NET 9 + SignalR para sessões de RPG e rolagens de dados em tempo real.

---

## 🎯 Visão Geral

**RollForge.Api** é a API responsável por gerenciar **sessões de RPG**, **jogadores** e **rolagens de dados** em tempo real.  
Ela serve como o núcleo do projeto [RollForge.Web](https://github.com/0GustavoAmorim/RollForge.Web), permitindo que múltiplos jogadores se conectem a uma mesma mesa e acompanhem as rolagens instantaneamente.

A comunicação em tempo real é feita via **SignalR**, enquanto o armazenamento temporário é mantido em **memória** (com opção futura de persistência via SQLite).

---

## 🧰 Tecnologias

- [.NET 8](https://dotnet.microsoft.com/)
- [ASP.NET Core Minimal API](https://learn.microsoft.com/aspnet/core)
- [SignalR](https://learn.microsoft.com/aspnet/core/signalr/introduction)
- [Swagger / Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

---
