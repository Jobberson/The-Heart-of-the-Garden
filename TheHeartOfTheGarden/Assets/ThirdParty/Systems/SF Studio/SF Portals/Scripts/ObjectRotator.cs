using UnityEngine;

namespace SpaceFusion.common.Scripts
{
    public class ObjectRotator : MonoBehaviour
    {
        public float rotationSpeed;
        public bool rotateX;
        public bool rotateY;
        public bool rotateZ;
        private void Update()
        {
            float normalizedRotation = rotationSpeed * Time.deltaTime;
            transform.Rotate(
                rotateX?normalizedRotation:0,
                rotateY?normalizedRotation:0,
                rotateZ?normalizedRotation:0,
                Space.World
                );
        }
    }
}