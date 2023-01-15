using UnityEngine;

namespace Code
{
    public class Sensor : MonoBehaviour
    {
        [SerializeField] private Material _enabled;
        [SerializeField] private Material _disabled;

        private Material _material;

        private void Start()
        {
            _material = GetComponent<MeshRenderer>().material;
        }

        public void CheckSensor()
        {
            _material.color = _material.color == _disabled.color ? _enabled.color : _disabled.color;
        }

        public void ResetSensor()
        {
            _material.color = _disabled.color;
        }
    }
}