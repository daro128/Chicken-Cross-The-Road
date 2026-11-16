using System.Collections.Generic;
using UnityEngine;

namespace ChickenRush
{
    public enum RowType { Grass, Road, Finish }

    /// <summary>
    /// Procedurally builds the level row by row (one row = one grid cell deep along Z):
    /// grass with trees, road lanes with markings, and a finish row. Also answers grid queries
    /// (bounds / blocked cells) for the player.
    /// A "corridor" of guaranteed free cells in every grass row means the level can always be completed.
    /// </summary>
    public class EnvironmentGenerator : MonoBehaviour
    {
        [SerializeField] private GamePalette palette;
        [SerializeField] private TrafficSpawner traffic;

        [Header("Generation")]
        [SerializeField] private bool randomSeed = true;
        [SerializeField] private int seed = 12345;
        [SerializeField, Min(2)] private int playHalfWidth = 5;
        [SerializeField, Min(6)] private int decorHalfWidth = 14;
        [SerializeField, Min(1)] private int rowsBehind = 5;
        [SerializeField, Min(2)] private int rowsBeyondFinish = 8;
        [SerializeField, Range(0f, 0.5f)] private float treeDensity = 0.18f;

        private readonly HashSet<Vector2Int> blocked = new HashSet<Vector2Int>();
        private Transform rowRoot;
        private int finishRow = int.MaxValue;

        public int FinishRow => finishRow;

        /// <summary>World position of the centre of a grid cell (x = column, y = row).</summary>
        public static Vector3 CellToWorld(Vector2Int cell)
        {
            return new Vector3(cell.x, 0f, cell.y);
        }

        public bool InBounds(Vector2Int cell)
        {
            return cell.x >= -playHalfWidth && cell.x <= playHalfWidth && cell.y >= 0 && cell.y <= finishRow;
        }

        public bool IsBlocked(Vector2Int cell)
        {
            return blocked.Contains(cell);
        }

        public void Generate(int targetRow)
        {
            Clear();
            finishRow = Mathf.Max(5, targetRow);
            System.Random rng = new System.Random(randomSeed ? System.Environment.TickCount : seed);

            rowRoot = new GameObject("GeneratedRows").transform;
            rowRoot.SetParent(transform, false);
            if (traffic != null) traffic.Clear();

            float laneHalf = traffic != null ? traffic.LaneHalfLength : 14f;
            float slabWidth = laneHalf * 2f + 2f;
            int lastRow = finishRow + rowsBeyondFinish;
            RowType[] types = new RowType[lastRow + rowsBehind + 1];

            // Large dark-green ground under everything so the horizon is never empty.
            float groundDepth = lastRow + rowsBehind + 90f;
            PrimitiveFactory.Cube("Ground", rowRoot, new Vector3(0f, -0.55f, (lastRow - rowsBehind) * 0.5f),
                new Vector3(220f, 1f, groundDepth), palette != null ? palette.grassDark : null);

            int corridor = 0;
            bool inRoad = false;
            int remaining = 0;
            int dir = 1;
            RowType prev = RowType.Grass;

            for (int z = -rowsBehind; z <= lastRow; z++)
            {
                RowType type;
                if (z >= finishRow)
                {
                    type = z == finishRow ? RowType.Finish : RowType.Grass;
                }
                else if (z <= 1 || z == finishRow - 1)
                {
                    type = RowType.Grass;
                    inRoad = false;
                    remaining = 0;
                }
                else
                {
                    if (remaining <= 0)
                    {
                        inRoad = !inRoad;
                        remaining = inRoad ? rng.Next(1, 4) : rng.Next(1, 3);
                    }
                    remaining--;
                    type = inRoad ? RowType.Road : RowType.Grass;
                }

                types[z + rowsBehind] = type;
                CreateSlab(z, type, slabWidth);

                if (type == RowType.Road)
                {
                    dir = prev == RowType.Road ? -dir : (rng.Next(0, 2) == 0 ? -1 : 1);
                    float speed = 2f + (float)rng.NextDouble() * 3.5f;
                    if (traffic != null) traffic.SpawnLane(z, dir, speed);
                    if (prev == RowType.Road) CreateDashes(z, slabWidth);
                }
                else
                {
                    int previousCorridor = corridor;
                    if (z < finishRow)
                    {
                        corridor = Mathf.Clamp(corridor + rng.Next(-1, 2), -playHalfWidth, playHalfWidth);
                    }
                    DecorateGrass(z, type, previousCorridor, corridor, rng);
                    if (type == RowType.Finish) CreateFinish(z);
                }
                prev = type;
            }

            // Solid white edge lines at the start and end of every road group.
            for (int z = -rowsBehind; z <= lastRow; z++)
            {
                if (types[z + rowsBehind] != RowType.Road) continue;
                bool startsGroup = z == -rowsBehind || types[z + rowsBehind - 1] != RowType.Road;
                bool endsGroup = z == lastRow || types[z + rowsBehind + 1] != RowType.Road;
                if (startsGroup) CreateEdgeLine(z - 0.42f, slabWidth);
                if (endsGroup) CreateEdgeLine(z + 0.42f, slabWidth);
            }
        }

        public void Clear()
        {
            blocked.Clear();
            if (rowRoot != null) Destroy(rowRoot.gameObject);
            rowRoot = null;
        }

