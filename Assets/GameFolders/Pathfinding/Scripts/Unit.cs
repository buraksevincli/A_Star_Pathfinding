using System.Collections;
using ClashOfClans.Cores;
using UnityEngine;

namespace ClashOfClans.Pathfinding
{
    public class Unit : MonoBehaviour
    {
        private const float MinPathUpdateTime = .2f;
        private const float PathUpdateMoveThreshold = .5f;
        
        public Transform target;
        public float speed = 20;
        public float turnDistance = 5;
        public float turnSpeed = 3;
        public float stoppingDistance = 10;

        private Path _path;
        private Transform _transform;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        private void Start()
        {
            StartCoroutine(UpdatePath());
        }

        private void OnPathFound(Vector3[] wayPoints, bool pathSuccessful)
        {
            if (pathSuccessful)
            {
                _path = new Path(wayPoints, _transform.position, turnDistance, stoppingDistance);
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
        }

        private IEnumerator UpdatePath()
        {
            if (Time.timeSinceLevelLoad < .3f)
            {
                yield return new WaitForSeconds(.3f);
            }
            
            PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
            
            float squareMoveThreshold = PathUpdateMoveThreshold * PathUpdateMoveThreshold;
            Vector3 targetOldPosition = target.position;
            
            while (true)
            {
                yield return new WaitForSeconds(MinPathUpdateTime);
                if ((target.position - targetOldPosition).sqrMagnitude > squareMoveThreshold)
                {
                    PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));

                    targetOldPosition = target.position;
                }
            }
        }

        private IEnumerator FollowPath()
        {
            bool followingPath = true;
            int pathIndex = 0;
            _transform.LookAt(_path.lookPoints[0]);

            float speedPercent = 1;
            
            while (followingPath)
            {
                Vector2 position2D = new Vector2(_transform.position.x, _transform.position.z);
                while (_path.turnBoundaries[pathIndex].HasCrossedLine(position2D))
                {
                    if (pathIndex == _path.finishLineIndex)
                    {
                        followingPath = false;
                        break;
                    }
                    else
                    {
                        pathIndex++;
                    }
                }

                if (followingPath)
                {
                    if (pathIndex >= _path.slowDownIndex && stoppingDistance > 0)
                    {
                        speedPercent = Mathf.Clamp01(_path.turnBoundaries[_path.finishLineIndex].DistanceFromPoint(position2D) / stoppingDistance);
                        if (speedPercent < .01f)
                        {
                            followingPath = false;
                        }
                    }
                    
                    Quaternion targetRotation =
                        Quaternion.LookRotation(_path.lookPoints[pathIndex] - _transform.position);

                    transform.rotation =
                        Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

                    transform.Translate(VectorHelper.Forward * (Time.deltaTime * speed * speedPercent), Space.Self);
                }
                
                yield return null;
            }
        }

        public void OnDrawGizmos()
        {
            if (_path != null)
            {
                _path.DrawWithGizmos();
            }
        }
    }
}
