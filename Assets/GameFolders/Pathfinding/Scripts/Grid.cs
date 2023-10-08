using System.Collections.Generic;
using ClashOfClans.Cores;
using UnityEngine;

namespace ClashOfClans.Pathfinding
{
    public class Grid : MonoBehaviour
    {
        public bool displayGridGizmos;
        public LayerMask unwalkableLayerMask;
        public Vector2 gridWorldSize;
        public float nodeRadius;
        public TerrainType[] walkableRegions;
        public int obstacleProximityPenalty = 10;
        private LayerMask _walkableMask;
        private Dictionary<int, int> _walkableRegionDictionary = new Dictionary<int, int>();

        private Transform _transform;
        private Node[,] _grid;
        private float _nodeDiameter;
        private int _gridSizeX;
        private int _gridSizeY;

        private int _penaltyMin = int.MaxValue;
        private int _penaltyMax = int.MinValue;


        private void Awake()
        {
            _transform = GetComponent<Transform>();

            _nodeDiameter = nodeRadius * 2;
            _gridSizeX = Mathf.RoundToInt(gridWorldSize.x / _nodeDiameter);
            _gridSizeY = Mathf.RoundToInt(gridWorldSize.y / _nodeDiameter);

            foreach (TerrainType region in walkableRegions)
            {
                _walkableMask.value |= region.terrainMask.value;
                _walkableRegionDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
            }

            CreateGrid();
        }

        public int MaxSize
        {
            get { return _gridSizeX * _gridSizeY; }
        }

        private void CreateGrid()
        {
            _grid = new Node[_gridSizeX, _gridSizeY];
            Vector3 worldBottomLeft = _transform.position - VectorHelper.Right * gridWorldSize.x / 2 -
                                      VectorHelper.Forward * gridWorldSize.y / 2;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + VectorHelper.Right * (x * _nodeDiameter + nodeRadius) +
                                         VectorHelper.Forward * (y * _nodeDiameter + nodeRadius);
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableLayerMask));

                    int movementPenalty = 0;


                    Ray ray = new Ray(worldPoint + VectorHelper.Up * 50, VectorHelper.Down);

                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 100, _walkableMask))
                    {
                        _walkableRegionDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }

                    if (!walkable)
                    {
                        movementPenalty += obstacleProximityPenalty;
                    }


                    _grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
                }
            }

            BlurPenaltyMap(3);
        }

        private void BlurPenaltyMap(int blurSize)
        {
            int kernelSize = blurSize * 2 + 1;
            int kernelExtents = (kernelSize - 1) / 2;

            int[,] penaltiesHorizontalPass = new int[_gridSizeX, _gridSizeY];
            int[,] penaltiesVerticalPass = new int[_gridSizeX, _gridSizeY];

            for (int y = 0; y < _gridSizeY; y++)
            {
                for (int x = -kernelExtents; x <= kernelExtents; x++)
                {
                    int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                    penaltiesHorizontalPass[0, y] += _grid[sampleX, y].movementPenalty;
                }

                for (int x = 1; x < _gridSizeX; x++)
                {
                    int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, _gridSizeX);
                    int addIndex = Mathf.Clamp(x + kernelExtents, 0, _gridSizeX - 1);

                    penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] -
                        _grid[removeIndex, y].movementPenalty + _grid[addIndex, y].movementPenalty;
                }
            }

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = -kernelExtents; y <= kernelExtents; y++)
                {
                    int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                    penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
                }

                int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));

                _grid[x, 0].movementPenalty = blurredPenalty;

                for (int y = 1; y < _gridSizeX; y++)
                {
                    int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, _gridSizeY);
                    int addIndex = Mathf.Clamp(y + kernelExtents, 0, _gridSizeY - 1);

                    penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] -
                        penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];

                    blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));

                    _grid[x, y].movementPenalty = blurredPenalty;

                    if (blurredPenalty > _penaltyMax)
                    {
                        _penaltyMax = blurredPenalty;
                    }

                    if (blurredPenalty < _penaltyMin)
                    {
                        _penaltyMin = blurredPenalty;
                    }
                }
            }
        }

        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < _gridSizeX &&
                        checkY >= 0 && checkY < _gridSizeY)
                    {
                        neighbours.Add(_grid[checkX, checkY]);
                    }
                }
            }

            return neighbours;
        }

        public Node NodeFromWorldPoint(Vector3 worldPosition)
        {
            float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
            float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

            return _grid[x, y];
        }

        [System.Serializable]
        public class TerrainType
        {
            public LayerMask terrainMask;
            public int terrainPenalty;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

            if (_grid != null && displayGridGizmos)
            {
                foreach (Node node in _grid)
                {
                    Gizmos.color = Color.Lerp(Color.white, Color.black,
                        Mathf.InverseLerp(_penaltyMin, _penaltyMax, node.movementPenalty));
                    Gizmos.color = (node.walkable) ? Gizmos.color : Color.red;

                    Gizmos.DrawCube(node.worldPosition, VectorHelper.One * (_nodeDiameter));
                }
            }
        }
    }
}