using UnityEngine;

namespace Core {
    public class SwipeManager : MonoBehaviour {
        public float minSwipeLength = 200f;
        private Vector2 _currentSwipe;

        private Vector2 _fingerStart;
        private Vector2 _fingerEnd;

        private float _angle;
        
        public static Swipes Direction;

        private void Update() {
            SwipeDetection();
        }

        private void SwipeDetection() {
            if (Input.GetMouseButtonDown(0)) {
                Clear();
                _fingerStart = Input.mousePosition;
                _fingerEnd = Input.mousePosition;
            }

            if (Input.GetMouseButton(0)) {
                _fingerEnd = Input.mousePosition;

                _currentSwipe = new Vector2(_fingerEnd.x - _fingerStart.x, _fingerEnd.y - _fingerStart.y);

                _angle = Mathf.Atan2(_currentSwipe.y, _currentSwipe.x) / Mathf.PI;
            }

            // Make sure it was a legit swipe, not a tap
            if (_currentSwipe.magnitude < minSwipeLength) {
                Clear();
                return;
            }
            
            if (!Input.GetMouseButtonUp(0)) return;
            
            if (_angle > 0.25f && _angle < 0.75f) {
                Direction = Swipes.Up;
            } else if (_angle < -0.25f && _angle > -0.75f) {
                Direction = Swipes.Down;
            } else if (_angle < -0.75f || _angle > 0.75f) {
                Direction = Swipes.Left;
            } else if (_angle > -0.25f && _angle < 0.25f) {
                Direction = Swipes.Right;
            }
        }

        public static void Clear() => Direction = Swipes.None;
    }
}