using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveDistance = 1f;   // 1 grid block forward/sideways
    public float moveSpeed = 15f;      // Speed of the movement
    public float hopHeight = 0.3f;    // Height of the hop arc

    private bool isMoving = false;

    void Update()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            StartCoroutine(MovePlayer(Vector3.forward));
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartCoroutine(MovePlayer(Vector3.left));
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartCoroutine(MovePlayer(Vector3.right));
        }
    }

    private IEnumerator MovePlayer(Vector3 direction)
    {
        isMoving = true;

        // Rotate chicken instantly toward the movement direction
        transform.rotation = Quaternion.LookRotation(direction);

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (direction * moveDistance);
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * moveSpeed;
            
            // Lerp straight position
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, elapsedTime);

            // Add vertical arc for the hop effect
            currentPos.y += Mathf.Sin(elapsedTime * Mathf.PI) * hopHeight;

            transform.position = currentPos;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
}