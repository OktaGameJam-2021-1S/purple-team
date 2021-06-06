using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Utils;

namespace GamePlay
{
    public class PlayerController : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] private MovementController _movementController;
        [SerializeField] private PlayerLightController _playerLightController;

        public bool HasKid => _carringKid != null;

        private LostKid _carringKid = null;
        private LostKid _closeKid = null;        

        /// <summary>
        /// Initialize the Player Controller
        /// </summary>
        public void Initialize(bool isLocalPlayer)
        {
            Utilities.ChangeObjectLayer(gameObject, LayerMask.NameToLayer((isLocalPlayer ? "MainCharacter" : "Character")));
            _playerLightController.Initialize(isLocalPlayer);
            _movementController.Initialize();
        }       

        /// <summary>
        /// Process player input
        /// </summary>
        /// <param name="axis">Input axis</param>
        public void ProcessInput(Vector2 axis, bool interactButton)
        {
            _movementController.UpdateAxis(axis);
            if (interactButton && _carringKid)
            {
                _carringKid.DropKid(this);
            }
            else if (interactButton && _closeKid != null && _closeKid.CanBeTaked)
            {
                _closeKid.TakeKid(this);
            }
            else
            {
                _playerLightController.ProcessInput(interactButton);
            }
        }

        public void UpdatePlayer(float deltaTime)
        {
            _playerLightController.UpdateLight(deltaTime);
        }

        public void UpdatePlayerPosition(float deltaTime)
        {
            _movementController.UpdatePosition(deltaTime);
        }

        public void TakeKid(LostKid kid)
        {
            _playerLightController.TurnOffLight();
            
            _carringKid = kid;
            _carringKid.transform.SetParent(transform);
            _carringKid.transform.localPosition = Vector3.up * 2;
            _carringKid.transform.localRotation = Quaternion.identity;
        }

        public void DropKid(LostKid kid)
        {
            _playerLightController.TurnOnLight();

            _carringKid = null;
            kid.transform.SetParent(null);
            kid.transform.position = transform.position + transform.forward;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_playerLightController.IsLightOn);
                stream.SendNext(_playerLightController.CurrentLightPower);
                stream.SendNext(PhotonNetwork.ServerTimestamp);
            }
            else
            {
                bool isLightOn = (bool) stream.ReceiveNext();
                float lightPower = (float) stream.ReceiveNext();
                int sendTimeStamp = (int) stream.ReceiveNext();

                float deltaTime = Utilities.DeltaTime(sendTimeStamp, PhotonNetwork.ServerTimestamp);

                _playerLightController.SyncLight(isLightOn, lightPower, deltaTime);
            }
        }

        #region Unity Events

        //DEBUG::: Debug initialization
        private void Start()
        {            
            Initialize(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            LostKid kid = other.GetComponent<LostKid>();
            if (kid != null)
            {
                _closeKid = kid;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_closeKid != null && other.gameObject == _closeKid.gameObject)
            {
                _closeKid = null;
            }
        }
        #endregion
    }
}
