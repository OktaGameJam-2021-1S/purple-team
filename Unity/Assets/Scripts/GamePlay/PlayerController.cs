using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Utils;

namespace GamePlay
{
    public class PlayerController : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] private Transform _kidBoneRoot;
        [SerializeField] private Animator _animator;
        [SerializeField] private MovementController _movementController;
        [SerializeField] private PlayerLightController _playerLightController;
        [SerializeField] private Renderer m_Renderer;

        public string UserID = "";

        public bool HasKid => _carringKid != null;

        private LostKid _carringKid = null;
        private LostKid _closeKid = null;

        private bool isLocalPlayer;

        /// <summary>
        /// Initialize the Player Controller
        /// </summary>
        public void Initialize(bool isLocalPlayer)
        {
            this.isLocalPlayer = isLocalPlayer;
            Utilities.ChangeObjectLayer(gameObject, LayerMask.NameToLayer((isLocalPlayer ? "MainCharacter" : "Character")));
            if(isLocalPlayer)
            {
                var audioListener = FindObjectOfType<AudioListener>();
                audioListener.transform.SetParent(transform);
                audioListener.transform.localPosition = Vector3.zero;
            }
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

        public void UpdatePlayerPosition(float deltaTime)
        {
            _movementController.UpdatePosition(deltaTime);
        }

        public void UpdatePlayer(float deltaTime)
        {
            _playerLightController.UpdateLight(deltaTime);
        }

        public void UpdatePlayerAnimation()
        {
            _animator.SetFloat("Speed", _movementController.Velocity.magnitude / _movementController.MaxSpeed);
            _animator.SetBool("HasKid", HasKid);
            _animator.SetBool("HasLamp", _playerLightController.CurrentLightPower > 0);
        }

        public void TakeKid(LostKid kid)
        {
            _playerLightController.TurnOffLight();
            
            _carringKid = kid;
            _carringKid.transform.SetParent(_kidBoneRoot);
            _carringKid.transform.localPosition = new Vector3(0, 0, -0.2f);
            _carringKid.transform.localRotation = Quaternion.Euler(60, 0, 0);
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

        public void InitColors(bool primary)
        {
            if (primary)
                m_Renderer.material.SetTextureOffset("_BaseMap", new Vector2(0, 0));
            else
                m_Renderer.material.SetTextureOffset("_BaseMap", new Vector2(0.0f, 0.88f));
        }

        #region Unity Events
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
