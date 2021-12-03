using UnityEngine;

namespace Core {
    public class Node : MonoBehaviour {
        public Vector2 Pos => transform.position;
        public Block occupiedBlock;
    }
}