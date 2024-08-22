using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;
using System;

[RequireComponent(typeof(NetworkMatch))]
    public class MatchController : NetworkBehaviour
    {
        public readonly SyncDictionary<NetworkIdentity, MatchPlayerData> matchPlayerData = new SyncDictionary<NetworkIdentity, MatchPlayerData>();
        public readonly SyncDictionary<NetworkIdentity, MatchObjectData> matchObjectData = new SyncDictionary<NetworkIdentity, MatchObjectData>();

        //bool playAgain = false;
        [SyncVar] public int round1; 
        [Header("Diagnostics")]
        [ReadOnly, SerializeField] internal CanvasController canvasController;
        [ReadOnly, SerializeField,SyncVar] public List<NetworkIdentity> players;
        [ReadOnly, SerializeField,SyncVar] internal List<NetworkIdentity> objects;


        void Awake()
        {
#if UNITY_2022_2_OR_NEWER
            canvasController = GameObject.FindAnyObjectByType<CanvasController>();
#else
            // Deprecated in Unity 2023.1
            canvasController = GameObject.FindObjectOfType<CanvasController>();
#endif
        }

        public override void OnStartServer()
        {
            StartCoroutine(AddPlayersToMatchController());
            StartCoroutine(AddObjectToMatchController());
        }

        // For the SyncDictionary to properly fire the update callback, we must
        // wait a frame before adding the players to the already spawned MatchController
        IEnumerator AddPlayersToMatchController()
        {
            if(players != null) 
            {
                yield return null;
                foreach (var player in players)
                {
                    matchPlayerData.Add(player, new MatchPlayerData { playerIndex = CanvasController.playerInfos[player.connectionToClient].playerIndex });
                }
            }
            else
                Debug.Log("NO Players(MatchController)");

        Debug.Log("ienm add"+players.Count);

        }
        IEnumerator AddObjectToMatchController()
        {
            yield return null;
            foreach (var objec in objects)
            {
                matchObjectData.Add(objec, new MatchObjectData { objectIndex = CanvasController.objectInfos[objec.connectionToClient].objectIndex });
            }
            
        }
        

 /*       // Assigned in inspector to ReplayButton::OnClick
        [ClientCallback]
        public void RequestPlayAgain()
        {
            playAgainButton.gameObject.SetActive(false);
            CmdPlayAgain();
        }*/

/*        [Command(requiresAuthority = false)]
        public void CmdPlayAgain(NetworkConnectionToClient sender = null)
        {
            if (!playAgain)
                playAgain = true;
            else
            {
                playAgain = false;
                RestartGame();
            }
        }*/

/*        [ServerCallback]
        public void RestartGame()
        {
            foreach (CellGUI cellGUI in MatchCells.Values)
                cellGUI.SetPlayer(null);

            boardScore = CellValue.None;

            NetworkIdentity[] keys = new NetworkIdentity[matchPlayerData.Keys.Count];
            matchPlayerData.Keys.CopyTo(keys, 0);

            foreach (NetworkIdentity identity in keys)
            {
                MatchPlayerData mpd = matchPlayerData[identity];
                mpd.currentScore = CellValue.None;
                matchPlayerData[identity] = mpd;
            }

            RpcRestartGame();

            startingPlayer = startingPlayer == player1 ? player2 : player1;
            currentPlayer = startingPlayer;
        }*/

/*        [ClientRpc]
        public void RpcRestartGame()
        {
            foreach (CellGUI cellGUI in MatchCells.Values)
                cellGUI.SetPlayer(null);

            exitButton.gameObject.SetActive(false);
            playAgainButton.gameObject.SetActive(false);
        }*/

/*        // Assigned in inspector to BackButton::OnClick
        [Client]
        public void RequestExitGame()
        {
            exitButton.gameObject.SetActive(false);
            playAgainButton.gameObject.SetActive(false);
            CmdRequestExitGame();
        }*/

/*        [Command(requiresAuthority = false)]
        public void CmdRequestExitGame(NetworkConnectionToClient sender = null)
        {
            StartCoroutine(ServerEndMatch(sender, false));
        }*/

        [ServerCallback]
        public void OnPlayerDisconnected(NetworkConnectionToClient conn)
        {
                foreach (var player in players)
            {
                // Check that the disconnecting client is a player in this match
                if (player == conn.identity)
                    StartCoroutine(ServerEndMatch(conn, true));
            }
        }

        [ServerCallback]
        public IEnumerator ServerEndMatch(NetworkConnectionToClient conn, bool disconnected)
        {
            RpcExitGame();

            canvasController.OnPlayerDisconnected -= OnPlayerDisconnected;

            // Wait for the ClientRpc to get out ahead of object destruction
            yield return new WaitForSeconds(0.1f);

        foreach (var player in players)
        {
            if (!disconnected)
            {
                NetworkServer.RemovePlayerForConnection(player.connectionToClient, true);
                CanvasController.waitingConnections.Add(player.connectionToClient);
            }
        }



        // Skip a frame to allow the Removal(s) to complete
        yield return null;

            // Send latest match list
            canvasController.SendMatchList();
            NetworkServer.Destroy(gameObject);
        }

        [ServerCallback]
        public IEnumerator Serverlostmatch(NetworkConnectionToClient conn, bool disconnected)
        {
            RpcExitGame();

            canvasController.OnPlayerDisconnected -= OnPlayerDisconnected;

            // Wait for the ClientRpc to get out ahead of object destruction
            yield return new WaitForSeconds(0.1f);

        foreach (var player in players)
        {
            if (!disconnected)
            {
                NetworkServer.RemovePlayerForConnection(player.connectionToClient, true);
                CanvasController.waitingConnections.Add(player.connectionToClient);
            }
        }



        // Skip a frame to allow the Removal(s) to complete
        yield return null;

            // Send latest match list
            canvasController.SendMatchList();
            NetworkServer.Destroy(gameObject);
        }

        [ClientRpc]
        public void RpcExitGame()
        {
            canvasController.RequestLeaveMatch();
        }

        /*[ClientRpc]
        public void RpcLostGame(Guid matchid)
        {
            canvasController.Requestlost(matchid);
        }*/

        [ServerCallback]
        public void ServerEndRound()
        {
            canvasController.RequestNextRound(gameObject.GetComponent<NetworkMatch>().matchId);
            Debug.Log("ServerEndRound coroutine completed.");
        }
        
    }


