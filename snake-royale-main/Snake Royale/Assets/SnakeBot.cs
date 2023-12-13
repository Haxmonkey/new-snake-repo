using Mirror;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SnakeBot : NetworkBehaviour
{
    public Transform segmentPrefab;
    [SyncVar] public Vector2Int direction;

    public Vector3 startPosition = Vector3.zero;
    public float speed = 20f;
    public float speedMultiplier = 1f;
    public int initialSize = 4;
    private List<Transform> segments = new List<Transform>();
    private float nextUpdate;

    [SyncVar] public bool isPlayerDead = false;

    #region Server

    [ServerCallback]
    private void FixedUpdate()
    {
        if (isPlayerDead) return;

        if (Time.time < nextUpdate)
        {
            return;
        }

        int randomNumber = Random.Range(0, 10);

        switch (randomNumber)
        {
            case 0:
                if (direction.x != 0f)
                {
                    direction = Vector2Int.up;
                }
                break;
            case 1:
                if (direction.x != 0f)
                {
                    direction = Vector2Int.down;
                }
                break;
            case 2:
                if (direction.y != 0f)
                {
                    direction = Vector2Int.left;
                }
                break;
            case 3:
                if (direction.y != 0f)
                {
                    direction = Vector2Int.right;
                }
                break;
        }

        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].position = segments[i - 1].position;
        }

        int x = Mathf.RoundToInt(transform.position.x) + direction.x;
        int y = Mathf.RoundToInt(transform.position.y) + direction.y;
        transform.position = new Vector2(x, y);

        nextUpdate = Time.time + (1f / (speed * speedMultiplier));
    }

    [Server]
    public void Grow()
    {
        Transform segment = Instantiate(segmentPrefab);
        segment.position = segments[segments.Count - 1].position;
        segments.Add(segment);

        NetworkServer.Spawn(segment.gameObject, netIdentity.connectionToClient);
    }

    [Server]
    public void ResetState(Vector3 startPosition)
    {
        direction = Vector2Int.right;
        transform.position = startPosition;

        for (int i = 1; i < segments.Count; i++)
        {
            Destroy(segments[i].gameObject);
        }

        segments.Clear();
        segments.Add(transform);

        for (int i = 0; i < initialSize - 1; i++)
        {
            Grow();
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerDead) return;
        if (m_NetworkManager.instance.isGameOver) return;

        if (other.gameObject.CompareTag("Food"))
        {
            Grow();
            NetworkServer.Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Obstacle") || other.gameObject.CompareTag("Wall"))
        {
            isPlayerDead = true;

            for (int i = 0; i < segments.Count; i++)
            {
                GameObject instantiatedFood = (GameObject)Instantiate(Resources.Load("Objects/Food"), segments[i].position, Quaternion.identity);
                NetworkServer.Spawn(instantiatedFood);

                NetworkServer.Destroy(segments[i].gameObject);
            }
        }
    }
    #endregion Server

}