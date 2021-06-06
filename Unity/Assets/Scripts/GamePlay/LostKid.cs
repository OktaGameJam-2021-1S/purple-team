using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Utils;

namespace GamePlay
{
    public class LostKid : MonoBehaviourPun, IPunObservable
    {
        [SerializeField] private AudioSource _loopCry;
        [SerializeField] private AudioSource _farAwayCry;
        [SerializeField] private float _timeFarCry = 30;

        public bool CanBeTaked => _currentGrabbingPlayer == null;

        private List<PlayerController> _players;
        private PlayerController _currentGrabbingPlayer = null;

        public void Initialize(List<PlayerController> players)
        {
            _players = players;
            _loopCry.Play();

            StartCoroutine(FarAwayCry());
        }

        private IEnumerator FarAwayCry()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(_timeFarCry);
            
            while(true)
            {
                yield return waitForSeconds;

                if (_currentGrabbingPlayer == null)
                {
                    _farAwayCry.Play();
                }
            }
        }

        public void TakeKid(PlayerController playerController)
        {
            photonView.RPC("RequesTakeKid", RpcTarget.MasterClient, playerController.photonView.ViewID);
        }

        public void DropKid(PlayerController playerController)
        {
            photonView.RPC("RequestDropKid", RpcTarget.MasterClient, playerController.photonView.ViewID);
        }

        [PunRPC]
        public void RequesTakeKid(int viewID)
        {
            if (_currentGrabbingPlayer == null)
            {
                SyncKid(viewID);
            }
        }

        [PunRPC]
        public void RequestDropKid(int viewID)
        {
            if (_currentGrabbingPlayer != null && _currentGrabbingPlayer.photonView.ViewID == viewID)
            {
                SyncKid(-1);               
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_currentGrabbingPlayer != null? _currentGrabbingPlayer.photonView.ViewID : -1);
            }
            else
            {
                int grabbingPlayerViewID = (int) stream.ReceiveNext();
                SyncKid(grabbingPlayerViewID);
            }
        }

        private void SyncKid(int viewID)
        {
            if (_currentGrabbingPlayer != null && _currentGrabbingPlayer.photonView.ViewID != viewID)
            {
                _currentGrabbingPlayer.DropKid(this);
                _currentGrabbingPlayer = null;
                Utilities.ChangeObjectLayer(gameObject, LayerMask.NameToLayer("Character"));
            }

            if (viewID != -1)
            {
                _currentGrabbingPlayer = _players.Find((PlayerController p) => p.photonView.ViewID == viewID);
                _currentGrabbingPlayer.TakeKid(this);
                Utilities.ChangeObjectLayer(gameObject, _currentGrabbingPlayer.gameObject.layer);
            }

            if (_currentGrabbingPlayer == null)
            {
                _loopCry.Play();
            }
            else
            {
                _loopCry.Stop();
            }
        }
    }
}
