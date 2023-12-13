using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class m_PlayerFollow : MonoBehaviour
{
    Vector3 refVelocity;

    // Update is called once per frame
    void LateUpdate()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null && NetworkClient.active)
        {
            Vector3 targetPosition = new Vector3(GameObject.FindGameObjectWithTag("Player").transform.position.x, GameObject.FindGameObjectWithTag("Player").transform.position.y, -10);
           // transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref refVelocity, 0.01f);
            transform.position =targetPosition;
        }
    }
}
