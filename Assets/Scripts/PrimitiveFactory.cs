using UnityEngine;

namespace ChickenRush
{
    /// <summary>Helpers that build the low-poly / voxel look out of Unity primitives (no external assets).</summary>
    public static class PrimitiveFactory
    {
        public static GameObject Create(PrimitiveType type, string name, Transform parent,
            Vector3 localPosition, Vector3 localScale, Material material, Vector3 localEuler = default)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;

            // Decorative parts have no collider; gameplay colliders are added explicitly elsewhere.
            Collider col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            Transform t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = localPosition;
            t.localRotation = Quaternion.Euler(localEuler);
            t.localScale = localScale;

            if (material != null)
            {
                Renderer r = go.GetComponent<Renderer>();
                if (r != null) r.sharedMaterial = material;
            }
            return go;
        }

        public static GameObject Cube(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            return Create(PrimitiveType.Cube, name, parent, pos, scale, mat);
        }

        /// <summary>Voxel-style chicken. Pivot is at ground level, facing +Z.</summary>
        public static void BuildChicken(Transform parent, GamePalette p)
        {
            Material white = p != null ? p.chickenWhite : null;
            Material wing = p != null ? p.chickenWing : null;
            Material beak = p != null ? p.beak : null;
            Material comb = p != null ? p.comb : null;
            Material eye = p != null ? p.eye : null;

            Cube("Body", parent, new Vector3(0f, 0.40f, 0f), new Vector3(0.60f, 0.50f, 0.70f), white);
            Cube("Head", parent, new Vector3(0f, 0.80f, 0.20f), new Vector3(0.40f, 0.40f, 0.40f), white);
            Cube("Beak", parent, new Vector3(0f, 0.75f, 0.45f), new Vector3(0.20f, 0.12f, 0.18f), beak);
            Cube("Comb", parent, new Vector3(0f, 1.05f, 0.20f), new Vector3(0.12f, 0.15f, 0.22f), comb);
            Cube("Wattle", parent, new Vector3(0f, 0.62f, 0.42f), new Vector3(0.08f, 0.10f, 0.08f), comb);
            Cube("EyeL", parent, new Vector3(-0.13f, 0.85f, 0.39f), new Vector3(0.07f, 0.07f, 0.04f), eye);
            Cube("EyeR", parent, new Vector3(0.13f, 0.85f, 0.39f), new Vector3(0.07f, 0.07f, 0.04f), eye);
            Cube("WingL", parent, new Vector3(-0.34f, 0.42f, -0.02f), new Vector3(0.08f, 0.30f, 0.45f), wing);
            Cube("WingR", parent, new Vector3(0.34f, 0.42f, -0.02f), new Vector3(0.08f, 0.30f, 0.45f), wing);
            Cube("Tail", parent, new Vector3(0f, 0.58f, -0.40f), new Vector3(0.20f, 0.25f, 0.12f), wing);
            Cube("LegL", parent, new Vector3(-0.12f, 0.10f, 0f), new Vector3(0.08f, 0.20f, 0.08f), beak);
            Cube("LegR", parent, new Vector3(0.12f, 0.10f, 0f), new Vector3(0.08f, 0.20f, 0.08f), beak);
        }

        /// <summary>Simple car built from boxes and cylinders. Front of the car points to local +X.</summary>
        public static void BuildCar(Transform parent, GamePalette p, Material bodyMat)
        {
            Material window = p != null ? p.carWindow : null;
            Material wheel = p != null ? p.carWheel : null;
            Material light = p != null ? p.carLight : null;

            Cube("Body", parent, new Vector3(0f, 0.30f, 0f), new Vector3(1.60f, 0.40f, 0.80f), bodyMat);
            Cube("Cabin", parent, new Vector3(-0.10f, 0.62f, 0f), new Vector3(0.80f, 0.30f, 0.70f), window);
            Cube("Roof", parent, new Vector3(-0.10f, 0.80f, 0f), new Vector3(0.70f, 0.06f, 0.64f), bodyMat);
            Cube("LightL", parent, new Vector3(0.81f, 0.32f, -0.25f), new Vector3(0.04f, 0.12f, 0.16f), light);
            Cube("LightR", parent, new Vector3(0.81f, 0.32f, 0.25f), new Vector3(0.04f, 0.12f, 0.16f), light);

            float[] xs = { -0.5f, 0.5f };
            float[] zs = { -0.40f, 0.40f };
            foreach (float x in xs)
            {
                foreach (float z in zs)
                {
                    Create(PrimitiveType.Cylinder, "Wheel", parent, new Vector3(x, 0.17f, z),
                        new Vector3(0.34f, 0.07f, 0.34f), wheel, new Vector3(90f, 0f, 0f));
                }
            }
        }
    }
}
