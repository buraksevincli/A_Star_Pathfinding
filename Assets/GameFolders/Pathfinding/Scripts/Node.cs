using UnityEngine;

namespace ClashOfClans.Pathfinding
{
    public class Node : IHeapItem<Node>
    {
        public bool walkable;
        public Vector3 worldPosition;
        public int gridX;
        public int gridY;
        public int movementPenalty;

        public Node parent;
        public int gCost;
        public int hCost;
        private int _heapIndex;

        public Node(bool walkable, Vector3 worldPosition, int gridX, int gridY, int penalty)
        {
            this.walkable = walkable;
            this.worldPosition = worldPosition;
            this.gridX = gridX;
            this.gridY = gridY;
            movementPenalty = penalty;
        }

        public int FCost()
        {
            return gCost + hCost;
        }

        public int HeapIndex
        {
            get
            {
                return _heapIndex;
            }
            set
            {
                _heapIndex = value;
            }
        }

        public int CompareTo(Node nodeToCompare)
        {
            int compare = FCost().CompareTo(nodeToCompare.FCost());
            
            if (compare == 0)
            {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }

            return -compare;
        }
    }
}
