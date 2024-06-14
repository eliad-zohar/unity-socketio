using System;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class SocketManager : MonoBehaviour
{
    public SocketIOUnity socket;
    private string socketToken = "eyJhbGciOiJIUzI1NiJ9.Mm9hbzB5bno5eDFrOWFrdnR2eDN5cG4.U3JwZatSvGT1QCoaIGPIasqOILe579v5vATWu6oHK0s"; // Replace with your actual token
    private int playerId = 2; // Replace with your actual player ID

    [Serializable]
    private class JoinMessage
    {
        public string soketToken;
        public int playerId;
    }

    // Start is called before the first frame update
    void Start()
    {
        //TODO: check the Uri if Valid.
        var uri = new Uri("https://socketservice-stg.shibadragon.app");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", "UNITY" }
            },
            EIO = 4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        // Reserved socket.io events
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("socket.OnConnected");
            SendJoinMessage();  // Send join message when connected
        };
        socket.OnPing += (sender, e) =>
        {
            Debug.Log("Ping");
        };
        socket.OnPong += (sender, e) =>
        {
            Debug.Log("Pong: " + e.TotalMilliseconds);
        };
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };

        // On Exist Game
        socket.On("backHome", (response) =>
        {
            Debug.Log("backHome Triggered!");
        });

        // On Exist Game
        socket.On("balanceUpdate", (response) =>
        {
            Debug.Log("balanceUpdate Triggered!");
        });

        // On Exist Game
        socket.On("jackpotWin", (response) =>
        {
            Debug.Log("jackpotWin Triggered!");
        });


        Debug.Log("Connecting...");
        socket.Connect();

        socket.OnUnityThread("spin", (data) =>
        {
            Debug.Log("Received spin event");
        });

        socket.OnAnyInUnityThread((name, response) =>
        {
          //  Debug.Log("Received On " + name + " : " + response.GetValue().GetRawText());
        });
    }

    private void SendJoinMessage()
    {
        var joinMessage = new JoinMessage
        {
            soketToken = socketToken,
            playerId = playerId
        };

        string json = JsonUtility.ToJson(joinMessage);
        Debug.Log($"Join message JSON: {json}");

        if (!string.IsNullOrEmpty(json))
        {
            socket.Emit("join", json);
            Debug.Log($"Sending join message: {json}");
        }
        else
        {
            Debug.LogError("Join message JSON is empty!");
        }
    }

    public void EmitTest(string eventName, string data)
    {
        if (!IsJSON(data))
        {
            socket.Emit(eventName, data);
        }
        else
        {
            socket.EmitStringAsJSON(eventName, data);
        }
    }

    public static bool IsJSON(string str)
    {
        if (string.IsNullOrWhiteSpace(str)) { return false; }
        str = str.Trim();
        if ((str.StartsWith("{") && str.EndsWith("}")) || // For object
            (str.StartsWith("[") && str.EndsWith("]"))) // For array
        {
            try
            {
                var obj = JToken.Parse(str);
                return true;
            }
            catch (Exception ex) // Some other exception
            {
                Debug.Log(ex.ToString());
                return false;
            }
        }
        else
        {
            return false;
        }
    }

}