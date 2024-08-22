using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class HubManager : MonoBehaviour
{
    public Canvas mmcanva;
    private string baseUrl= "http://localhost:9090";
    public PlayerData playerData;
    public MatchmakingEntry matchmakingEntry;
    CanvasController LobbyCanvas;

    private void Start()
    {
        LobbyCanvas=GameObject.Find("lobbyGUI").GetComponent<CanvasController>();
    }

    // Start is called before the first frame update
    public void openprofile()
    {
        /*profilecanva.enabled = true;*/
        /*mmcanva.enabled = false;*/
        SceneManager.LoadScene(4);
    }

    public void StartGame()
    {
        /*StartCoroutine(GetAllMatchmakingEntries(MatchMaker));*/
        LobbyCanvas.ShowLobbyView();

    }

    public IEnumerator AddMatchmaking(string playerId, string status, DateTime startDate, DateTime endDate)
    {
        // Prepare the request body
        Dictionary<string, string> requestBody = new Dictionary<string, string>();
        requestBody.Add("playerId", playerId);
        requestBody.Add("status", status);
        requestBody.Add("startDate", startDate.ToString());
        requestBody.Add("endDate", endDate.ToString());

        // Convert the request body to JSON
        string jsonRequestBody = JsonUtility.ToJson(requestBody);

        // Create the request URL
        string url = baseUrl + "/addMatchmaking"; // Adjust the endpoint as per your API

        // Create the request
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, jsonRequestBody))
        {
            // Set headers
            www.SetRequestHeader("Content-Type", "application/json");

            // Send the request
            yield return www.SendWebRequest();

            // Check for errors
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Matchmaking added successfully!");
            }
        }
    }

    // Function to retrieve all matchmaking entries
    public IEnumerator GetAllMatchmakingEntries(Action<List<MatchmakingEntry>> callback)
    {
        string url = baseUrl + "/matchmakings"; // Adjust the endpoint as per your API

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                callback(null); // Notify caller with null data
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                // Parse JSON response
                List<MatchmakingEntry> entries = JsonUtility.FromJson<List<MatchmakingEntry>>(jsonResponse);
                callback(entries); // Pass data to callback function
            }
        }
    }

    private void MatchMaker(List<MatchmakingEntry> entries)
    {
        GetPlayerByUsername(PlayerPrefs.GetString("username"));
        if (entries != null && playerData !=null )
        {
            bool idAdded = false;

            foreach (var entry in entries)
            {
                Debug.Log("Status: " + entry.status + ", StartDate: " + entry.startDate + ", EndDate: " + entry.endDate);

                if (entry.status == "queued" && !idAdded)
                {
                    if (!entry.playerIds.Contains(playerData.id))
                    {
                        entry.playerIds.Add(playerData.id);
                        matchmakingEntry = entry;
                        idAdded = true; // Mark the ID as added
                    }
                }
            }

            if (!idAdded)
            {
                Debug.Log("ID was not added to any entry.");
                StartCoroutine(AddMatchmaking(playerData.id,"queued", DateTime.Now, DateTime.Now.AddSeconds(1500)));
            }
        }
        else
        {
            Debug.LogError("Failed to retrieve matchmaking entries.");
        }
    }

    public async Task<string> UpdateMatchmaking(string matchmakingId, Dictionary<string, string> updateData)
    {
        string url = baseUrl + "/matchmakings/" + matchmakingId; // Adjust the endpoint as per your API

        string jsonUpdateData = JsonUtility.ToJson(updateData);

        using (UnityWebRequest www = UnityWebRequest.Put(url, jsonUpdateData))
        {
            // Set headers
            www.SetRequestHeader("Content-Type", "application/json");

            // Send the request asynchronously
            var asyncOperation = www.SendWebRequest();

            // Wait until the request is completed
            while (!asyncOperation.isDone)
            {
                await Task.Delay(100); // Adjust delay as needed
            }

            // Check for errors
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                return null;
            }
            else
            {
                return www.downloadHandler.text;
            }
        }
    }

    IEnumerator GetPlayerByUsername(string username)
    {
        string url = "http://localhost:9090/players/username/" + username;

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Parse the JSON response and extract player data
                string jsonResult = www.downloadHandler.text;
                playerData = JsonUtility.FromJson<PlayerData>(jsonResult);

                // Use the player data as needed
                Debug.Log("Player found: " + playerData);
                // You can access other properties of the player data object here
            }
            else
            {
                Debug.LogError("Error getting player data: " + www.error);
            }
        }
    }

    [System.Serializable]
    public class MatchmakingEntry
    {
        public List<string> playerIds;
        public string status;
        public DateTime startDate;
        public DateTime endDate;
    }



}
