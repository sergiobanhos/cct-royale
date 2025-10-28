// Scripts/Net/GameNetworkClient.cs
using UnityEngine;
using NativeWebSocket;
using System.Text;
using Core;
using Utils;
using Newtonsoft.Json;
using System;

public class GameNetworkClient : MonoSingleton<GameNetworkClient>
{
    [Header("Connection")]
    public string host = "ws://localhost:3000";
    public string roomId = "training";
    public string token  = "dev-token";
    public string playerName = "Player";
    public int playerIndex = 0;

    private WebSocket ws;
    private string clientId;

    public string ClientId => clientId;

    

    async void Start()
    {
        Application.runInBackground = true;
        var url = $"{host}/sync?token={token}&room={roomId}&name={System.Uri.EscapeDataString(playerName)}";
        ws = new WebSocket(url);

        ws.OnOpen   += () => Debug.Log("WS Open");
        ws.OnError  += (e) => Debug.LogError("WS Error: " + e);
        ws.OnClose  += (c) => Debug.LogWarning("WS Closed: " + c);
        ws.OnMessage+= (bytes) =>
        {
            var json = Encoding.UTF8.GetString(bytes);
            // garante que handlers de UI rodem no main thread
            MainThread.Run(() => Route(json));
        };

        await ws.Connect();
        InvokeRepeating(nameof(SendPing), 2f, 2f);
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        ws?.DispatchMessageQueue();
#endif
    }

    void OnApplicationQuit() { ws?.Close(); }

    void SendPing()
    {
        if (ws == null || ws.State != WebSocketState.Open) return;
        ws.SendText("{\"t\":\"PING\",\"ts\":0}");
    }

    // --------- roteamento para eventos ---------
    void Route(string json)
    {
        Debug.Log(json);
        var t = GetTypeTag(json);
        switch (t)
        {
            case "WELCOME":
                var w = JsonUtility.FromJson<SWelcome>(json);
                clientId = w.clientId;
                EventBus.Publish(new WelcomeEvent(clientId));
                // opcional: avisar sala explicitamente
                ws.SendText(JsonUtility.ToJson(new { t = "JOIN_MATCH", roomId = roomId, name = playerName }));
                break;

            case "JOINED":
                var j = JsonUtility.FromJson<SJoined>(json);
                var players = new (string id, string name)[j.players.Length];
                for (int i=0;i<players.Length;i++) players[i] = (j.players[i].id, j.players[i].name);
                EventBus.Publish(new JoinedEvent(j.roomId, players));
                break;

            case "SNAPSHOT":
                var s = JsonUtility.FromJson<SSnapshot>(json);
                EventBus.Publish(new SnapshotReceivedEvent(s.tick, s.time, s.state));
                break;

            case "EVENT":
                // como o payload é livre em JSON, quando for usar, troque JsonUtility por Newtonsoft
                // para já, vamos detectar emote simples:
                if (json.Contains("\"kind\":\"EMOTE\""))
                {
                    // hack leve pra extrair só o emoji e sender sem Newtonsoft
                    var sender = Extract(json, "\"by\":\"", "\"");
                    var emoji  = Extract(json, "\"emoji\":\"", "\"");
                    EventBus.Publish(new EmoteReceivedEvent(sender, emoji));
                }
                if (json.Contains("\"kind\":\"CARD_PLACED\""))
                {
                    var cardId = Extract(json, "\"cardId\":\"", "\"");
                    var x      = Extract(json, "\"x\":", ",");
                    var y      = Extract(json, "\"y\":", ",");
                    Vec2 position = new Vec2();
                    position.x = float.Parse(x);
                    position.y = float.Parse(y);
                    EventBus.Publish(new CardPlacedEvent(clientId,cardId, "a", position));
                }
                break;
        
            case "CARD_PLACED":
                var c = JsonUtility.FromJson<CardPlacedMsg>(json);
                EventBus.Publish(new CardPlacedEvent("a", c.cardId, "a", new Vec2()));
                break;

            case "ERROR":
                var err = JsonUtility.FromJson<SError>(json);
                EventBus.Publish(new NetworkErrorEvent(err.code, err.message));
                break;
        }
    }

    string GetTypeTag(string json)
    {
        int i = json.IndexOf("\"t\"");
        if (i < 0) return "";
        int c = json.IndexOf(':', i);
        int q1 = json.IndexOf('"', c + 1);
        int q2 = json.IndexOf('"', q1 + 1);
        if (q1 < 0 || q2 < 0) return "";
        return json.Substring(q1 + 1, q2 - q1 - 1);
    }
    string Extract(string src, string start, string end)
    {
        var i = src.IndexOf(start);
        if (i < 0) return "";
        i += start.Length;
        var j = src.IndexOf(end, i);
        if (j < 0) return "";
        return src.Substring(i, j - i);
    }

    // API de comandos (fica limpo)
    public void SendPlaceCard(string cardId, Vector2 world)
    {
        if (ws == null || ws.State != WebSocketState.Open) return;
        ws.SendText(JsonConvert.SerializeObject(new PlaceCardMsg {
            cardId = cardId, lane = "", x = world.x, y = world.y, ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        }));
    }

    public void SendEmote(string emoji)
    {
        if (ws == null || ws.State != WebSocketState.Open) return;
        ws.SendText(JsonUtility.ToJson(new { t="EMOTE", emoji }));
    }
}
