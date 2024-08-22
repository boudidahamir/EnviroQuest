using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{

    [SyncVar] public float time;
    public TextMeshProUGUI timer;
    public TextMeshProUGUI winlose;
    [SyncVar] public GameObject tree1;
    [SyncVar] public GameObject tree2;
    public GameObject[] players=new GameObject[100];
    [SyncVar] public int minutes;
    [SyncVar] public int seconds;
    [SyncVar] public MatchController match;
    [SyncVar] public int winner;

    // Start is called before the first frame update
    private void Awake()
    {
        winner = -1;
        time = 20;
    }
    void Start()
    {
        match = GameObject.Find("MatchController(Clone)").GetComponent<MatchController>();
        int i = 0;
        Debug.Log(match.matchPlayerData.Count);
        int teamNumber = 1;
        foreach (var kvp in match.matchPlayerData)
        {
            NetworkIdentity playerIdentity = kvp.Key;
            MatchPlayerData playerData = kvp.Value;

            playerData.team = teamNumber;
            pickupmechanic pickupMechanics = playerIdentity.gameObject.GetComponent<pickupmechanic>();
            if (pickupMechanics != null)
            {
                pickupMechanics.team = playerData.team;
            }


            teamNumber = teamNumber == 1 ? 2 : 1;

            i++;
        }

        timer =GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        winlose = GameObject.Find("winlose").GetComponent<TextMeshProUGUI>();
        initializeUI();
        treeGrow[] trees = GameObject.FindObjectsOfType<treeGrow>();
        
        foreach (treeGrow t in trees)
        {
            if(t.GetComponent<NetworkMatch>().matchId==gameObject.GetComponent<NetworkMatch>().matchId)
            { 
                if(t.gameObject.name.Equals("TreeGrow1(Clone)"))
                { tree1=t.gameObject; }
                if (t.gameObject.name.Equals("TreeGrow2(Clone)"))
                { tree2 = t.gameObject; }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

            if (tree1.GetComponent<treeGrow>().total == 16)
            {
                winner=1;
                winlose.text = "team1 wins";
                timer.GetComponent<TextMeshProUGUI>().enabled = false;
                disableTimerClient();
                UpdateStatsClient("team1 wins");
            }
            else if (tree2.GetComponent<treeGrow>().total == 16)
            {
                winner=2;
                winlose.text = "team2 wins";
                timer.GetComponent<TextMeshProUGUI>().enabled = false;
                disableTimerClient();
                UpdateStatsClient("team2 wins");
            }
            else
            {
                /*if (endgame == false) { return; }*/
                time -= Time.deltaTime;
                minutes = Mathf.FloorToInt(time / 60);
                seconds = Mathf.FloorToInt(time % 60);
                timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                UpdateTimerClient(minutes, seconds);
                UpdateTimerServer(minutes, seconds);
                if (time <= 0)
                {
                    timer.GetComponent<TextMeshProUGUI>().enabled = false;
                    disableTimerClient();
                    if (tree1.GetComponent<treeGrow>().total > tree2.GetComponent<treeGrow>().total)
                    {
                        winner = 1;
                        winlose.text = "team1 wins";
                        UpdateStatsClient("team1 wins");
                    }
                    if (tree1.GetComponent<treeGrow>().total < tree2.GetComponent<treeGrow>().total)
                    {
                        winner = 2;
                        winlose.text = "team2 wins";
                        UpdateStatsClient("team2 wins");
                    }
                    if (tree1.GetComponent<treeGrow>().total == tree2.GetComponent<treeGrow>().total)
                    {
                        winner=0;
                        winlose.text = "draw";
                        UpdateStatsClient("draw");
                    }

                }
            }

            Debug.Log("round1" + match.round1);

            if (match.round1==2)
            {
                Debug.Log("Else 1");
                switch(winner)
                {
                    case 0:
                        {
                            Debug.Log("case 0");
                            Debug.Log(match.matchObjectData);
                            foreach (var kvp in match.matchObjectData)
                            {
                                
                                NetworkIdentity objectIdentity = kvp.Key;
                                MatchObjectData objectData = kvp.Value;

                                // Assuming you have a despawn method in your InGameController to handle despawning objects
                                NetworkServer.Destroy(objectIdentity.gameObject);

                                // Optionally, you may want to remove the object's data from the dictionary after despawning
                                Debug.Log(match.matchObjectData.Remove(objectIdentity));
                                match.matchObjectData.Remove(objectIdentity);
                                
                            }
                            match.ServerEndRound();
                            break;
                        }
                    case 1:
                        {
                        Debug.Log("case 1");
                        foreach (var kvp in match.matchObjectData)
                        {
                            NetworkIdentity objectIdentity = kvp.Key;
                            MatchObjectData objectData = kvp.Value;

                            // Assuming you have a despawn method in your InGameController to handle despawning objects
                            NetworkServer.Destroy(objectIdentity.gameObject);

                            // Optionally, you may want to remove the object's data from the dictionary after despawning
                            match.matchObjectData.Remove(objectIdentity);
                        }
                        /*foreach (var kvp in match.matchPlayerData)
                        {
                            NetworkIdentity playerIdentity = kvp.Key;
                            MatchPlayerData playerData = kvp.Value;

                            if (playerData.team == 2)
                            {
                                match.matchPlayerData.Remove(playerIdentity);
                                match.RpcLostGame(playerIdentity.connectionToClient);
                            }

                        }*/
                        match.ServerEndRound();
                        break;
                        }
                    case 2:
                        {
                            Debug.Log("case 1");
                            foreach (var kvp in match.matchObjectData)
                            {
                                NetworkIdentity objectIdentity = kvp.Key;
                                MatchObjectData objectData = kvp.Value;

                                // Assuming you have a despawn method in your InGameController to handle despawning objects
                                NetworkServer.Destroy(objectIdentity.gameObject);

                                // Optionally, you may want to remove the object's data from the dictionary after despawning
                                match.matchObjectData.Remove(objectIdentity);
                            }
                            /*foreach (var kvp in match.matchPlayerData)
                            {
                                NetworkIdentity playerIdentity = kvp.Key;
                                MatchPlayerData playerData = kvp.Value;

                                if (playerData.team == 1)
                                {
                                    match.matchPlayerData.Remove(playerIdentity);
                                    match.ServerEndMatch(playerIdentity.connectionToClient, true);
                                }

                            }*/
                            break;
                        }
                }
            }
            else
            {
                Debug.Log("Else 2");
                /*foreach (var kvp in match.matchPlayerData)
                {
                NetworkIdentity playerIdentity = kvp.Key;
                MatchPlayerData playerData = kvp.Value;

                    if (playerData.team == 2)
                    {
                        match.matchPlayerData.Remove(playerIdentity);
                        match.RpcExitGame();
                        match.ServerEndMatch(playerIdentity.connectionToClient,true);
                    }

                }*/
                match.ServerEndRound();
            }
    }

    [ServerCallback]
    void UpdateTimerServer(int minutes, int seconds)
    {
        minutes = Mathf.FloorToInt(time / 60);
        seconds = Mathf.FloorToInt(time % 60);
        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    [ClientCallback]
    void UpdateTimerClient(int minutes, int seconds)
    {
        timer = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        winlose = GameObject.Find("winlose").GetComponent<TextMeshProUGUI>();
        treeGrow[] trees = GameObject.FindObjectsOfType<treeGrow>();

        foreach (treeGrow t in trees)
        {
            if (t.GetComponent<NetworkMatch>().matchId == gameObject.GetComponent<NetworkMatch>().matchId)
            {
                if (t.gameObject.name.Equals("TreeGrow1(Clone)"))
                { tree1 = t.gameObject; }
                if (t.gameObject.name.Equals("TreeGrow2(Clone)"))
                { tree2 = t.gameObject; }
            }
        }
        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    [ClientCallback]
    void UpdateStatsClient(string text)
    {
        winlose.text = text;
    }

    [ClientCallback]
    void disableTimerClient()
    {
        timer.GetComponent<TextMeshProUGUI>().enabled = false;
    }

    [ClientCallback]

    private void initializeUI()
    {
        timer = GameObject.Find("Timer").GetComponent<TextMeshProUGUI>();
        winlose = GameObject.Find("winlose").GetComponent<TextMeshProUGUI>();
    }
    internal IEnumerator TimeToWait()
    {
        yield return null;
    }
}
