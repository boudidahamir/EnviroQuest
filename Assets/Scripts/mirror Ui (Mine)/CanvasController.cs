using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.ProBuilder.Shapes;
using Mirror.Examples.MultipleMatch;

public class CanvasController : MonoBehaviour
    {
        /// <summary>
        /// Match Controllers listen for this to terminate their match and clean up
        /// </summary>
        public event Action<NetworkConnectionToClient> OnPlayerDisconnected;

        /// <summary>
        /// Cross-reference of client that created the corresponding match in openMatches below
        /// </summary>
        internal static readonly Dictionary<NetworkConnectionToClient, Guid> playerMatches = new Dictionary<NetworkConnectionToClient, Guid>();

        /// <summary>
        /// Open matches that are available for joining
        /// </summary>
        internal static readonly Dictionary<Guid, MatchInfo> openMatches = new Dictionary<Guid, MatchInfo>();

        /// <summary>
        /// Network Connections of all players in a match
        /// </summary>
        internal static readonly Dictionary<Guid, HashSet<NetworkConnectionToClient>> matchConnections = new Dictionary<Guid, HashSet<NetworkConnectionToClient>>();

        /// <summary>
        /// Player informations by Network Connection
        /// </summary>
        internal static readonly Dictionary<NetworkConnection, PlayerInfo> playerInfos = new Dictionary<NetworkConnection, PlayerInfo>();
        internal static readonly Dictionary<NetworkConnection, ObjectInfo> objectInfos = new Dictionary<NetworkConnection, ObjectInfo>();

        /// <summary>
        /// Network Connections that have neither started nor joined a match yet
        /// </summary>
        internal static readonly List<NetworkConnectionToClient> waitingConnections = new List<NetworkConnectionToClient>();

        /// <summary>
        /// GUID of a match the local player has created
        /// </summary>
        internal Guid localPlayerMatch = Guid.Empty;

        /// <summary>
        /// GUID of a match the local player has joined
        /// </summary>
        internal Guid localJoinedMatch = Guid.Empty;

        /// <summary>
        /// GUID of a match the local player has selected in the Toggle Group match list
        /// </summary>
        internal Guid selectedMatch = Guid.Empty;

        // Used in UI for "Player #"
        int playerIndex = 1;
        [Header("PREFABS References")]
        public GameObject InGamePlayer;
        public GameObject[] InGameObjects;
        public GameObject[] InGameObjects2;

        [Header("GUI References")]
        public GameObject matchList;
        public GameObject matchPrefab;
        public GameObject matchControllerPrefab;
        public Button createButton;
        public Button joinButton;
        public GameObject lobbyView;
        public GameObject roomView;
        public RoomGUI roomGUI;
        public ToggleGroup toggleGroup;
        public GameObject HUB;
        public GameObject TreeGrowMap;
        public int randomScene;
        public int[] sceneList;

        void Update()
        {
            if(!GameObject.Find("hub"))
                { return; }
            HUB= GameObject.Find("hub");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetStatics()
        {
            playerMatches.Clear();
            openMatches.Clear();
            matchConnections.Clear();
            playerInfos.Clear();
            waitingConnections.Clear();
        }

        #region UI Functions

        // Called from several places to ensure a clean reset
        //  - MatchNetworkManager.Awake
        //  - OnStartServer
        //  - OnStartClient
        //  - OnClientDisconnect
        //  - ResetCanvas
        internal void InitializeData()
        {
            playerMatches.Clear();
            openMatches.Clear();
            matchConnections.Clear();
            waitingConnections.Clear();
            localPlayerMatch = Guid.Empty;
            localJoinedMatch = Guid.Empty;
        }

        // Called from OnStopServer and OnStopClient when shutting down
        void ResetCanvas()
        {
            InitializeData();
            lobbyView.SetActive(false);
            roomView.SetActive(false);
            gameObject.SetActive(false);
        }

        #endregion

        #region Button Calls

        [ClientCallback]
        public void SelectMatch(Guid matchId)
        {
            if (matchId == Guid.Empty)
            {
                selectedMatch = Guid.Empty;
                joinButton.interactable = false;
            }
            else
            {
                if (!openMatches.Keys.Contains(matchId))
                {
                    joinButton.interactable = false;
                    return;
                }

                selectedMatch = matchId;
                MatchInfo infos = openMatches[matchId];
                joinButton.interactable = infos.players < infos.maxPlayers;
            }
        }

        [ClientCallback]
        public void RequestCreateMatch()
        {
            NetworkClient.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.Create });
        }
        
        [ClientCallback]
        public void RequestNextRound(Guid m)
        {
            NetworkClient.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.NextRound ,matchId=m });
        }

        [ClientCallback]
        public void RequestCancelMatch()
        {
            if (localPlayerMatch == Guid.Empty) return;

            NetworkClient.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.Cancel });
        }

        [ClientCallback]
        public void RequestCancelLobby()
        {

            lobbyView.SetActive(false);
            roomView.SetActive(false);
            HUB.SetActive(true);
            
        }

        [ClientCallback]
        public void RequestJoinMatch()
        {
            if (selectedMatch == Guid.Empty) return;

            NetworkClient.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.Join, matchId = selectedMatch });
        }

        [ClientCallback]
        public void RequestLeaveMatch()
        {
            if (localJoinedMatch == Guid.Empty) return;

            NetworkClient.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.Leave, matchId = localJoinedMatch });
        }

        

        [ClientCallback]
        public void RequestReadyChange()
        {
            if (localPlayerMatch == Guid.Empty && localJoinedMatch == Guid.Empty) return;

            Guid matchId = localPlayerMatch == Guid.Empty ? localJoinedMatch : localPlayerMatch;

            NetworkClient.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.Ready, matchId = matchId });
        }

        [ClientCallback]
        public void RequestStartMatch()
        {
            if (localPlayerMatch == Guid.Empty) return;
            
            NetworkClient.Send(new ServerMatchMessage { serverMatchOperation = ServerMatchOperation.Start });
        }

    [ClientCallback]
        public void OnMatchEnded()
        {
            localPlayerMatch = Guid.Empty;
            localJoinedMatch = Guid.Empty;
            ShowLobbyView();
        }

        #endregion

        #region Server & Client Callbacks

        // Methods in this section are called from MatchNetworkManager's corresponding methods

        [ServerCallback]
        internal void OnStartServer()
        {
            InitializeData();
            NetworkServer.RegisterHandler<ServerMatchMessage>(OnServerMatchMessage);
            
        }

        [ServerCallback]
        internal void OnServerReady(NetworkConnectionToClient conn)
        {
            waitingConnections.Add(conn);
            playerInfos.Add(conn, new PlayerInfo { playerIndex = this.playerIndex, ready = false });
            playerIndex++;

            SendMatchList();
        }

        [ServerCallback]
        internal IEnumerator OnServerDisconnect(NetworkConnectionToClient conn)
        {
            // Invoke OnPlayerDisconnected on all instances of MatchController
            OnPlayerDisconnected?.Invoke(conn);

            if (playerMatches.TryGetValue(conn, out Guid matchId))
            {
                playerMatches.Remove(conn);
                openMatches.Remove(matchId);

                foreach (NetworkConnectionToClient playerConn in matchConnections[matchId])
                {
                    PlayerInfo _playerInfo = playerInfos[playerConn];
                    _playerInfo.ready = false;
                    _playerInfo.matchId = Guid.Empty;
                    playerInfos[playerConn] = _playerInfo;
                    playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Departed });
                }
            }

            foreach (KeyValuePair<Guid, HashSet<NetworkConnectionToClient>> kvp in matchConnections)
                kvp.Value.Remove(conn);

            PlayerInfo playerInfo = playerInfos[conn];
            if (playerInfo.matchId != Guid.Empty)
            {
                if (openMatches.TryGetValue(playerInfo.matchId, out MatchInfo matchInfo))
                {
                    matchInfo.players--;
                    openMatches[playerInfo.matchId] = matchInfo;
                }

                HashSet<NetworkConnectionToClient> connections;
                if (matchConnections.TryGetValue(playerInfo.matchId, out connections))
                {
                    PlayerInfo[] infos = connections.Select(playerConn => playerInfos[playerConn]).ToArray();

                    foreach (NetworkConnectionToClient playerConn in matchConnections[playerInfo.matchId])
                        if (playerConn != conn)
                            playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.UpdateRoom, playerInfos = infos });
                }
            }

            SendMatchList();

            yield return null;
        }

        [ServerCallback]
        internal void OnStopServer()
        {
            ResetCanvas();
        }

        [ClientCallback]
        internal void OnClientConnect()
        {
            playerInfos.Add(NetworkClient.connection, new PlayerInfo { playerIndex = this.playerIndex, ready = false });
        }

        [ClientCallback]
        internal void OnStartClient()
        {
            InitializeData();
            /*ShowLobbyView();*/
            createButton.gameObject.SetActive(true);
            joinButton.gameObject.SetActive(true);
            NetworkClient.RegisterHandler<ClientMatchMessage>(OnClientMatchMessage);
        }

        [ClientCallback]
        internal void OnClientDisconnect()
        {
            InitializeData();
        }

        [ClientCallback]
        internal void OnStopClient()
        {
            ResetCanvas();
        }

        #endregion

        #region Server Match Message Handlers

        [ServerCallback]
        void OnServerMatchMessage(NetworkConnectionToClient conn, ServerMatchMessage msg)
        {
            switch (msg.serverMatchOperation)
            {
                case ServerMatchOperation.None:
                    {
                        Debug.LogWarning("Missing ServerMatchOperation");
                        break;
                    }
                case ServerMatchOperation.Create:
                    {
                        OnServerCreateMatch(conn);
                        break;
                    }
                case ServerMatchOperation.Cancel:
                    {
                        OnServerCancelMatch(conn);
                        break;
                    }
                case ServerMatchOperation.Join:
                    {
                        OnServerJoinMatch(conn, msg.matchId);
                        break;
                    }
                case ServerMatchOperation.Leave:
                    {
                        OnServerLeaveMatch(conn, msg.matchId);
                        break;
                    }
                case ServerMatchOperation.Ready:
                    {
                        OnServerPlayerReady(conn, msg.matchId);
                        break;
                    }
                case ServerMatchOperation.Start:
                {
                    Guid matchId;
                    MatchInfo matchInfo;
                    if (playerMatches.TryGetValue(conn, out matchId))
                    {
                        if (openMatches.TryGetValue(matchId, out matchInfo))
                        {
                            StartCoroutine(loadscene(matchInfo.maps[0]));
                            Debug.Log("MatchInfo retrieved successfully: " + matchInfo.maps[0]);
                            OnServerStartMatch(conn, matchInfo.maps[0]);
                        }
                        else
                        {
                            Debug.LogError("Failed to find MatchInfo for match ID: " + matchId);
                        }
                    }
                    else
                    {
                        Debug.LogError("Failed to find match ID for the player.");
                    }

                        
                        break;
                    }
                case ServerMatchOperation.NextRound:
                {
                        OnServerNextRound(conn, msg.matchId);
                        break;
                    }
        }
        }

        [ServerCallback]
        void OnServerCreateMatch(NetworkConnectionToClient conn)
        {
            if (playerMatches.ContainsKey(conn)) return;
            sceneList = new int[2];
            randomScene = UnityEngine.Random.Range(2, 4);
            sceneList[0] = randomScene;
            if (randomScene == 2)
            {
                sceneList[1] = 3;
            }
            else
            {
                sceneList[1] = 2;
            }
            Guid newMatchId = Guid.NewGuid();
            matchConnections.Add(newMatchId, new HashSet<NetworkConnectionToClient>());
            matchConnections[newMatchId].Add(conn);
            playerMatches.Add(conn, newMatchId);
            openMatches.Add(newMatchId, new MatchInfo { matchId = newMatchId, maxPlayers = 100, players = 1 , maps=sceneList });;

            PlayerInfo playerInfo = playerInfos[conn];
            playerInfo.ready = false;
            playerInfo.matchId = newMatchId;
            playerInfos[conn] = playerInfo;

            PlayerInfo[] infos = matchConnections[newMatchId].Select(playerConn => playerInfos[playerConn]).ToArray();

            conn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Created, matchId = newMatchId, playerInfos = infos });

            SendMatchList();
        }

        [ServerCallback]
        void OnServerCancelMatch(NetworkConnectionToClient conn)
        {
            if (!playerMatches.ContainsKey(conn)) return;

            conn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Cancelled });

            Guid matchId;
            if (playerMatches.TryGetValue(conn, out matchId))
            {
                playerMatches.Remove(conn);
                openMatches.Remove(matchId);

                foreach (NetworkConnectionToClient playerConn in matchConnections[matchId])
                {
                    PlayerInfo playerInfo = playerInfos[playerConn];
                    playerInfo.ready = false;
                    playerInfo.matchId = Guid.Empty;
                    playerInfos[playerConn] = playerInfo;
                    playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Departed });
                }

                SendMatchList();
            }
        }

        [ServerCallback]
        void OnServerJoinMatch(NetworkConnectionToClient conn, Guid matchId)
        {
            if (!matchConnections.ContainsKey(matchId) || !openMatches.ContainsKey(matchId)) return;

            MatchInfo matchInfo = openMatches[matchId];
            matchInfo.players++;
            openMatches[matchId] = matchInfo;
            matchConnections[matchId].Add(conn);

            PlayerInfo playerInfo = playerInfos[conn];
            playerInfo.ready = false;
            playerInfo.matchId = matchId;
            playerInfos[conn] = playerInfo;

            PlayerInfo[] infos = matchConnections[matchId].Select(playerConn => playerInfos[playerConn]).ToArray();
            SendMatchList();

            conn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Joined, matchId = matchId, playerInfos = infos });

            foreach (NetworkConnectionToClient playerConn in matchConnections[matchId])
                playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.UpdateRoom, playerInfos = infos });
        }

        [ServerCallback]
        void OnServerLeaveMatch(NetworkConnectionToClient conn, Guid matchId)
        {
            MatchInfo matchInfo = openMatches[matchId];
            matchInfo.players--;
            openMatches[matchId] = matchInfo;

            PlayerInfo playerInfo = playerInfos[conn];
            playerInfo.ready = false;
            playerInfo.matchId = Guid.Empty;
            playerInfos[conn] = playerInfo;

            foreach (KeyValuePair<Guid, HashSet<NetworkConnectionToClient>> kvp in matchConnections)
                kvp.Value.Remove(conn);

            HashSet<NetworkConnectionToClient> connections = matchConnections[matchId];
            PlayerInfo[] infos = connections.Select(playerConn => playerInfos[playerConn]).ToArray();

            foreach (NetworkConnectionToClient playerConn in matchConnections[matchId])
                playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.UpdateRoom, playerInfos = infos });

            SendMatchList();

            conn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Departed });
        }

        [ServerCallback]
        void OnServerPlayerReady(NetworkConnectionToClient conn, Guid matchId)
        {
            PlayerInfo playerInfo = playerInfos[conn];
            playerInfo.ready = !playerInfo.ready;
            playerInfos[conn] = playerInfo;

            HashSet<NetworkConnectionToClient> connections = matchConnections[matchId];
            PlayerInfo[] infos = connections.Select(playerConn => playerInfos[playerConn]).ToArray();

            foreach (NetworkConnectionToClient playerConn in matchConnections[matchId])
                playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.UpdateRoom, playerInfos = infos });
        }

    [ServerCallback]
    void OnServerStartMatch(NetworkConnectionToClient conn,int minigame)
    {
        if (!playerMatches.ContainsKey(conn)) return;
        
        Guid matchId;
        if (playerMatches.TryGetValue(conn, out matchId))
        {
            
            GameObject matchControllerObject = Instantiate(matchControllerPrefab);
            matchControllerObject.GetComponent<NetworkMatch>().matchId = matchId;
            NetworkServer.Spawn(matchControllerObject);
            MatchController matchController = matchControllerObject.GetComponent<MatchController>();
            Debug.Log("MINIGAME "+minigame);
            matchController.round1 = minigame;
            
            foreach (NetworkConnectionToClient playerConn in matchConnections[matchId])
            {
                playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Started });
                GameObject player = Instantiate(InGamePlayer);
                player.GetComponent<NetworkMatch>().matchId = matchId;
                NetworkServer.AddPlayerForConnection(playerConn, player);
                matchController.players.Add(playerConn.identity);

                PlayerInfo playerInfo = playerInfos[playerConn];
                playerInfo.ready = false;
                playerInfos[playerConn] = playerInfo;
            }
            Debug.Log("canvas" + matchController.players.Count);
            /*if(minigame == 2) {*/
                TreeGrowMap.SetActive(true);
                foreach (GameObject obj in InGameObjects)
                {
                    GameObject objec = Instantiate(obj);
                    objec.GetComponent<NetworkMatch>().matchId = matchId;
                    NetworkServer.Spawn(objec);
                    matchController.objects.Add(objec.GetComponent<NetworkIdentity>());
                }
            /*}*/
            if (minigame == 3)
            {
                //Ahmed
            }


            playerMatches.Remove(conn);
            openMatches.Remove(matchId);
            matchConnections.Remove(matchId);
            SendMatchList();

            OnPlayerDisconnected += matchController.OnPlayerDisconnected;

        }
        else
        {
            Debug.LogError("Failed to find match ID for the player.");
        }
    }

    [ServerCallback]
    void OnServerNextRound(NetworkConnectionToClient conn, Guid matchId)
    {
        MatchController[] matchControllers =GameObject.FindObjectsOfType<MatchController>();
        conn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.NextRound , matchId=matchId });
        foreach (MatchController matchController in matchControllers) {
            if (matchController.GetComponent<NetworkMatch>().matchId == matchId) {
                int round = matchController.round1 == 2 ? 3 : 2; 
                /*if(maplist[index] == 2) {*/
                TreeGrowMap.SetActive(true);
                foreach (GameObject obj in InGameObjects)
                {
                    GameObject objec = Instantiate(obj);
                    objec.GetComponent<NetworkMatch>().matchId = matchId;
                    NetworkServer.Spawn(objec);
                    matchController.objects.Add(objec.GetComponent<NetworkIdentity>());
                }
                /*}*/
                if (round == 3)
                {
                    //Ahmed
                }

                // Perform any necessary actions for starting the next round here
                Debug.Log("Starting next round: " + round );
                StartCoroutine(Unloadscene(matchController.round1));
                StartCoroutine(loadscene(round));
            }
        }
    }

    [ServerCallback]
        internal void SendMatchList(NetworkConnectionToClient conn = null)
        {
            if (conn != null)
                conn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.List, matchInfos = openMatches.Values.ToArray() });
            else
                foreach (NetworkConnectionToClient waiter in waitingConnections)
                    waiter.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.List, matchInfos = openMatches.Values.ToArray() });
        }

        #endregion

        #region Client Match Message Handler

        [ClientCallback]
        void OnClientMatchMessage(ClientMatchMessage msg)
        {
            switch (msg.clientMatchOperation)
            {
                case ClientMatchOperation.None:
                    {
                        Debug.LogWarning("Missing ClientMatchOperation");
                        break;
                    }
                case ClientMatchOperation.List:
                    {
                        openMatches.Clear();
                        foreach (MatchInfo matchInfo in msg.matchInfos)
                            openMatches.Add(matchInfo.matchId, matchInfo);

                        RefreshMatchList();
                        break;
                    }
                case ClientMatchOperation.Created:
                    {
                        localPlayerMatch = msg.matchId;
                        ShowRoomView();
                        roomGUI.RefreshRoomPlayers(msg.playerInfos);
                        roomGUI.SetOwner(true);
                        break;
                    }
                case ClientMatchOperation.Cancelled:
                    {
                        localPlayerMatch = Guid.Empty;
                        ShowLobbyView();
                        break;
                    }
                case ClientMatchOperation.Joined:
                    {
                        localJoinedMatch = msg.matchId;
                        ShowRoomView();
                        roomGUI.RefreshRoomPlayers(msg.playerInfos);
                        roomGUI.SetOwner(false);
                        break;
                    }
                case ClientMatchOperation.Departed:
                    {
                        localJoinedMatch = Guid.Empty;
                        ShowLobbyView();
                        break;
                    }
                case ClientMatchOperation.UpdateRoom:
                    {
                        roomGUI.RefreshRoomPlayers(msg.playerInfos);
                        break;
                    }
                case ClientMatchOperation.Started:
                    {
                        MatchInfo matchInfo;
                        if (openMatches.TryGetValue(localPlayerMatch, out matchInfo))
                        {
                                
                            Debug.Log("MatchInfo retrieved successfully CLIENT OWNER: " + matchInfo.maps);
                        }
                        else if (openMatches.TryGetValue(localJoinedMatch, out matchInfo))
                        {
                                
                            Debug.Log("MatchInfo retrieved successfully CLIENT JOINED: " + matchInfo.maps);
                        }
                        else
                        {
                            Debug.Log("Match info CLIENT: " + matchInfo.maps);
                        }
                        StartCoroutine(loadscene(matchInfo.maps[0]));
                        lobbyView.SetActive(false);
                        roomView.SetActive(false);
                        break;
                    }
                case ClientMatchOperation.NextRound:
                {
                    MatchController[] matchControllers = GameObject.FindObjectsOfType<MatchController>();
                    foreach (MatchController matchController in matchControllers)
                    {
                        if (matchController.GetComponent<NetworkMatch>().matchId == msg.matchId)
                        {
                            int round = matchController.round1 == 2 ? 3 : 2;
                            /*if(maplist[index] == 2) {*/
                            TreeGrowMap.SetActive(true);
                            foreach (GameObject obj in InGameObjects)
                            {
                                GameObject objec = Instantiate(obj);
                                objec.GetComponent<NetworkMatch>().matchId = msg. matchId;
                                NetworkServer.Spawn(objec);
                                matchController.objects.Add(objec.GetComponent<NetworkIdentity>());
                            }
                            /*}*/
                            if (round == 3)
                            {
                                //Ahmed
                            }

                            // Perform any necessary actions for starting the next round here
                            Debug.Log("Starting next round: " + round);
                            StartCoroutine(Unloadscene(matchController.round1));
                            StartCoroutine(loadscene(round));
                        }
                    }
                    
                    break;
                }
        }
        }

 /*       [ClientCallback]
        void onClientSceneMsg(SceneMsg msg)
        {
            Scene loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            // Deactivate the hub
            HUB.SetActive(false);

            // Deactivate the canvas
            gameObject.SetActive(false);

            // Load the scene additively
            SceneManager.LoadScene(msg.sceneIndex, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(loadedScene);

    }*/

        [ClientCallback]
        public void ShowLobbyView()
        {
            lobbyView.SetActive(true);
            roomView.SetActive(false);
            HUB.SetActive(false);
            Debug.Log("lobbyViewActive");
            foreach (Transform child in matchList.transform)
                if (child.gameObject.GetComponent<MatchGUI>().GetMatchId() == selectedMatch)
                {
                    Toggle toggle = child.gameObject.GetComponent<Toggle>();
                    toggle.isOn = true;
                }
        }

        [ClientCallback]
        void ShowRoomView()
        {
            lobbyView.SetActive(false);
            roomView.SetActive(true);
        }

        [ClientCallback]
        void RefreshMatchList()
        {
            foreach (Transform child in matchList.transform)
                Destroy(child.gameObject);

            joinButton.interactable = false;

            foreach (MatchInfo matchInfo in openMatches.Values)
            {
                GameObject newMatch = Instantiate(matchPrefab, Vector3.zero, Quaternion.identity);
                newMatch.transform.SetParent(matchList.transform, false);
                newMatch.GetComponent<MatchGUI>().SetMatchInfo(matchInfo);

                Toggle toggle = newMatch.GetComponent<Toggle>();
                toggle.group = toggleGroup;
                if (matchInfo.matchId == selectedMatch)
                    toggle.isOn = true;
            }
        }

    #endregion

    internal IEnumerator loadscene(int index)
    {
        TreeGrowMap.SetActive(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(/*scenelist[index]*/ index, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
        yield return new WaitForSeconds(0.1f);
    }
    internal IEnumerator Unloadscene(int index)
    {
        TreeGrowMap.SetActive(true);
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(/*scenelist[index]*/ index);

        // Wait until the asynchronous scene fully loads
        while (!asyncUnload.isDone)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.1f);
    }

    internal IEnumerator TimeToWait(float time) 
    {
        yield return new WaitForSeconds(time);
    }
}
