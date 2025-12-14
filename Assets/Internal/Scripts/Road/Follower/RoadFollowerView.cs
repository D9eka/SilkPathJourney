using System.Collections.Generic;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Orientation;
using Internal.Scripts.Road.Pathfinder;
using Internal.Scripts.Road.Paths;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Road.Follower
{
    [RequireComponent(typeof(Transform))]
    public sealed class RoadFollowerView : MonoBehaviour
    {
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _rotationSpeed = 10f;
        
        private Transform _controller;
        private RoutePathCursor _cursor;
        private IOrientationStrategy _orientationStrategy;
        private IRoadPathfinderStrategy _roadPathfinderStrategy;

        private RoadGraph _graph;
        private RoadNode _currentRoadNode;

        [Inject]
        public void Construct(RoadGraph graph, IOrientationStrategy orientationStrategy, 
            IRoadPathfinderStrategy roadPathfinderStrategy, int startPointId)
        {
            _graph = graph;
            _orientationStrategy = orientationStrategy;
            _roadPathfinderStrategy = roadPathfinderStrategy;
            _currentRoadNode = _graph.Nodes[startPointId];
        }

        private void Awake()
        {
            _controller = GetComponent<Transform>();
            transform.position = _currentRoadNode.Position;
            _orientationStrategy.SetRotationSpeed(_rotationSpeed);
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
        
        public void GoToNode(int nodeId)
        {
            if (!_graph.TryGetNode(nodeId, out RoadNode to))
            {
                Debug.LogError("Invalid end node");
                return;
            }

            SetupCursor(_currentRoadNode, to);
        }

        private void SetupCursor(RoadNode from, RoadNode to)
        {
            List<RoadNode> path = _roadPathfinderStrategy.FindPathNodes(_graph, from, to);
            if (path.Count == 0)
            {
                Debug.LogError("No path from startFromNodeId to startToNodeId");
                return;
            }

            if (_cursor == null)
            {
                _cursor = new RoutePathCursor(path);
                _cursor.CurrentNodeChanged += CursorOnCurrentNodeChanged;
                _cursor.Finished += CursorOnFinished;
            }
            
            _cursor = new RoutePathCursor(path);
        }

        private void CursorOnCurrentNodeChanged(RoadNode currentNode)
        {
            _currentRoadNode = currentNode;
        }

        private void CursorOnFinished()
        {
            transform.position = _currentRoadNode.Position;
        }
    }
}
