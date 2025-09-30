// Scripts/Core/GameEvents.cs
using Core;
using System.Collections.Generic;
using System;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}

public record WelcomeEvent(string ClientId) : IGameEvent;
public record JoinedEvent(string RoomId, (string id, string name)[] Players) : IGameEvent;
public record EmoteReceivedEvent(string SenderId, string Emoji) : IGameEvent;
public record CardPlacedEvent(string SenderId, string CardId, string Lane, Vec2 World) : IGameEvent;

// Snapshot minimalista
public class GameStateSnapshot
{
    public List<Unit> units;
    public List<Tower> towers;
}

public class Unit { public string id; public string ownerId; public Vec2 pos; public int hp; public string kind; }
public class Tower { public string id; public string ownerId; public Vec2 pos; public int hp; }
public struct Vec2 { public float x, y; }

public record SnapshotReceivedEvent(int Tick, long Time, GameStateSnapshot State) : IGameEvent;

public record NetworkErrorEvent(string Code, string Message) : IGameEvent;
