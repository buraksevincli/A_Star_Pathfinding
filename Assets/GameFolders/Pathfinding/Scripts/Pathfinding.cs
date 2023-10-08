using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

namespace ClashOfClans.Pathfinding
{
    public class Pathfinding : MonoBehaviour
    {
        private Grid _grid;

        private void Awake()
        {
            _grid = GetComponent<Grid>();
        }

        public void FindPath(PathRequest request, Action<PathResult> callback)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Vector3[] waypoints = new Vector3[0];
            bool pathSuccess = false;
            
            Node startNode = _grid.NodeFromWorldPoint(request.pathStart);
            Node targetNode = _grid.NodeFromWorldPoint(request.pathEnd);

            if (startNode.walkable && targetNode.walkable)
            {
                Heap<Node> openSet = new Heap<Node>(_grid.MaxSize);
                HashSet<Node> closedSet = new HashSet<Node>();

                openSet.Add(startNode);

                while (openSet.Count > 0)
                {
                    Node currentNode = openSet.RemoveFirst();
                
                    closedSet.Add(currentNode);

                    if (currentNode == targetNode)
                    {
                        stopwatch.Stop();
                        print("Path found " + stopwatch.ElapsedMilliseconds + " ms");
                        pathSuccess = true;
                        break;
                    }

                    foreach (Node neighbour in _grid.GetNeighbours(currentNode))
                    {
                        if (!neighbour.walkable || closedSet.Contains(neighbour))
                        {
                            continue;
                        }

                        int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;

                        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetNode);
                            neighbour.parent = currentNode;

                            if (!openSet.Contains(neighbour))
                            {
                                openSet.Add(neighbour);
                            }
                            else
                            {
                                openSet.UpdateItem(neighbour);
                            }
                        }
                    }
                }
            }
            
            if (pathSuccess)
            {
                waypoints = RetracePath(startNode, targetNode);
                pathSuccess = waypoints.Length > 0;
            }

            callback(new PathResult(waypoints, pathSuccess, request.callback));
        }

        private Vector3[] RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            Vector3[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);
            return waypoints;
        }

        private Vector3[] SimplifyPath(List<Node> path)
        {
            List<Vector3> waypoint = new List<Vector3>();
            Vector2 directionOld = Vector2.zero;

            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);

                if (directionNew != directionOld)
                {
                    waypoint.Add(path[i].worldPosition);
                }

                directionOld = directionNew;
            }

            return waypoint.ToArray();
        }

        private int GetDistance(Node nodeA, Node nodeB)
        {
            int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (distanceX > distanceY)
                return 14 * distanceY + 10 * (distanceX - distanceY);
            return 14 * distanceX + 10 * (distanceY - distanceX);
            
        }
    }
}
