using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class NetworkRunnerService : MonoBehaviour, INetworkRunnerService, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkPrefabRef _playerPrefab;

        private PlayerInputActions _controls;

        private NetworkRunner _runner;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();

        public bool IsConnected => _runner != null && _runner.IsRunning;

        void Awake()
        {
            _controls = new PlayerInputActions();
            _controls.Enable();
        }

        public void Init()
        {
            if (_runner != null) return;

            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            _runner.AddCallbacks(this);
        }

        public void Shutdown()
        {
            if (_runner != null)
            {
                if (_runner.IsRunning) _runner.Shutdown();
                Destroy(_runner.gameObject);
                _runner = null;
            }
        }

        public void Host(string roomName, string password = "")
        {
            if (_runner == null) Init();
            StartGame(GameMode.Host, roomName);
        }

        public void Join(string roomName, string password = "")
        {
            if (_runner == null) Init();
            StartGame(GameMode.Client, roomName);
        }

        public void Leave()
        {
            if (_runner != null && _runner.IsRunning)
            {
                _runner.Shutdown();
                Debug.Log("Left session.");
            }
        }

        private async void StartGame(GameMode mode, string roomName)
        {
            const int GAME_SCENE_BUILD_INDEX = 2;
            var scene = SceneRef.FromIndex(GAME_SCENE_BUILD_INDEX);

            await _runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = roomName,
                Scene = scene,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });

            Debug.Log($"NetworkRunner started as {mode}");
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 1, 0);
                NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
                _spawnedCharacters.Add(player, networkPlayerObject);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();

            Vector2 move = _controls.Player.Move.ReadValue<Vector2>();
            data.Direction = new Vector3(move.x, 0, move.y);

            input.Set(data);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    }
}
