using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public string loginUrl = "http://localhost:9090/players/login"; // Update this with your server's login endpoint
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Canvas signUpcanvas;
    public Canvas signIncanvas;
    public string serverAddress = "localhost"; // Address of the server to connect to
    public int serverPort = 7777; // Port number of the server
    public void Login()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        StartCoroutine(SendLoginRequest(username, password));
    }

    public void GoSignUp()
    {
        signUpcanvas.GetComponent<Canvas>().enabled = true;
        signIncanvas.GetComponent<Canvas>().enabled = false;
    }

    IEnumerator SendLoginRequest(string username, string password)
    {
        // Create JSON data with username and password
        string jsonData = "{\"username\":\"" + username + "\",\"passwordHash\":\"" + password + "\"}";

        // Create a POST request
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(loginUrl, "POST"))
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
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                // Parse the response
                string jsonResponse = request.downloadHandler.text;
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save();
                SceneManager.LoadScene(1);

                // Extract token from response and store it locally
                // Handle subsequent requests with the token
            }
        }
    }
}
