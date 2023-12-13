using Mirror;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Snake : NetworkBehaviour
{
    public Transform segmentPrefab;
    [SyncVar] public Vector2Int direction;

    public Vector3 startPosition = Vector3.zero;
    public float speed = 20f;
    private float speedMultiplier = 1f;
    private float speedTimer = 0;
    public int initialSize = 4;
    private List<Transform> segments = new List<Transform>();
    private float nextUpdate;

    [SyncVar] public bool isPlayerDead = false;
    [SyncVar] public int score = 0;

    float lastZoneDamageTime;

    Vector3 normalScale = new Vector3(0.4f, 0.4f, 0.4f);
    Vector3 jumpScale = new Vector3(0.6f, 0.6f, 0.6f);

    #region Server

    [Command]
    public void CmdChangeDirection(Vector2Int newInput) 
    {
        if (newInput == Vector2Int.zero) return;
        direction = newInput;
    }

    [Command]
    public void CmdJump()
    {
        transform.localScale = jumpScale;
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (isPlayerDead) return;
        if (m_NetworkManager.instance.isGameOver) return;

        score = Mathf.Max(0, segments.Count - initialSize);
        
        if (Time.time < nextUpdate)
        {
            return;
        }

        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].position = segments[i - 1].position;
            segments[i].localScale = segments[i - 1].localScale;
            segments[i - 1].localScale = normalScale;
        }

        int x = Mathf.RoundToInt(transform.position.x) + direction.x;
        int y = Mathf.RoundToInt(transform.position.y) + direction.y;
        transform.position = new Vector2(x, y);

        if(speedTimer > 0)
        {
            speedTimer -= Time.deltaTime;
            speedMultiplier = 3;
        }
        else
        {
            speedMultiplier = 2;
        }

        nextUpdate = Time.time + (1f / (speed * speedMultiplier));

        //Player when colliding with border
        if (lastZoneDamageTime - (float)NetworkTime.time < 0 &&
           (transform.position.x > m_Zone.instance.circleSize.x / 2 ||
           transform.position.x < -m_Zone.instance.circleSize.x / 2 ||
           transform.position.y > m_Zone.instance.circleSize.y / 2 ||
           transform.position.y < -m_Zone.instance.circleSize.y / 2))
        {
            lastZoneDamageTime = (float)NetworkTime.time + (1.5f / m_Zone.instance.zoneCounter) ;
            GameObject g = segments[segments.Count - 1].gameObject;

            if (g == this.gameObject)
            {
                TargetGameOver();

                isPlayerDead = true;

                foreach (var segment in segments)
                {
                    segment.tag = "Untagged";
                }
            }
            else
            {
                GameObject instantiatedFood = (GameObject)Instantiate(Resources.Load("Objects/Food"), g.transform.position, Quaternion.identity);
                NetworkServer.Spawn(instantiatedFood);

                segments.RemoveAt(segments.Count - 1);
                NetworkServer.Destroy(g);
            }
        }

        //Player when colliding with wall
        if (transform.position.x > m_Zone.instance.borderSize.x / 2 ||
           transform.position.x < -m_Zone.instance.borderSize.x / 2 ||
           transform.position.y > m_Zone.instance.borderSize.y / 2 ||
           transform.position.y < -m_Zone.instance.borderSize.y / 2)
        {
            DestroySnake();
        }
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

    [Server]
    public void Grow()
    {
        Transform segment = Instantiate(segmentPrefab);
        segment.position = segments[segments.Count - 1].position;
        segments.Add(segment);

        NetworkServer.Spawn(segment.gameObject, netIdentity.connectionToClient);
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(isPlayerDead) return;
        if(m_NetworkManager.instance.isGameOver) return;

        if (other.gameObject.CompareTag("Food"))
        {
            Grow();
            NetworkServer.Destroy(other.gameObject);
        }
        if (other.gameObject.CompareTag("Chest"))
        {
            speedTimer = 3;
            NetworkServer.Destroy(other.gameObject);
        }
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            DestroySnake();
        }
    }


    [Server]
    public void DestroySnake()
    {
        if (isPlayerDead) return;
        if (m_NetworkManager.instance.isGameOver) return;

        TargetGameOver();
        isPlayerDead = true;

        foreach (var segment in segments)
        {
            segment.tag = "Untagged";
        }

        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] != this.transform)
            {
                GameObject instantiatedFood = (GameObject)Instantiate(Resources.Load("Objects/Food"), segments[i].position, Quaternion.identity);
                NetworkServer.Spawn(instantiatedFood);

                NetworkServer.Destroy(segments[i].gameObject);
            }
        }
    }


    #endregion Server

    #region Client

    [ClientCallback]
    private void Start()
    {
        if (isOwned)
        {
            gameObject.tag = "Player";
        }
    }

    [ClientCallback]
    private void Update()
    {
        if (isPlayerDead)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            return;
        }
        
        transform.rotation = Quaternion.Euler(0,0, (direction.x * -90) + direction.y) ;

        if (!isOwned) return;

        Vector2Int newInput = Vector2Int.zero;

        if (direction.x != 0f)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                newInput = Vector2Int.up;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                newInput = Vector2Int.down;
            }
        }
        else if (direction.y != 0f)
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                newInput = Vector2Int.right;
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                newInput = Vector2Int.left;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CmdJump();
        }


        if (newInput == Vector2Int.zero) return;
        CmdChangeDirection(newInput);

    }

    [TargetRpc]
    public void TargetGameWon()
    {
        m_GameManager.instance.gameWonPanel.SetActive(true);
    }

    [TargetRpc]
    public void TargetGameOver()
    {
        m_GameManager.instance.gameOverPanel.SetActive(true);
    }
    #endregion Client
}