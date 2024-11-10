using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class ObjectDataTracker : MonoBehaviour
{
    public GameObject rightbody;          // The GameObject to track
    public string objectInfo;             // Context info about the object (e.g., "ball", "sword", etc.)
    public string holderInfo;             // Information about who holds the object, updated as needed

    private List<ObjectData> recordedData;  // List to store data for JSON output
    private float recordInterval = 1f;      // Interval in seconds to record data
    public float saveInterval = 7f;         // Interval in seconds to save data to JSON

    private void Start()
    {
        recordedData = new List<ObjectData>();
        StartCoroutine(RecordData());
        StartCoroutine(SaveDataToJson());
    }

    private IEnumerator RecordData()
    {
        while (true)
        {
            yield return new WaitForSeconds(recordInterval);

            if (rightbody != null)
            {
                Vector3 position = rightbody.transform.position;
                Vector3 velocity = rightbody.GetComponent<Rigidbody>().velocity; // Requires a Rigidbody for velocity

                ObjectData data = new ObjectData
                {
                    timestamp = Mathf.Round(Time.time * 100f) / 100f, // Rounding timestamp to .2f as well
                    position = new SerializableVector3(position),
                    velocity = new SerializableVector3(velocity),
                    context = new ObjectContext
                    {
                        objectName = objectInfo,
                        holder = holderInfo
                    }
                };

                recordedData.Add(data);
                Debug.Log("Data recorded: " + JsonUtility.ToJson(data));
            }
        }
    }

    private IEnumerator SaveDataToJson()
    {
        while (true)
        {
            yield return new WaitForSeconds(saveInterval);

            if (recordedData.Count > 0)
            {
                string jsonData = JsonConvert.SerializeObject(recordedData, Formatting.Indented);

                // Save JSON file
                string filePath = Path.Combine(Application.persistentDataPath, $"ObjectData_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json");
                File.WriteAllText(filePath, jsonData);

                Debug.Log($"Data saved to: {filePath}");

                // Clear the recorded data after saving
                recordedData.Clear();
            }
        }
    }
}

[System.Serializable]
public class ObjectData
{
    public float timestamp;
    public SerializableVector3 position;
    public SerializableVector3 velocity;
    public ObjectContext context;
}

[System.Serializable]
public class ObjectContext
{
    public string objectName; // e.g., "ball", "sword", "book", etc.
    public string holder;     // e.g., "Player1", "AICharacter", etc.
}

[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(Vector3 vector)
    {
        x = Mathf.Round(vector.x * 100f) / 100f;
        y = Mathf.Round(vector.y * 100f) / 100f;
        z = Mathf.Round(vector.z * 100f) / 100f;
    }
}
