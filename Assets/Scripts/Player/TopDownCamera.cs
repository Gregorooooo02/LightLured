using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float height = 15.0f;

    private void Update()
    {
        if (target != null)
        {
            transform.position = new Vector3(target.position.x, height, target.position.z);
        }
    }
}
