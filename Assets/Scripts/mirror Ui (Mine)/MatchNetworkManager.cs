using System.Collections;
using UnityEngine;
using Mirror;
using System.Net;
using System.Net.Sockets;
using kcp2k;
using TMPro;
using Unity.VisualScripting;

[AddComponentMenu("")]
    public class MatchNetworkManager : NetworkManager
    {

        [Header("Match GUI")]
        public GameObject canvas;
        public CanvasController canvasController;
        public bool isServerInstance;
        public TextMeshProUGUI texttest;

    public override void Awake()
    {
        base.Awake();
        canvasController.InitializeData();

        try
        {
            StartServer();
            Debug.Log("server started");
        }
        catch (SocketException)
        {
            try
            {
                StartClient();
                Debug.Log("client started");
            }
            catch (SocketException)
            {
                texttest.text = ("COULDNT CONNECT");
            }
        }
    }

    #region Server System Callbacks

    public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            canvasController.OnServerReady(conn);
        }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            StartCoroutine(DoServerDisconnect(conn));
        }

        IEnumerator DoServerDisconnect(NetworkConnectionToClient conn)
        {
            yield return canvasController.OnServerDisconnect(conn);
            base.OnServerDisconnect(conn);
        }

        #endregion

        #region Client System Callbacks

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            canvasController.OnClientConnect();
        }
        public override void OnClientDisconnect()
        {
            canvasController.OnClientDisconnect();
            base.OnClientDisconnect();
        }

        #endregion

        #region Start & Stop Callbacks
        public override void OnStartServer()
        {
            if (mode == NetworkManagerMode.ServerOnly)
                canvas.SetActive(true);

            canvasController.OnStartServer();
        }

        public override void OnStartClient()
        {
            /*canvas.SetActive(true);*/
            canvasController.OnStartClient();
        }

        public override void OnStopServer()
        {
            canvasController.OnStopServer();
            canvas.SetActive(false);
        }
        public override void OnStopClient()
        {
            canvasController.OnStopClient();
        }

        #endregion
    }
