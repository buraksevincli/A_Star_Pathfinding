using ClashOfClans.Cores;
using UnityEngine;

namespace ClashOfClans.Pathfinding
{
    public struct Line
    {
        private const float VerticalLineGradient = 1e5f;
        
        private float _gradient;
        private float _yIntercept;
        private float _gradientPerpendicular;

        private Vector2 pointOnLine1, pointOnLine2;
        private bool _approachSide;

        public Line(Vector2 pointOnline, Vector2 pointPerpendicularToLine)
        {
            float distanceX = pointOnline.x - pointPerpendicularToLine.x;
            float distanceY = pointOnline.y - pointPerpendicularToLine.y;

            if (distanceX == 0)
            {
                _gradientPerpendicular = VerticalLineGradient;
            }
            else
            {
                _gradientPerpendicular = distanceY / distanceX;
            }

            if (_gradientPerpendicular == 0)
            {
                _gradient = VerticalLineGradient;
            }
            else
            {
                _gradient = -1 / _gradientPerpendicular;
            }

            _yIntercept = pointOnline.y - _gradient * pointOnline.x;
            
            pointOnLine1 = pointOnline;
            pointOnLine2 = pointOnline + new Vector2(1, _gradient);

            _approachSide = false;
            _approachSide = GetSide(pointPerpendicularToLine);
        }

        private bool GetSide(Vector2 point)
        {
            return (point.x - pointOnLine1.x) * (pointOnLine2.y - pointOnLine1.y) >
                   (point.y - pointOnLine1.y) * (pointOnLine2.x - pointOnLine1.x);
        }

        public bool HasCrossedLine(Vector2 point)
        {
            return GetSide(point) != _approachSide;
        }

        public float DistanceFromPoint(Vector2 point)
        {
            float yInterceptPerpendicular = point.y - _gradientPerpendicular * point.x;
            float intersectX = (yInterceptPerpendicular - _yIntercept) / (_gradient - _gradientPerpendicular);
            float intersectY = _gradient * intersectX + _yIntercept;

            return Vector2.Distance(point, new Vector2(intersectX, intersectY));
        }

        public void DrawWithGizmos(float lenght)
        {
            Vector3 lineDirection = new Vector3(1, 0, _gradient).normalized;
            Vector3 lineCenter = new Vector3(pointOnLine1.x, 0, pointOnLine1.y) + VectorHelper.Up;

            Gizmos.DrawLine(lineCenter - lineDirection * lenght / 2f, lineCenter + lineDirection * lenght / 2f);
        }
    }
}
