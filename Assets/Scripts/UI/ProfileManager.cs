using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
[System.Serializable]
public class PlayerData
{
    public string id;
    public string username;
    public string email;
    public string passwordHash;
    public int coins;
    public int level;
    public int xp;
    public string rank;

    // Define other properties of the player data object here
}

public class ProfileManager : MonoBehaviour
{
    public TextMeshProUGUI usertext;
    public TextMeshProUGUI xptext;
    public TextMeshProUGUI leveltext;
    public Canvas profilecanva;
    public Canvas mmcanva;
    // Start is called before the first frame update
    void Start()
    {
        string username = PlayerPrefs.GetString("username");
        StartCoroutine(GetPlayerByUsername(username));
    }

    // Update is called once per frame
    void Update()
    {
        
        


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
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonResult);

                // Use the player data as needed
                Debug.Log("Player found: " + playerData);
                usertext.text = playerData.username;
                xptext.text = "XP: " + playerData.xp.ToString();
                leveltext.text = "Level: " + playerData.level.ToString();
                
                // You can access other properties of the player data object here
            }
            else
            {
                Debug.LogError("Error getting player data: " + www.error);
            }
        }
    }
    public void back()
    {
        profilecanva.enabled = false;
        mmcanva.enabled = true;
    }
}
