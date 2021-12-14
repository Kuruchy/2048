using UnityEngine;

namespace Core {
    public class SwipeManager : MonoBehaviour {
        public float minSwipeLength = 200f;
        private Vector2 _currentSwipe;

        private Vector2 _fingerStart;
        private Vector2 _fingerEnd;

        public static Swipes Direction;

        private void Update() {
            SwipeDetection();
        }

        private void SwipeDetection() {
            if (Input.GetMouseButtonDown(0)) {
                _fingerStart = Input.mousePosition;
                _fingerEnd = Input.mousePosition;
            }

            if (Input.GetMouseButton(0)) {
                _fingerEnd = Input.mousePosition;

                _currentSwipe = new Vector2(_fingerEnd.x - _fingerStart.x, _fingerEnd.y - _fingerStart.y);

                // Make sure it was a legit swipe, not a tap
                if (_currentSwipe.magnitude < minSwipeLength) {
                    Direction = Swipes.None;
                    return;
                }

                var angle = Mathf.Atan2(_currentSwipe.y, _currentSwipe.x) / Mathf.PI;

                if (angle > 0.375f && angle < 0.625f) {
                    Direction = Swipes.Up;
                } else if (angle < -0.375f && angle > -0.625f) {
                    Direction = Swipes.Down;
                } else if (angle < -0.875f || angle > 0.875f) {
                    Direction = Swipes.Left;
                } else if (angle > -0.125f && angle < 0.125f) {
                    Direction = Swipes.Right;
                } else if (angle > 0.125f && angle < 0.375f) {
                    Direction = Swipes.TopRight;
                } else if (angle > 0.625f && angle < 0.875f) {
                    Direction = Swipes.TopLeft;
                } else if (angle < -0.125f && angle > -0.375f) {
                    Direction = Swipes.BottomRight;
                } else if (angle < -0.625f && angle > -0.875f) {
                    Direction = Swipes.BottomLeft;
                }
            }

            if (Input.GetMouseButtonUp(0)) {
                Direction = Swipes.None;
            }
        }
    }
}