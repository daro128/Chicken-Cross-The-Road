using UnityEngine;

namespace ChickenRush
{
    /// <summary>
    /// Grid-based chicken movement. Input is read in Update, the hop itself is advanced in FixedUpdate
    /// through Rigidbody.MovePosition (kinematic body) so physics triggers with the cars stay reliable.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private EnvironmentGenerator environment;
        [SerializeField] private Transform visual;

        [Header("Hop")]
        [SerializeField, Min(0.05f)] private float hopDuration = 0.14f;
        [SerializeField, Min(0f)] private float hopHeight = 0.4f;
        [SerializeField, Min(1f)] private float turnSpeed = 900f;

        private Rigidbody body;
        private Vector2Int cell = Vector2Int.zero;
        private bool alive = true;
        private bool hopping;
        private float hopTime;
        private Vector3 hopFrom;
        private Vector3 hopTo;
        private float targetYaw;
        private bool hasBuffered;
        private Vector2Int buffered;

        public Vector2Int Cell => cell;
        public bool IsAlive => alive;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void Start()
        {
            Vector3 start = EnvironmentGenerator.CellToWorld(cell);
            body.position = start;
            transform.position = start;
        }

        private void Update()
        {
            AnimateVisual();

            if (!alive || gameManager == null || gameManager.State != GameState.Playing) return;

            Vector2Int move = InputReader.ReadMoveDown();
            if (move != Vector2Int.zero)
            {
                if (hopping)
                {
                    buffered = move;       // remember one input so fast tapping feels responsive
                    hasBuffered = true;
                }
                else
                {
                    TryMove(move);
                }
            }
            else if (!hopping && InputReader.HopPressed())
            {
                StartHop(cell);            // optional hop in place
            }
        }

        private void FixedUpdate()
        {
            if (!hopping) return;

            hopTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(hopTime / hopDuration);
            Vector3 p = Vector3.Lerp(hopFrom, hopTo, t);
            p.y = Mathf.Sin(t * Mathf.PI) * hopHeight;
            body.MovePosition(p);

            if (t >= 1f)
            {
                hopping = false;
                OnLanded();
            }
        }

        private void TryMove(Vector2Int direction)
        {
            targetYaw = YawFor(direction);
            Vector2Int target = cell + direction;

            if (environment != null && (!environment.InBounds(target) || environment.IsBlocked(target)))
            {
                return;                    // edge of the map or a tree: ignore the move
            }

            cell = target;
            StartHop(cell);
        }

        private void StartHop(Vector2Int destination)
        {
            hopFrom = body.position;
            hopFrom.y = 0f;
            hopTo = EnvironmentGenerator.CellToWorld(destination);
            hopTime = 0f;
            hopping = true;
        }

        private void OnLanded()
        {
            if (!alive) return;
            if (scoreManager != null) scoreManager.ReportRow(cell.y);

            if (environment != null && gameManager != null && cell.y >= environment.FinishRow)
            {
                gameManager.Victory();
                return;
            }

            if (hasBuffered && gameManager != null && gameManager.State == GameState.Playing)
            {
                hasBuffered = false;
                TryMove(buffered);
            }
        }

        /// <summary>Called by CarController when a car touches the chicken.</summary>
        public void Die()
        {
            if (!alive) return;
            if (gameManager == null || gameManager.State != GameState.Playing) return;

            alive = false;
            hopping = false;
            hasBuffered = false;
            gameManager.GameOver();
        }

        private void AnimateVisual()
        {
            if (visual == null) return;

            if (!alive)
            {
                visual.localScale = new Vector3(1.25f, 0.15f, 1.25f);   // squashed
                return;
            }

            visual.localRotation = Quaternion.RotateTowards(visual.localRotation,
                Quaternion.Euler(0f, targetYaw, 0f), turnSpeed * Time.deltaTime);

            float t = hopping ? Mathf.Clamp01(hopTime / hopDuration) : 0f;
            float squash = Mathf.Sin(t * Mathf.PI);
            visual.localScale = new Vector3(1f - 0.1f * squash, 1f + 0.2f * squash, 1f - 0.1f * squash);
        }

        private static float YawFor(Vector2Int dir)
        {
            if (dir == Vector2Int.down) return 180f;
            if (dir == Vector2Int.left) return -90f;
            if (dir == Vector2Int.right) return 90f;
            return 0f;
        }
    }
}
