using System.Collections.Generic;
using UnityEngine;

public class ObjectMoverRb : MonoBehaviour
{
    public List<Transform> points = new List<Transform>();  // List of Transforms for the cube to move to
    public bool loop = true;  // Set to true for looping, false for reverse movement
    public float moveSpeed = 5f;  // Speed of movement
    private int currentTargetIndex = 0;  // Index of the current target point
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (points.Count > 0)
        {
            transform.position = points[0].position;  // Start at the first point
        }
    }

    private void Update()
    {
        if (points.Count > 1) // Ensure there are at least two points to move between
        {
            MoveCube();
        }
    }

    private void MoveCube()
    {
        // Move towards the current target point
        Vector3 target = points[currentTargetIndex].position;  // Get the position from the Transform
        Vector3 direction = (target - transform.position).normalized;
        rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);

        // Check if the cube has reached the current target
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            // Check whether to reverse or loop
            if (loop)
            {
                currentTargetIndex = (currentTargetIndex + 1) % points.Count;
            }
            else
            {
                if (currentTargetIndex == points.Count - 1 || currentTargetIndex == 0)
                {
                    // Reverse direction
                    points.Reverse();
                    currentTargetIndex = 0;
                }
            }
        }
    }
}
