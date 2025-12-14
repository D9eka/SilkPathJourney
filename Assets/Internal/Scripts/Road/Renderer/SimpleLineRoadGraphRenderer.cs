using Internal.Scripts.Road.Graph;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Road.Renderer
{
    public sealed class SimpleLineRoadGraphRenderer : MonoBehaviour, IRoadGraphRenderer
    {
        private const string ROOT_NAME = "__RoadLines";
        
        [Header("Visual")]
        [SerializeField] private Material _material;
        [SerializeField] private float _width = 0.25f;
        [SerializeField] private Color _color = Color.white;
        [Space]
        [Header("Sampling")]
        [SerializeField] private int _segmentsPerEdge = 32;
        [Space]
        [Header("2D Sorting")]
        [SerializeField] private string _sortingLayer = "Default";
        [SerializeField] private int _sortingOrder = 0;
        [Space]
        [Header("Topdown")]
        [SerializeField] private bool _forceZ = true;
        [SerializeField] private float _zValue = 0f;
        
        private RoadGraph _roadGraph;

        [Inject]
        public void Construct(RoadGraph graph)
        {
            _roadGraph = graph;
        }

        private void Awake()
        {
            transform.position = Vector3.zero;
            Build(_roadGraph, gameObject);
        }

        public void Build(RoadGraph graph, GameObject owner)
        {
            Transform root = GetOrCreateRoot(owner);

            ClearRoot(root);

            foreach (RoadEdge edge in graph.Edges)
            {
                BuildEdge(edge, root);
            }
        }

        private void BuildEdge(RoadEdge edge, Transform root)
        {
            GameObject go = new GameObject($"edge_{edge.From.Id}_{edge.To.Id}");
            go.transform.SetParent(root, false);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = false;

            lr.material = _material != null
                ? _material
                : new Material(Shader.Find("Sprites/Default"));

            lr.startWidth = _width;
            lr.endWidth = _width;

            lr.startColor = _color;
            lr.endColor = _color;

            lr.sortingLayerName = _sortingLayer;
            lr.sortingOrder = _sortingOrder;

            var points = edge.Path.SampleByDistance(_segmentsPerEdge);

            lr.positionCount = points.Count;

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 p = points[i];
                if (_forceZ)
                    p.z = _zValue;

                lr.SetPosition(i, p);
            }
        }

        private static Transform GetOrCreateRoot(GameObject owner)
        {
            Transform root = owner.transform.Find(ROOT_NAME);
            if (root != null)
                return root;

            GameObject go = new GameObject(ROOT_NAME);
            go.transform.SetParent(owner.transform, false);
            return go.transform;
        }

        private static void ClearRoot(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(root.GetChild(i).gameObject);
                else
                    Destroy(root.GetChild(i).gameObject);
#else
                Destroy(root.GetChild(i).gameObject);
#endif
            }
        }
    }
}
