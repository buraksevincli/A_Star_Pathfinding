using UnityEngine;

namespace ClashOfClans.Cores
{
    public class VectorHelper : MonoBehaviour
    {
        public static Vector3 Right { get; }
        public static Vector3 Forward { get; }
        public static Vector3 One { get; }
        public static Vector3 Up { get; }
        public static Vector3 Down { get; }

        static VectorHelper()
        {
            Right = Vector3.right;
            Forward = Vector3.forward;
            One = Vector3.one;
            Up = Vector3.up;
            Down = Vector3.down;
        }
    }
}