using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBehavior : MonoBehaviour
{
    public Transform player; // Assign via the inspector
    public float moveForce = 2f;
    public float minHeightFromGround = 2f;
    public float minDistanceFromPlayer = 1f;
    public float rotationSpeed = 2.0f;

    public LayerMask groundLayer; // Assign the ground layer

    private Rigidbody rb;
    private RaycastHit hit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Adjust height using force
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity, groundLayer))
        {
            if (hit.distance < minHeightFromGround)
            {
                // Apply upward force
                rb.AddForce(Vector3.up * moveForce);
            }
        }

        // Move towards player
        Vector3 directionToPlayer = player.position - transform.position;
        // directionToPlayer.y = 0; // Ignore vertical component for this calculation

        if (directionToPlayer.magnitude > minDistanceFromPlayer)
        {
            Vector3 moveDirection = directionToPlayer.normalized;
            rb.AddForce(moveDirection * moveForce);
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }
}
