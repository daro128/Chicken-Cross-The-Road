using System.Collections.Generic;
using UnityEngine;

namespace ChickenRush
{
    /// <summary>Creates the cars of each road lane. Cars are recycled by CarController (wrap-around).</summary>
    public class TrafficSpawner : MonoBehaviour
    {
        [SerializeField] private GamePalette palette;
        [SerializeField, Min(5f)] private float laneHalfLength = 14f;
        [SerializeField, Min(1)] private int minCarsPerLane = 2;
        [SerializeField, Min(1)] private int maxCarsPerLane = 4;

        private readonly List<CarController> cars = new List<CarController>();
        private Transform root;

        public float LaneHalfLength => laneHalfLength;

        public void Clear()
        {
            cars.Clear();
            if (root != null) Destroy(root.gameObject);
            root = new GameObject("Traffic").transform;
            root.SetParent(transform, false);
        }

        public void SpawnLane(int row, int direction, float speed)
        {
            if (root == null) Clear();

            int count = Random.Range(minCarsPerLane, Mathf.Max(minCarsPerLane, maxCarsPerLane) + 1);
            float length = laneHalfLength * 2f;
            float spacing = length / count;
            float offset = Random.Range(0f, spacing);

            for (int i = 0; i < count; i++)
            {
                float jitter = Random.Range(-0.15f, 0.15f) * spacing;
                float x = -laneHalfLength + Mathf.Repeat(offset + i * spacing + jitter, length);
                cars.Add(CreateCar(row, x, direction, speed));
            }
        }

        public void SetMoving(bool value)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                if (cars[i] != null) cars[i].SetMoving(value);
            }
        }

        private CarController CreateCar(int row, float x, int direction, float speed)
        {
            GameObject go = new GameObject("Car_" + row);
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(x, 0f, row);
            go.transform.rotation = Quaternion.Euler(0f, direction >= 0 ? 0f : 180f, 0f);

            go.AddComponent<Rigidbody>();
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, 0.4f, 0f);
            box.size = new Vector3(1.7f, 0.8f, 0.85f);

            Material bodyMat = null;
            if (palette != null && palette.carBodies != null && palette.carBodies.Length > 0)
            {
                bodyMat = palette.carBodies[Random.Range(0, palette.carBodies.Length)];
            }
            PrimitiveFactory.BuildCar(go.transform, palette, bodyMat);

            CarController car = go.AddComponent<CarController>();
            car.Configure(direction, speed, laneHalfLength);
            return car;
        }
    }
}
