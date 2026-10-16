using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace RoomGame
{
    /// <summary>
    /// Create/join rooms through Multiplayer Services sessions (Lobby + Relay).
    /// The session starts Netcode as host/client over Relay; on WebGL the transport uses secure WebSockets.
    /// </summary>
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        public int maxPlayers = 10;

        public ISession Session { get; private set; }
        public event Action<string> Disconnected;

        bool leaving;

        void Awake() => Instance = this;

        void Start()
        {
            NetworkManager.Singleton.OnClientStopped += OnClientStopped;
        }

        async Task EnsureSignedIn()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                // A random profile per page load, so two tabs in one browser get different player IDs.
                var options = new InitializationOptions().SetProfile("p" + Guid.NewGuid().ToString("N").Substring(0, 16));
                await UnityServices.InitializeAsync(options);
            }
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        void ConfigureTransport()
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
#if UNITY_WEBGL
            transport.UseWebSockets = true; // WebGL has no UDP
#endif
        }

        public async Task<string> CreateRoom()
        {
            await EnsureSignedIn();
            ConfigureTransport();
            var options = new SessionOptions { MaxPlayers = maxPlayers, IsPrivate = true }.WithRelayNetwork();
            Session = await MultiplayerService.Instance.CreateSessionAsync(options);
            Hook();
            return Session.Code;
        }

        public async Task<string> JoinRoom(string code)
        {
            await EnsureSignedIn();
            ConfigureTransport();
            Session = await MultiplayerService.Instance.JoinSessionByCodeAsync(code.Trim().ToUpperInvariant());
            Hook();
            return Session.Code;
        }

        void Hook()
        {
            leaving = false;
            Session.RemovedFromSession += () => Lost("You left the room.");
            Session.Deleted += () => Lost("The host closed the room.");
        }

        public async Task Leave()
        {
            leaving = true;
            var s = Session;
            Session = null;
            if (s != null)
            {
                try { await s.LeaveAsync(); } catch (Exception e) { Debug.LogWarning(e.Message); }
            }
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
        }

        void OnClientStopped(bool wasHost)
        {
            if (leaving) return;
            Lost(wasHost ? "Room closed." : "Disconnected: the host left the room.");
        }

        void Lost(string reason)
        {
            if (leaving) return;
            leaving = true;
            Session = null;
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
            Disconnected?.Invoke(reason);
        }
    }
}
