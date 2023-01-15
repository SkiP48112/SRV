using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    public class Arg : MonoBehaviour
    {
        [SerializeField] private Drum _drum;
        [SerializeField] private List<Sensor> _sensors;

        public Drum GetDrum()
        {
            return _drum;
        }

        public List<Sensor> GetSensors()
        {
            return _sensors;
        }
    }
}