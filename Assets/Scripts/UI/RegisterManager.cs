using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class RegistrationManager : MonoBehaviour
{
    public string registrationUrl = "http://localhost:9090/players"; // Update this with your server's registration endpoint
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI statusText;
    public Canvas signUpcanvas;
    public Canvas signIncanvas;
    public void Register()
    {
        string username = usernameInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;
        int coins = 0;
        int level = 0;
        int xp = 0;
        string rank = "beginner";

        StartCoroutine(SendRegistrationRequest(username, email, password, coins, level, xp, rank));
    }
    public void GoSignIn()
    {
        signUpcanvas.GetComponent<Canvas>().enabled = false;
        signIncanvas.GetComponent<Canvas>().enabled = true;
    }
    IEnumerator SendRegistrationRequest(string username, string email, string password, int coins, int level, int xp, string rank)
    {
        // Create JSON data with user information
        string jsonData = "{\"username\":\"" + username + "\",\"email\":\"" + email + "\",\"passwordHash\":\"" + password + "\",\"coins\":" + coins + ",\"level\":" + level + ",\"xp\":" + xp + ",\"rank\":\"" + rank + "\"}";

        // Create a POST request
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(registrationUrl, "POST"))
        {
            // Set the content type
            request.SetRequestHeader("Content-Type", "application/json");

            // Attach JSON data to request body
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Send the request
            yield return request.SendWebRequest();

            // Check for errors
            if (request.result != UnityWebRequest.Result.Success)
            {
                statusText.text = "Error: " + request.error;
            }
            else
            {
                // Registration successful
                statusText.text = "Registration successful!";
            }
        }
    }
}
