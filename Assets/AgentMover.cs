using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class AgentMover : MonoBehaviour
{
    public NavMeshAgent agent;           // The NavMeshAgent component attached to the agent
    public GameObject destinationObject;  // The GameObject the agent will move toward
    public Button moveButton;             // UI Button to trigger the move action

    void Start()
    {
        // Add a listener to the button to call GoToDestination when pressed
        if (moveButton != null)
        {
            moveButton.onClick.AddListener(GoToDestination);
        }
    }

    // Method to make the agent move to the destination
    public void GoToDestination()
    {
        if (agent != null && destinationObject != null)
        {
            Vector3 destination = destinationObject.transform.position;
            agent.SetDestination(destination);
            Debug.Log("Agent is moving to destination.");
        }
        else
        {
            Debug.LogWarning("Agent or Destination object not set.");
        }
    }

    void OnDestroy()
    {
        // Remove the listener when this object is destroyed
        if (moveButton != null)
        {
            moveButton.onClick.RemoveListener(GoToDestination);
        }
    }
}
