using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class FollowObject : MonoBehaviour
    {
        public enum UpdateMethod
        {
            Update,
            LateUpdate,
            FixedUpdate,
            Manual,
        }

        [SerializeField] protected Transform _target; // Current target to follow
        [SerializeField] protected Vector3 _offset; // offset in unity coordinates
        [SerializeField] private UpdateMethod _updateMethod = UpdateMethod.Update; // inform in which method the position should be updated
        [SerializeField] private bool _useLocalOffset;
        [SerializeField] private bool _copyRotation = false;
        [SerializeField] private bool _removeParent = false;
        [SerializeField] private bool _destroyWithTarget = false;

        public Vector3 OffSet { get { return _offset; } }
        public Transform Target { get { return _target; } }

        private bool _hasTarget = false;

        #region UNITY
        private void Start()
        {
            if (_target != null)
            {
                _hasTarget = true;
                if (_useLocalOffset)
                {
                    _offset = transform.position - _target.position;
                }
            }

            if (_removeParent)
            {
                transform.SetParent(null);
            }
        }
        private void Update()
        {
            if (_target && _updateMethod == UpdateMethod.Update)
            {
                UpdateObject();
            }

            if (_target == null && _hasTarget && _destroyWithTarget)
            {
                Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
            if (_target && _updateMethod == UpdateMethod.LateUpdate)
            {
                UpdateObject();
            }
        }

        private void FixedUpdate()
        {
            if (_target && _updateMethod == UpdateMethod.FixedUpdate)
            {
                UpdateObject();
            }
        }
        #endregion

        /// <summary>
        /// Make the follow to update position and rotation now
        /// </summary>
        public void ForceUpdate()
        {
            UpdateObject();
        }

        public virtual void SetUpdateMethod(UpdateMethod updateMethod)
        {
            _updateMethod = updateMethod;
        }

        public virtual void SetTarget(Transform newTarget)
        {
            _target = newTarget;
            _hasTarget = _target != null;
        }

        public virtual void SetOffset(Vector3 newOffset)
        {
            _offset = newOffset;
        }

        public virtual void SetCopyRotation(bool copyRotation)
        {
            _copyRotation = copyRotation;
        }

        protected virtual void UpdateObject()
        {
            UpdatePosition();
            if (_copyRotation)
            {
                UpdateRotation();
            }
        }

        protected virtual void UpdatePosition()
        {
            transform.position = _target.position + _offset;
        }

        protected virtual void UpdateRotation()
        {
            transform.rotation = _target.rotation;
        }
    }
}
