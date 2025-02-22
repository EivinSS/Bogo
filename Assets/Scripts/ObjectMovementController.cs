using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMovementController : MonoBehaviour
{
    public GameObject Player;

    public enum MovementType
    {
        None,
        Circular,
        PointToPoint
    }

    [Header("Movement Settings")]
    public MovementType movementType = MovementType.None;

    [Header("Circular Movement Settings")]
    public float radius = 5f;
    public float speed = 2f;

    [Header("Point-to-Point Movement Settings")]
    public List<Transform> points = new List<Transform>();
    public float pointToPointSpeed = 2f;
    public bool loop = false;
    public bool reverseAtEnd = false;

    private Vector3 startPosition;
    private float currentAngle = 0f;
    private Vector3 previousPosition;
    private int currentPointIndex = 0;
    private bool goingBackwards = false;

    public Vector3 Displacement { get; private set; }
    public Vector3 Velocity { get; private set; }

    private void Start()
    {
        startPosition = transform.position;
        previousPosition = transform.position;

        if (movementType == MovementType.PointToPoint && points.Count > 0)
        {
            Transform closestPoint = GetClosestPoint();
            transform.position = closestPoint.position;
            currentPointIndex = points.IndexOf(closestPoint);
        }
    }

    private void Update()
    {
        switch (movementType)
        {
            case MovementType.Circular:
                HandleCircularMovement();
                break;

            case MovementType.PointToPoint:
                HandlePointToPointMovement();
                break;
        }

        CalculateDisplacement();
    }

    private void HandleCircularMovement()
    {
        currentAngle += speed * Time.deltaTime;
        if (currentAngle > 360f)
        {
            currentAngle -= 360f;
        }

        float x = Mathf.Cos(currentAngle) * radius;
        float z = Mathf.Sin(currentAngle) * radius;

        transform.position = startPosition + new Vector3(x, 0f, z);
    }

    private void HandlePointToPointMovement()
    {
        if (points.Count < 2) return;

        Transform targetPoint = points[currentPointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, pointToPointSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            if (reverseAtEnd)
            {
                if (goingBackwards)
                {
                    currentPointIndex--;
                    if (currentPointIndex < 0)
                    {
                        currentPointIndex = 1;
                        goingBackwards = false;
                    }
                }
                else
                {
                    currentPointIndex++;
                    if (currentPointIndex >= points.Count)
                    {
                        currentPointIndex = points.Count - 2;
                        goingBackwards = true;
                    }
                }
            }
            else if (loop)
            {
                currentPointIndex = (currentPointIndex + 1) % points.Count;
            }
            else
            {
                currentPointIndex++;
                if (currentPointIndex >= points.Count)
                {
                    currentPointIndex = points.Count - 1;
                }
            }
        }
    }

    private Transform GetClosestPoint()
    {
        Transform closestPoint = null;
        float closestDistance = float.MaxValue;

        foreach (Transform point in points)
        {
            float distance = Vector3.Distance(transform.position, point.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    private void CalculateDisplacement()
    {
        Displacement = transform.position - previousPosition;
        Velocity = Displacement / Time.deltaTime;
        previousPosition = transform.position;
    }

    private void OnTriggerStay(Collider other)
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Player)
        {
            BallController ballController = Player.GetComponent<BallController>();
            if (ballController != null)
            {
                //ballController.currentPlatform = transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == Player)
        {
            // Reset the ball's current platform
            BallController ballController = Player.GetComponent<BallController>();
            if (ballController != null)
            {
                ballController.currentPlatform = null;
            }
        }
    }

}
