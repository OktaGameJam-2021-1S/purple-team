using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class MonoBehaviourTrigger : MonoBehaviour
    {
        public Action<Collider> OnTriggerEnterEvent;
        public Action<Collider> OnTriggerStayEvent;
        public Action<Collider> OnTriggerExitEvent;
        public Action UpdateEvent;
        public Action OnExitEvent;

        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEnterEvent?.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            OnTriggerStayEvent?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            OnTriggerExitEvent?.Invoke(other);
        }

        private void Update()
        {
            UpdateEvent?.Invoke();
        }
        private void OnApplicationQuit()
        {
            OnExitEvent?.Invoke();
        }

        public void DestroyObject(GameObject gameObject)
        {
            Destroy(gameObject);
        }

        public static MonoBehaviourTrigger CreateMonoBehaviourTrigger(bool dontDestroyOnload = false)
        {
            GameObject trigger = new GameObject("MonoBehaviourTrigger");

            if(dontDestroyOnload) GameObject.DontDestroyOnLoad(trigger);

            return trigger.AddComponent<MonoBehaviourTrigger>();
        }
    }
}
