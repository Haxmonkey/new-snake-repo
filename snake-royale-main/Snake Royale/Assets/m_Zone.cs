using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class m_Zone : NetworkBehaviour
{
    public static m_Zone instance;

    public SpriteRenderer targetCircleTransform;

    public Transform wallLeft;
    public Transform wallRight;
    public Transform wallUp;
    public Transform wallDown;

    float circleSpeed = 5f;
    public float shrinkTimer;

    public Vector3 borderSize = new Vector3(500f,500f);

    [SyncVar] public Vector3 circleSize;
    [SyncVar] public Vector3 targetCircleSize;

    bool isZoneEnd;
    public int zoneCounter;

    private void Awake()
    {
        instance = this;
    }

    [ServerCallback]
    private void Start()
    {
        circleSize = borderSize;
        GenerateTargetCircle();
    }

    [ServerCallback]
    private void Update()
    {
        shrinkTimer -= Time.deltaTime;

        if (shrinkTimer < 0)
        {
            Vector3 sizeChangeVector = (targetCircleSize - circleSize).normalized;
            Vector3 newCircleSize = circleSize + sizeChangeVector * Time.deltaTime * circleSpeed;

            circleSize = newCircleSize;

            if(Vector3.Distance(newCircleSize,targetCircleSize) < 0.1f)
            {
                GenerateTargetCircle();
            }
        }
    }

    [ClientCallback]
    private void LateUpdate()
    {
        targetCircleTransform.size = targetCircleSize;

        wallUp.localScale = new Vector3(1000, 1000);
        wallUp.localPosition = new Vector3(0, wallUp.localScale.y * 0.5f + circleSize.y * 0.5f);

        wallDown.localScale = new Vector3(1000, 1000);
        wallDown.localPosition = new Vector3(0, -(wallUp.localScale.y * 0.5f + circleSize.y * 0.5f));

        wallLeft.localScale = new Vector3(1000, circleSize.y);
        wallLeft.localPosition = new Vector3(-(wallLeft.localScale.x * 0.5f + circleSize.x * 0.5f), 0f);

        wallRight.localScale = new Vector3(1000, circleSize.y);
        wallRight.localPosition = new Vector3(wallLeft.localScale.x * 0.5f + circleSize.x * 0.5f, 0f);
    }

    void GenerateTargetCircle()
    {
        if (isZoneEnd) return;

        zoneCounter++;

        float shrinkSizeAmount = Random.Range(5f, 30f);
        Vector3 generatadTargetCircleSize = circleSize - new Vector3(shrinkSizeAmount, shrinkSizeAmount);

        if(generatadTargetCircleSize.x < 0 || generatadTargetCircleSize.y < 0)
        {
            isZoneEnd = true;
            generatadTargetCircleSize = Vector3.zero;
        }

        this.shrinkTimer = Random.Range(5f, 10f);
        targetCircleSize = generatadTargetCircleSize;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = UnityEngine.Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, borderSize);
    }
}
