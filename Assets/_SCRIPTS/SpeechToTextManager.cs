using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HuggingFace.API;
using UnityEngine.InputSystem;

public class GetAudioInput : MonoBehaviour
{
    private bool isRecording = false;
    private AudioClip clip;
    private byte[] bytes;
    private bool recording;

    public Button startButton;
    public TextMeshProUGUI text;

    public string[] targetWords; // Define your target words here
    public GameObject[] objectPrefab; // Prefabs to spawn for each target word
    public Transform[] spawnPoint; // Spawn locations for the objects
    public GameObject x; // Parent object for spawned items

    //private bool recording = false;
    //private AudioClip clip;
    //private byte[] bytes;

    private void Start()
    {
        Debug.Log("AudioLog: Starting setup.");
        recording = false;
    }

    private void Update()
    {
        // Start recording when the space key is pressed (new Input System)
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !recording)
        {
            StartRecording();
        }

        // Stop recording once the audio clip reaches the desired length
        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        Debug.Log("AudioLog: Starting recording.");
        clip = Microphone.Start(null, false, 5, 44100);
        recording = true;
    }

    private void StopRecording()
    {
        Debug.Log("AudioLog: Stopping recording.");
        recording = false;

        int position = Microphone.GetPosition(null);
        Microphone.End(null);

        float[] samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);

        SendRecording();
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }

    private void SendRecording()
    {
        Debug.Log("AudioLog: Sending recording for transcription.");

        // Sending audio bytes to Hugging Face API for transcription
        HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response =>
        {
            Debug.Log("AudioLog: Transcription received - " + response);
            response = response.ToLower();

            string firstFoundWord = FindTargetWord(response);
            Debug.Log("AudioLog: Detected word - " + firstFoundWord);

            int index = Array.IndexOf(targetWords, firstFoundWord);
            Debug.Log("AudioLog: Word index - " + index);

            if (index >= 0 && index < objectPrefab.Length)
            {
                LoadObject(index);
            }
        }, error =>
        {
            Debug.LogError("AudioLog: Error in transcription - " + error);
        });
    }

    private string FindTargetWord(string sentence)
    {
        string[] wordsInSentence = sentence.Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in wordsInSentence)
        {
            if (targetWords.Any(targetWord => string.Equals(word, targetWord, StringComparison.OrdinalIgnoreCase)))
            {
                return word;
            }
        }
        return null;
    }

    public void LoadObject(int index)
    {
        if (objectPrefab[index] != null && spawnPoint[index] != null)
        {
            Debug.Log("AudioLog: Spawning object - " + objectPrefab[index].name);
            GameObject instantiatedObject = Instantiate(objectPrefab[index], spawnPoint[index].position, Quaternion.identity);
            instantiatedObject.transform.SetParent(x.transform);
            instantiatedObject.transform.Rotate(-90f, 0f, 0f);
        }
    }
}
