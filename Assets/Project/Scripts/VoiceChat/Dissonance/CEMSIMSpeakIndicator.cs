using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Dissonance;
using CEMSIM.GameLogic;
using System;

namespace CEMSIM
{
    namespace VoiceChat
    {
        public class CEMSIMSpeakIndicator
            : MonoBehaviour
        {
            [Tooltip("The object based on which the indicator is located. E.g. avatar username.")]
            public Transform baseObjectTransform;
            private GameObject _indicator;
            private Light _light;
            private Transform _transform;

            private float _intensity;

            private IDissonancePlayer _player;
            private VoicePlayerState _state;
            private bool isSpeaking_prev = false;

            public static event Action<string> onPlayerStartSpeaking;
            public static event Action<string> onPlayerStopSpeaking;

            private bool IsSpeaking
            {
                //get { return _player.Type == NetworkPlayerType.Remote && _state != null && _state.IsSpeaking; }
                get { return _state != null && _state.IsSpeaking; } // also display for local player
            }

            private void OnEnable()
            {
                //Get some bits from the indicator game object
                _indicator = Instantiate(Resources.Load<GameObject>("SpeechIndicator")); // load a preset gameobject
                _indicator.transform.SetParent(baseObjectTransform);
                _indicator.transform.localPosition = new Vector3(0, ClientGameConstants.SPEAK_INDICATOR_HEIGHT_OFFSET, 0);

                _light = _indicator.GetComponent<Light>();
                _transform = _indicator.GetComponent<Transform>();

                //Find the component attached to this game object which marks it as a Dissonance player representation
                _player = GetComponent<IDissonancePlayer>();

                StartCoroutine(FindPlayerState());
            }

            private void OnDisable()
            {
                StopAllCoroutines();
            }

            private IEnumerator FindPlayerState()
            {
                //Wait until player tracking has initialized
                while (!_player.IsTracking)
                    yield return null;

                //Now ask Dissonance for the object which represents the state of this player
                //The loop is necessary in case Dissonance is still initializing this player into the network session
                while (_state == null)
                {
                    _state = FindObjectOfType<DissonanceComms>().FindPlayer(_player.clientuuid);
                    yield return null;
                }
            }


            private void Update()
            {
                //Debug.Log($"player type {_player.PlayerId} - {_player.Type}, Remote = {NetworkPlayerType.Remote}");
                //Debug.Log($"player state is null?{_state == null} is speaking? {IsSpeaking}");
                if (IsSpeaking)
                {
                    //Calculate intensity of speech - do the pow to visually boost the scale at lower intensities
                    _intensity = Mathf.Max(Mathf.Clamp(Mathf.Pow(_state.Amplitude, 0.175f), 0.25f, 1) * 0.25f, _intensity - Time.unscaledDeltaTime);
                    _indicator.SetActive(true);
                    if(!isSpeaking_prev)
                        StartSpeakingTrigger(_player.clientuuid);
                }
                else
                {
                    //Fade out intensity when player is not talking
                    _intensity -= Time.unscaledDeltaTime * 2;

                    if (_intensity <= 0)
                        _indicator.SetActive(false);
                    if (isSpeaking_prev)
                        StopSpeakingTrigger(_player.clientuuid);
                    
                }
                isSpeaking_prev = IsSpeaking;

                UpdateLight(_light, _intensity);
                UpdateChildTransform(_transform, _intensity);
            }

            private static void UpdateChildTransform([NotNull] Transform transform, float intensity)
            {
                transform.localScale = new Vector3(intensity, intensity, intensity);
            }

            private static void UpdateLight([NotNull] Light light, float intensity)
            {
                light.intensity = intensity;
            }

            public void UpdateState()
            {
                StartCoroutine(FindPlayerState());
            }


            #region Event system
            public void StartSpeakingTrigger(string _playerUId)
            {
                //Debug.Log($"{_playerUId} starts speaking");
                if (onPlayerStartSpeaking != null)
                    onPlayerStartSpeaking(_playerUId);
            }

            public void StopSpeakingTrigger(string _playerUId)
            {
                //Debug.Log($"{_playerUId} stops speaking");
                if (onPlayerStopSpeaking != null)
                    onPlayerStopSpeaking(_playerUId);
            }

            #endregion

        }
    }
}