using System.Collections.Generic;
using UnityEngine;

using anyID = System.UInt16;
using uint64 = System.UInt64;

public class TSClient3DManager : TSClient3DManagerBase {
    private Dictionary<anyID, GameObject> gameObjects;

    private void CreateObject(anyID clientID)
    {
        GameObject newGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newGameObject.transform.position = new Vector3(0, 0.5F, 0);

        /*TSObject tsObject =*/ TSObject.Create(newGameObject, 0, clientID);
        gameObjects.Add(clientID, newGameObject);
        Debug.Log("Created object " + clientID);
    }

    private void RemoveObject(anyID clientID)
    {
        GameObject targetGameObject;
        if (gameObjects.TryGetValue(clientID, out targetGameObject))
            Destroy(targetGameObject);

        gameObjects.Remove(clientID);
    }

    private void RemoveObjects()
    {
        foreach (var targetObject in gameObjects.Values)
            Destroy(targetObject);

        gameObjects.Clear();
    }

    protected override void OnClientMoveCommonMainThread(uint64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID, int visibility)
    {
        anyID myID = ts_client.GetClientID();
        if (visibility == (int)Visibility.LEAVE_VISIBILITY)
        {
            if (myID == clientID)
            {
                RemoveObjects();
                Debug.Log("bye bye");
            }
            else
            {
                RemoveObject(clientID);
                Debug.Log("bye bye client: " + clientID.ToString());
            }
        }
        else
        {
            if (myID == clientID)
            {
                RemoveObjects();
                if (newChannelID != 0)
                {
                    List<anyID> newClients = ts_client.GetChannelClientList(serverConnectionHandlerID, newChannelID);
                    foreach (anyID client in newClients)
                    {
                        if (client != myID)
                        {
                            CreateObject(client);
                        }
                    }
                }
            }
            else
            {
                // limit to enter own channel
                uint64 my_channel = ts_client.GetChannelOfClient(myID);
                if (newChannelID == my_channel)
                {
                    CreateObject(clientID);
                }
                else if (oldChannelID == my_channel)
                {
                    RemoveObject(clientID);
                }
            }
        }
    }

    // Use this for initialization
    protected override void onStart() {
        gameObjects = new Dictionary<anyID, GameObject>();
        
        // Attach our self position to the camera
        GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        cameraObject.AddComponent<SilenceTsMix>();
    }
}
