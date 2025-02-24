using System.Collections.Generic;
using UnityEngine;

public class ObjectMoverRb : MonoBehaviour
{
    public enum ActionType { MoveToPoint }

    [System.Serializable]
    public class MovementAction
    {
        public ActionType actionType;
        public Transform targetPoint; // Used for MoveToPoint
    }

    public List<MovementAction> actions = new List<MovementAction>();
    public bool reverseAtEnd = false;
    public float moveSpeed = 5f;

    private int currentActionIndex = 0;
    private bool reversing = false;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (actions.Count > 0 && actions[0].actionType == ActionType.MoveToPoint)
        {
            transform.position = actions[0].targetPoint.position;
        }
    }

    private void Update()
    {
        if (actions.Count > 0)
        {
            PerformAction(actions[currentActionIndex]);
        }
    }

    private void PerformAction(MovementAction action)
    {
        if (action.actionType == ActionType.MoveToPoint)
        {
            MoveToPoint(action.targetPoint.position);
        }
    }

    public void MoveToPoint(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        rb.MovePosition(transform.position + direction * (moveSpeed * Time.deltaTime));

        if (Vector3.Distance(transform.position, target) < 1f)
        {
            GoToNextAction();
        }
    }

    private void GoToNextAction()
    {
        if (reversing)
        {
            currentActionIndex--;
            if (currentActionIndex < 0)
            {
                reversing = false;
                currentActionIndex = 1;
            }
        }
        else
        {
            currentActionIndex++;
            if (currentActionIndex >= actions.Count)
            {
                if (reverseAtEnd)
                {
                    reversing = true;
                    currentActionIndex = actions.Count - 2;
                }
                else
                {
                    currentActionIndex = 0; // Move back to first action naturally
                }
            }
        }
    }
}