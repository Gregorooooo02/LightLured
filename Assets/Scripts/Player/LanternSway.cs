using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternSway : MonoBehaviour
{
    [SerializeField] private float swayAmount = 0.5f;
    [SerializeField] private float swayMaxAmount = 0.1f;
    [SerializeField] private float smoothAmount;

    private Vector3 initialPosition;

    private void Start() {
        initialPosition = transform.localPosition;
    }

    private void Update() {
        float moveX = -Input.GetAxis("Mouse X") * swayAmount;
        float moveY = -Input.GetAxis("Mouse Y") * swayAmount;

        moveX = Mathf.Clamp(moveX, -swayMaxAmount, swayMaxAmount);
        moveY = Mathf.Clamp(moveY, -swayMaxAmount, swayMaxAmount);

        Vector3 finalPosition = new Vector3(moveX, moveY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);
    }
}
