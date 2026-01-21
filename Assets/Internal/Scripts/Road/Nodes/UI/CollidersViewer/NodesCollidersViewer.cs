using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Road.Nodes.UI.CollidersViewer
{
    public class NodesCollidersViewer : INodesCollidersViewer, IInitializable
    {
        private const string JUNCTION_KEY = "N_Junction";
        
        private readonly IRoadNodeLookup _nodeLookup;
        private readonly NodeView _nodeViewPrefab;
        private readonly List<NodeView> _nodeColliders = new List<NodeView>();

        public NodesCollidersViewer(IRoadNodeLookup nodeLookup, NodeView nodeViewPrefab)
        {
            _nodeLookup = nodeLookup;
            _nodeViewPrefab = nodeViewPrefab;
        }

        public void Initialize()
        {
            CreateColliders();
        }

        public List<NodeView> GetAllNodes()
        {
            return _nodeColliders;
        }

        public void ShowColliders()
        {
            foreach (NodeView nodeView in _nodeColliders)
            {
                nodeView.Enable();
            }
        }

        public void HideColliders()
        {
            foreach (NodeView nodeView in _nodeColliders)
            {
                nodeView.Disable();
            }
        }
        
        private void CreateColliders()
        {
            IReadOnlyDictionary<string, Transform> allNodes = _nodeLookup.Nodes;
            foreach (KeyValuePair<string, Transform> node in allNodes)
            {
                if (node.Key.StartsWith(JUNCTION_KEY)) continue;
                NodeView nodeView = Object.Instantiate(_nodeViewPrefab.gameObject, node.Value)
                    .GetComponent<NodeView>();
                nodeView.Initialize(node.Key);
                _nodeColliders.Add(nodeView);
                nodeView.gameObject.SetActive(false);
            }
        }
    }
}