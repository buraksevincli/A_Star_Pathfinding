using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

namespace ClashOfClans.Pathfinding
{
    public class PathRequestManager : MonoBehaviour
    {
        private Queue<PathResult> _results = new Queue<PathResult>();

        private Pathfinding _pathfinding;
        private static PathRequestManager _instance;
        
        private void Awake()
        {
            _instance = this;
            _pathfinding = GetComponent<Pathfinding>();
        }

        private void Update()
        {
            if (_results.Count > 0)
            {
                int itemsInQueue = _results.Count;
                lock (_results)
                {
                    for (int i = 0; i < itemsInQueue; i++)
                    {
                        PathResult result = _results.Dequeue();
                        result.callback(result.path, result.success);
                    }
                }
            }
        }

        public static void RequestPath(PathRequest request)
        {
            ThreadStart threadStart = delegate
            {
                _instance._pathfinding.FindPath(request, _instance.FinishedProcessingPath);
            };
            threadStart.Invoke();
        }

        public void FinishedProcessingPath(PathResult result)
        {
            lock (_results)
            {
                _results.Enqueue(result);
            }
        }
    }

    public struct PathResult
    {
        public Vector3[] path;
        public bool success;
        public Action<Vector3[], bool> callback;

        public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
        {
            this.path = path;
            this.success = success;
            this.callback = callback;
        }
    }
    public struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> callback)
        {
            pathStart = start;
            pathEnd = end;
            this.callback = callback;
        }
    }
}
