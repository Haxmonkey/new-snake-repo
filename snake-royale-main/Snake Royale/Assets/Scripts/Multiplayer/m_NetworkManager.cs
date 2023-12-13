using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

public class m_NetworkManager : NetworkManager
{
    public static m_NetworkManager instance;

    public List<m_RoomPlayer> RoomPlayers = new List<m_RoomPlayer>();
    public bool isGameStarted = false;
    public bool isGameOver = false;

    [Range(1,40)]
    public int maxFood =10;

    public override void Start()
    {
        base.Start();
        instance = this;
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        if (isGameStarted)
        {
            conn.Disconnect();
        }

    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        if (conn.identity.TryGetComponent(out m_RoomPlayer player))
        {
            RoomPlayers.Add(player);
        }

        if(numPlayers >= maxConnections && !isGameStarted)
        {
            isGameStarted = true;
            StartGame();
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity.TryGetComponent(out m_RoomPlayer player))
        {
            RoomPlayers.Remove(player);
        }

        base.OnServerDisconnect(conn);

    }

    [Server]
    public void StartGame()
    {
        isGameStarted = true;
        ServerChangeScene(Scene.GameScene);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == Scene.GameScene)
        {
            foreach (m_RoomPlayer player in RoomPlayers)
            {
                Vector3 getStartPosition = GetStartPosition().position;
                GameObject instantiatedPlayer = (GameObject)Instantiate(Resources.Load("Players/Snake"), getStartPosition, Quaternion.identity);
                NetworkServer.Spawn(instantiatedPlayer, player.connectionToClient);

                instantiatedPlayer.GetComponent<Snake>().ResetState(getStartPosition);
            }

            for (int i = 0; i < maxFood; i++)
            {
                Vector3 getStartPosition = GetStartPosition().position;
                getStartPosition = new Vector3(Mathf.RoundToInt(getStartPosition.x), Mathf.RoundToInt(getStartPosition.y), Mathf.RoundToInt(getStartPosition.z));
                GameObject instantiatedFood = (GameObject)Instantiate(Resources.Load("Objects/Apple"), getStartPosition, Quaternion.identity);
                NetworkServer.Spawn(instantiatedFood);
            }

            for (int i = 0; i < 10; i++)
            {
                Vector3 getStartPosition = GetStartPosition().position;
                getStartPosition = new Vector3(Mathf.RoundToInt(getStartPosition.x) + 3, Mathf.RoundToInt(getStartPosition.y) - 3, Mathf.RoundToInt(getStartPosition.z));
                GameObject instantiatedChest = (GameObject)Instantiate(Resources.Load("Objects/Chest"), getStartPosition, Quaternion.identity);
                NetworkServer.Spawn(instantiatedChest);
            }

            int botsNeeded = 50 - numPlayers;

            for (int i = 0;i < botsNeeded; i++)
            {
                Vector3 getStartPosition = GetStartPosition().position;
                GameObject instantiatedPlayer = (GameObject)Instantiate(Resources.Load("Bots/Snake"), getStartPosition, Quaternion.identity);
                NetworkServer.Spawn(instantiatedPlayer);

                instantiatedPlayer.GetComponent<SnakeBot>().ResetState(getStartPosition);
            }
        }
    }

}
