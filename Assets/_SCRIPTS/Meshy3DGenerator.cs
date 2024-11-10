using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Meshy3DGenerator : MonoBehaviour
{
    private string apiKey = "msy_m8UCjkLnjBin720GAK9nnqGNRED6OtjNhqLP";  // Replace with your Meshy.ai API Key
    private string endpointUrl = "https://api.meshy.ai/v1/text-to-3d";  // Example endpoint; confirm with Meshy.ai documentation

    // Function to request a 3D model generation from text input
    public void Generate3DModelFromText(string prompt)
    {
        StartCoroutine(Request3DModel(prompt));
    }

    private IEnumerator Request3DModel(string prompt)
    {
        // Create a JSON payload for the request
        string jsonPayload = JsonUtility.ToJson(new { prompt = prompt });

        // Create a POST request
        UnityWebRequest request = new UnityWebRequest(endpointUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // Send the request and wait for the response
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Parse the response, which may contain a URL or path to the generated 3D model file
            string responseText = request.downloadHandler.text;
            Debug.Log("Meshy.ai Response: " + responseText);
            // Process response to load or download the 3D model into Unity
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }
}
