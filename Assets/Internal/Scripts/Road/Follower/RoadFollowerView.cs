using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Orientation;
using Internal.Scripts.Road.Paths;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Road.Follower
{
    public sealed class RoadFollowerView : MonoBehaviour
    {
        [SerializeField] private int _startFromNodeId;
        [SerializeField] private int _startToNodeId;
        [SerializeField] private float _speed = 3f;
        [SerializeField] private float _rotationSpeed = 10f;

        private RoadGraph _graph;
        private Transform _controller;
        private PathCursor _cursor;
        private IOrientationStrategy _orientationStrategy;

        [Inject]
        public void Construct(RoadGraph graph)
        {
            _graph = graph;
        }

        private void Start()
        {
            _controller = GetComponent<Transform>();

            if (!_graph.TryGetNode(_startFromNodeId, out RoadNode from) ||
                !_graph.TryGetNode(_startToNodeId, out RoadNode to))
            {
                Debug.LogError("Invalid start nodes");
                enabled = false;
                return;
            }

            RoadEdge edge = from.OutgoingEdges.FirstOrDefault(e => e.To == to);
            if (edge == null)
            {
                Debug.LogError("No edge from startFromNodeId to startToNodeId");
                enabled = false;
                return;
            }

            _cursor = new PathCursor(edge);
            _cursor.JunctionReached += OnJunctionReached;

            _orientationStrategy = new Orientation2DStrategy(_rotationSpeed);

            transform.position = _cursor.Position;
        }

        private void Update()
        {
            if (_cursor == null) return;

            float deltaDist = _speed * Time.deltaTime;
            _cursor.Advance(deltaDist);

            Vector3 targetPos = _cursor.Position;
            Vector3 forward = _cursor.Forward;

            Vector3 delta = targetPos - transform.position;
            _controller.position += delta;
            
            _orientationStrategy?.Apply(transform, forward, Time.deltaTime);
        }

        private void OnJunctionReached(RoadNode node)
        {
            IReadOnlyList<RoadEdge> options = node.OutgoingEdges;

            if (options.Count == 0) return;

            // TODO: заменить на выбор игрока.
            RoadEdge chosen = options[0];
            _cursor.SwitchToEdge(chosen);
        }

        public void ChooseEdge(RoadEdge edge)
        {
            _cursor.SwitchToEdge(edge);
        }
    }
}
