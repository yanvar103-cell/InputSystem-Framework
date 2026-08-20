using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;

        private List<Rigidbody> _brakeOff = new List<Rigidbody>();

        private void OnEnable()
        {
            InteractableZone.onHoldStarted += InteractableZone_onHoldStarted;// still handles first-time reveal

            InteractableZone.onHoldEnded += OnHoldEnded;//destruction will call here which takes into account the variable _duration
        }

        private void OnDisable()
        {
            InteractableZone.onHoldStarted -= InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded -= OnHoldEnded;
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);

        }

        private void InteractableZone_onHoldStarted(int _zoneID)
        {
            if (_zoneID != _interactableZone.GetZoneID()) return;

            if (_isReadyToBreak == false && _brakeOff.Count >0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }
        }

        /*private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {

            if (_isReadyToBreak == false && _brakeOff.Count > 0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone            
            {
                if (_brakeOff.Count > 0)
                {
                    BreakPart();
                    StartCoroutine(PunchDelay());
                }
                else if (_brakeOff.Count == 0)
                {
                    _isReadyToBreak = false;
                    _crateCollider.enabled = false;
                    _interactableZone.CompleteTask(6);
                    Debug.Log("Completely Busted");
                }
            }
        }*/

        private void OnHoldEnded(int _zoneID, float _duration)//new method, destruction will call here which takes into account the variable _duration
        {
            if (_zoneID != _interactableZone.GetZoneID() || !_isReadyToBreak) return;//Crate zone

            if (_brakeOff.Count > 0)
            {
                int _piecesToBreak = CalculatePiecesToBreak(_duration);//force calculation according to duration
                for (int i = 0; i < _piecesToBreak && _brakeOff.Count > 0; i++)
                {
                    BreakPart();
                }
                StartCoroutine(PunchDelay());
            }
            else if (_brakeOff.Count == 0)
            {
                _isReadyToBreak = false;
                _crateCollider.enabled = false;
                _interactableZone.CompleteTask(6);
                Debug.Log("Completely Busted");
            }
        }

        public void BreakPart()//added new _forceMultiplier
        {
            int rng = Random.Range(0, _brakeOff.Count);
            _brakeOff[rng].constraints = RigidbodyConstraints.None;
            _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Impulse);
            _brakeOff.Remove(_brakeOff[rng]);
        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }

        private int CalculatePiecesToBreak(float _duration)
        {
            float _holdThreshold = 0.3f;
            float _piecesPerSecond = 2f; // how many extra pieces per second held

            if (_duration < _holdThreshold)
                return 1; // tap = break only 1 piece
            else
            {
                int _pieces = 1 + Mathf.FloorToInt(_duration * _piecesPerSecond);
                return Mathf.Min(_pieces, _brakeOff.Count);//capped at remaining count
            }
        }
    }
}
