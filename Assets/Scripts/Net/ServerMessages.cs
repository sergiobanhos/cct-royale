using System;

[Serializable] public class SWelcome { public string t; public string clientId; }
[Serializable] public class SJoined  { public string t; public string roomId; public Player[] players; }
[Serializable] public class Player   { public string id; public string name; }
[Serializable] public class SSnapshot{ public string t; public int tick; public long time; public GameStateSnapshot state; }
[Serializable] public class SEvent { public string t; public string kind; public string payload; } // pode migrar p/ Newtonsoft depois

[Serializable] public class SError   { public string t; public string code; public string message; }
[Serializable] public class SPong    { public string t; public long ts; }