        private void CreateSlab(int z, RowType type, float width)
        {
            Material mat = null;
            if (palette != null)
            {
                if (type == RowType.Road) mat = palette.road;
                else if (type == RowType.Finish) mat = palette.finishTile;
                else mat = (z & 1) == 0 ? palette.grassLight : palette.grassDark;
            }
            PrimitiveFactory.Cube("Row_" + z, rowRoot, new Vector3(0f, -0.5f, z), new Vector3(width, 1f, 1f), mat);
        }

        private void CreateDashes(int z, float width)
        {
            Material white = palette != null ? palette.marking : null;
            for (float x = -width * 0.5f + 1f; x < width * 0.5f - 0.5f; x += 2f)
            {
                PrimitiveFactory.Cube("Dash", rowRoot, new Vector3(x, 0.011f, z - 0.5f), new Vector3(1f, 0.02f, 0.07f), white);
            }
        }

        private void CreateEdgeLine(float z, float width)
        {
            Material white = palette != null ? palette.marking : null;
            PrimitiveFactory.Cube("EdgeLine", rowRoot, new Vector3(0f, 0.011f, z), new Vector3(width, 0.02f, 0.07f), white);
        }

        private void DecorateGrass(int z, RowType type, int corridorA, int corridorB, System.Random rng)
        {
            bool beyondFinish = z > finishRow;
            for (int x = -decorHalfWidth; x <= decorHalfWidth; x++)
            {
                bool inside = Mathf.Abs(x) <= playHalfWidth;
                if (inside)
                {
                    if (type == RowType.Finish) continue;
                    if (beyondFinish)
                    {
                        if (rng.NextDouble() < 0.35) CreateTree(x, z, rng);
                        continue;
                    }
                    if (x == corridorA || x == corridorB) continue;   // guaranteed free cells
                    if (rng.NextDouble() < treeDensity)
                    {
                        CreateTree(x, z, rng);
                        blocked.Add(new Vector2Int(x, z));
                    }
                    else if (rng.NextDouble() < 0.06)
                    {
                        CreateFlower(x, z, rng);
                    }
                }
                else if (rng.NextDouble() < 0.4)
                {
                    CreateTree(x, z, rng);
                }
            }
        }

        private void CreateTree(int x, int z, System.Random rng)
        {
            float trunkHeight = 0.5f + (float)rng.NextDouble() * 0.4f;
            Material leaf = null;
            Material trunk = null;
            if (palette != null)
            {
                trunk = palette.trunk;
                leaf = rng.Next(0, 2) == 0 ? palette.leaves : palette.leavesDark;
            }

            GameObject tree = new GameObject("Tree");
            tree.transform.SetParent(rowRoot, false);
            tree.transform.position = new Vector3(x, 0f, z);

            PrimitiveFactory.Create(PrimitiveType.Cylinder, "Trunk", tree.transform,
                new Vector3(0f, trunkHeight * 0.5f, 0f), new Vector3(0.28f, trunkHeight * 0.5f, 0.28f), trunk);
            PrimitiveFactory.Cube("Leaves", tree.transform,
                new Vector3(0f, trunkHeight + 0.35f, 0f), new Vector3(0.9f, 0.8f, 0.9f), leaf);
            PrimitiveFactory.Cube("LeavesTop", tree.transform,
                new Vector3(0f, trunkHeight + 0.95f, 0f), new Vector3(0.55f, 0.5f, 0.55f), leaf);
            tree.transform.rotation = Quaternion.Euler(0f, rng.Next(0, 4) * 90f, 0f);
        }

        private void CreateFlower(int x, int z, System.Random rng)
        {
            Material mat = null;
            if (palette != null) mat = rng.Next(0, 2) == 0 ? palette.flowerPink : palette.flowerYellow;
            float ox = ((float)rng.NextDouble() - 0.5f) * 0.5f;
            float oz = ((float)rng.NextDouble() - 0.5f) * 0.5f;
            PrimitiveFactory.Cube("Flower", rowRoot, new Vector3(x + ox, 0.07f, z + oz), new Vector3(0.14f, 0.14f, 0.14f), mat);
        }

        private void CreateFinish(int z)
        {
            Material white = palette != null ? palette.marking : null;
            Material black = palette != null ? palette.eye : null;
            Material pole = palette != null ? palette.finishPole : null;
            Material banner = palette != null ? palette.finishTile : null;

            // Chequered strip across the finish row.
            for (int x = -playHalfWidth; x <= playHalfWidth; x++)
            {
                for (int k = 0; k < 2; k++)
                {
                    Material m = ((x + k) & 1) == 0 ? white : black;
                    PrimitiveFactory.Cube("Check", rowRoot, new Vector3(x, 0.011f, z - 0.25f + 0.5f * k),
                        new Vector3(1f, 0.02f, 0.5f), m);
                }
            }

            float poleX = playHalfWidth + 0.8f;
            PrimitiveFactory.Create(PrimitiveType.Cylinder, "FinishPoleL", rowRoot,
                new Vector3(-poleX, 1.5f, z), new Vector3(0.2f, 1.5f, 0.2f), pole);
            PrimitiveFactory.Create(PrimitiveType.Cylinder, "FinishPoleR", rowRoot,
                new Vector3(poleX, 1.5f, z), new Vector3(0.2f, 1.5f, 0.2f), pole);
            PrimitiveFactory.Cube("FinishBanner", rowRoot, new Vector3(0f, 3.0f, z),
                new Vector3(poleX * 2f, 0.6f, 0.15f), banner);
        }
    }
}
