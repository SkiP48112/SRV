using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Code
{
    public class SystemManager : MonoBehaviour
    {
        [SerializeField] private Arg _arg;
        [SerializeField] private float _rotationSpeed = 0.3f;
        [SerializeField] private float _rotationVelocity = 0.001f;
        [SerializeField] private int _rotationDirection = 1;
        [SerializeField] private float _positiveRotationTimer = 10;
        [SerializeField] private float _negativeRotationTimer = 10;
        
        [SerializeField] private float _temperatureModulationSpeed = 0.001f;
        
        [SerializeField] private float _currentTemperature;
        [SerializeField] private float _calculatedTemperature;
        [SerializeField] private float _minimalTemperature;

        [SerializeField] private List<float> _checkTimings;

        [Header("UI")] 
        [SerializeField] private TextMeshProUGUI _globalTime;
        [SerializeField] private TextMeshProUGUI _currentTemperatureLabel;
        [SerializeField] private TextMeshProUGUI _calculatedTemperatureLabel;
        [SerializeField] private TextMeshProUGUI _maxTemperatureLabel;

        [Header("Temperature Modulation")] [SerializeField]
        private float _controlTemperature = 95;
        [SerializeField, Range(0f, 1f)] private float _temperatureModulation = 0.51f;
        private float _currentTimer;
        private List<float> _timers;
        private List<Sensor> _sensors;
        private bool _downTemperature = false;
        private float _currentRotationSpeed;
        private float _maxTemperature;

        private void Start()
        {
            _currentTimer = 0;
            _currentRotationSpeed = _rotationSpeed;
            
            _timers = _checkTimings.ToList();
            _sensors = _arg.GetSensors();
        }

        private void Update()
        {
            if (!IsCrashed())
            {
                CheckSensors();
            }
            else if(!_downTemperature)
            {
                foreach (var sensor in _sensors) 
                {
                    sensor.ResetSensor();
                }
                
                _temperatureModulation = 0.47f;
                _downTemperature = true;
                
                SetTimeScale(1f);
                StopAllCoroutines();
                StartCoroutine(SlowDown(true));
            }
            
            RotateDrum();
            ModulateTemperature();
            UpdateUI();
        }

        private bool IsCrashed()
        {
            return _maxTemperature > _controlTemperature;
        }
        
        private void ModulateTemperature()
        {
            var direction = Random.Range(0f, 100f) / 100f;
            if (direction < _temperatureModulation)
            {
                direction = 1;
            }
            else if(_currentTemperature > _minimalTemperature)
            {
                direction = -1;
            }
            else
            {
                direction = 1;
            }

            var temp = Random.Range(0f, 100f) *_temperatureModulationSpeed;
            _currentTemperature += temp * Math.Sign(direction);
        }

        private void RotateDrum()
        {
            var drum = _arg.GetDrum();
            var rotation = drum.transform.localRotation.eulerAngles;
            rotation.y += _currentRotationSpeed * _rotationDirection * Time.timeScale;
            _arg.GetDrum().transform.localRotation = Quaternion.Euler(rotation);

            if (!IsCrashed()) 
            {
                _currentTimer += Time.deltaTime;
            }


            var timer = _rotationDirection > 0 ? _positiveRotationTimer : _negativeRotationTimer;
            
            if (_currentTimer > timer && !IsCrashed())
            {
                _currentTimer = 0;
                StartCoroutine(SlowDown());
            }
        }

        private IEnumerator SlowDown(bool fullStop = false)
        {
            if (_currentRotationSpeed < 0)
            {
                _rotationDirection *= -1;
                if (!fullStop)
                {
                    StartCoroutine(SpeedUp());
                }

                yield break;
            }

            _currentRotationSpeed -= _rotationVelocity;
            yield return new WaitForSeconds(0.01f);
            StartCoroutine(SlowDown(fullStop));
        }

        private IEnumerator SpeedUp()
        {
            if (_currentRotationSpeed > _rotationSpeed)
            {
                 yield break;;
            }

            _currentRotationSpeed += _rotationVelocity;
            yield return new WaitForSeconds(0.01f);
            StartCoroutine(SpeedUp());
        }

        private void UpdateUI()
        {
            _globalTime.text = $"Глобальное время: {(int)Time.timeSinceLevelLoad}";
            _currentTemperatureLabel.text = $"Температура: {(int)_currentTemperature}C";
            _calculatedTemperatureLabel.text = $"Температура с датчиков: {(int)_calculatedTemperature}C";
            _maxTemperatureLabel.text = $"Максимальная температура: {(int)_maxTemperature}C";
        }

        private void UpdateMaxTemperature()
        {
            if (_calculatedTemperature > _maxTemperature)
            {
                _maxTemperature = _calculatedTemperature;
            }
        }

        private void CheckSensors()
        {
            if (_calculatedTemperature > _controlTemperature)
            {
                return;
            }
            
            for (var i = 0; i < _timers.Count; i++)
            {
                _timers[i] -= Time.deltaTime;
            }

            if (_timers[0] <= 0)
            {
                _timers[0] = _checkTimings[0];
                _sensors[0].CheckSensor();
                _sensors[2].CheckSensor();
                _calculatedTemperature = _currentTemperature;
                UpdateMaxTemperature();
            }
            
            if (_timers[1] <= 0)
            {
                _timers[1] = _checkTimings[1];
                _sensors[3].CheckSensor();
                _sensors[4].CheckSensor();
                _calculatedTemperature = _currentTemperature;
                UpdateMaxTemperature();
            }
            
            if (_timers[2] <= 0)
            {
                _timers[2] = _checkTimings[2];
                _sensors[1].CheckSensor();
                _sensors[7].CheckSensor();
                _calculatedTemperature = _currentTemperature;
                UpdateMaxTemperature();
            }
            
            if (_timers[3] <= 0)
            {
                _timers[3] = _checkTimings[3];
                _sensors[5].CheckSensor();
                _sensors[6].CheckSensor();
                _calculatedTemperature = _currentTemperature;
                UpdateMaxTemperature();
            }
        }

        public void ResetSystem()
        {
            _calculatedTemperature = _currentTemperature;
            _maxTemperature = _calculatedTemperature;
            _temperatureModulation = 0.51f;
            _downTemperature = false;
            
            SetTimeScale(1f);
            StopAllCoroutines();
            StartCoroutine(SpeedUp());
        }

        public void SetTimeScale(float timeScale)
        {
            Time.timeScale = timeScale;
        }

    }

    [CustomEditor(typeof(SystemManager))]
    public class SystemManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            SystemManager systemManager = (SystemManager) target;

            if (GUILayout.Button("Reset System"))
            {
                systemManager.ResetSystem();
            }

            if (GUILayout.Button("Time scale x1"))
            {
                systemManager.SetTimeScale(1f);
            }

            if (GUILayout.Button("Time scale x5"))
            {
                systemManager.SetTimeScale(5f);
            }
            
            if (GUILayout.Button("Time scale x10"))
            {
                systemManager.SetTimeScale(10f);
            }
        }
    }
}