using ClashOfClans.Cores;
using UnityEngine;

namespace ClashOfClans.Pathfinding
{
    public class Path
    {
        public readonly Vector3[] lookPoints;
        public readonly Line[] turnBoundaries;
        public readonly int finishLineIndex;
        public readonly int slowDownIndex;

        public Path(Vector3[] wayPoints, Vector3 startPosition, float turnDistance, float stoppingDistance)
        {
            lookPoints = wayPoints;
            turnBoundaries = new Line[lookPoints.Length];
            finishLineIndex = turnBoundaries.Length - 1;

            Vector2 previousPoint = Vector3ToVector2(startPosition);

            for (int i = 0; i < lookPoints.Length; i++)
            {
                Vector2 currentPoint = Vector3ToVector2(lookPoints[i]);
                Vector2 directionToCurrentPoint = (currentPoint - previousPoint).normalized;
                Vector2 turnBoundaryPoint = (i == finishLineIndex)
                    ? currentPoint
                    : currentPoint - directionToCurrentPoint * turnDistance;
                turnBoundaries[i] = new Line(turnBoundaryPoint, previousPoint - directionToCurrentPoint * turnDistance);

                previousPoint = turnBoundaryPoint;
            }

            float distanceFromEndPoint = 0;
            for (int i = lookPoints.Length - 1; i > 0; i--)
            {
                distanceFromEndPoint += Vector3.Distance(lookPoints[i], lookPoints[i - 1]);
                if (distanceFromEndPoint > stoppingDistance)
                {
                    slowDownIndex = i;
                    break;
                }
            }
        }

        private Vector2 Vector3ToVector2(Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }

        public void DrawWithGizmos()
        {
            Gizmos.color = Color.black;
            foreach (Vector3 point in lookPoints)
            {
                Gizmos.DrawCube(point + VectorHelper.Up, VectorHelper.One);
            }

            Gizmos.color = Color.white;
            foreach (Line line in turnBoundaries)
            {
                line.DrawWithGizmos(10);
            }
        }
    }
}