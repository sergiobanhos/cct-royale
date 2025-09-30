// Scripts/Net/ClientMessages.cs
using System;
using Newtonsoft.Json;

[Serializable]
public class JoinMatchMsg {
    [JsonProperty("t")] public string t = "JOIN_MATCH";
    [JsonProperty("roomId")] public string roomId;
    [JsonProperty("name")] public string name;
}

[Serializable]
public class PingMsg {
    [JsonProperty("t")] public string t = "PING";
    [JsonProperty("ts")] public long ts;
}

[Serializable]
public class EmoteMsg {
    [JsonProperty("t")] public string t = "EMOTE";
    [JsonProperty("emoji")] public string emoji;
}

[Serializable]
public class PlaceCardMsg {
    [JsonProperty("t")] public string t = "PLACE_CARD";
    [JsonProperty("cardId")] public string cardId;
    [JsonProperty("lane")] public string lane;
    [JsonProperty("x")] public float x;
    [JsonProperty("y")] public float y;
    [JsonProperty("ts")] public long ts;
}

[Serializable]
public class CardPlacedMsg
{
    [JsonProperty("t")] public string t = "CARD_PLACED";
    [JsonProperty("cardId")] public string cardId;
    [JsonProperty("lane")] public string lane;
    [JsonProperty("x")] public float x;
    [JsonProperty("y")] public float y;
    [JsonProperty("ts")] public long ts;
}