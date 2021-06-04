using System;
using System.Collections.Generic;

namespace Utils
{
    public class ObjectPool<T>
    {
        /// <summary>
        /// List with all objects that are currently pooled
        /// </summary>
        public List<T> BusyObjects => _busyObjects;

        private List<T> _availableObjects; // Objects that are available to be used
        private List<T> _busyObjects; // Objects in the pool that are currently not available

        private Func<T> ObjectGenerator; // Function used to generate new objects for the pool when there is no object available
        private Func<T, bool> CheckAvailabilityFunc; // Function used to check when an object in the busy list became available

        /// <summary>
        /// Create a new pool of given type object
        /// </summary>
        /// <param name="initialQuantity">The initial number of elements in the pool</param>
        /// <param name="objectGenerator">Function used to generate new objects for the pool when there is no object available</param>
        /// <param name="checkAvailabilityFunc">Function used to check when an object in that was not available became available</param>
        public ObjectPool(int initialQuantity, Func<T> objectGenerator, Func<T, bool> checkAvailabilityFunc)
        {
            _availableObjects = new List<T>(initialQuantity);
            _busyObjects = new List<T>(initialQuantity);

            ObjectGenerator = objectGenerator;
            CheckAvailabilityFunc = checkAvailabilityFunc;

            for (int i = 0; i < initialQuantity; i++)
            {
                _availableObjects.Add(ObjectGenerator());
            }
        }

        /// <summary>
        /// Get a object from the pool
        /// </summary>
        /// <returns></returns>
        public T PullObject()
        {
            // If there is no object available, then clear the busy list
            if (_availableObjects.Count == 0)
            {
                ClearBusyObjects();

                // If there is no object available, then create a new one for the pool
                if (_availableObjects.Count == 0)
                {
                    _availableObjects.Add(ObjectGenerator());
                }
            }

            // Remove object from available list and put it into the busy list
            T obj = _availableObjects[0];
            _busyObjects.Add(obj);
            _availableObjects.RemoveAt(0);

            return obj;
        }

        /// <summary>
        /// Get a currently busy object into the pool using the check function
        /// </summary>
        /// <param name="checkFunc">Function used to check if the object if the looking target</param>
        /// <returns>Object found using the check function</returns>
        public T FindObject(Func<T, bool> checkFunc)
        {
            ClearBusyObjects();

            foreach (T obj in _busyObjects)
            {
                if (checkFunc(obj))
                {
                    return obj;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Make all objects that are available back into the pool
        /// </summary>
        public void ClearBusyObjects()
        {
            for (int i = 0; i < _busyObjects.Count; i++)
            {
                if (CheckAvailabilityFunc(_busyObjects[i]))
                {
                    _availableObjects.Add(_busyObjects[i]);
                    _busyObjects.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Dispose all elements in the pool using the given destroyer
        /// </summary>
        /// <param name="destroyer">Destroyer function used to dispose each element</param>
        public void Dispose(Action<T> destroyer)
        {
            foreach(T obj in _busyObjects)
            {
                destroyer(obj);
            }

            foreach (T obj in _availableObjects)
            {
                destroyer(obj);
            }

            _busyObjects.Clear();
            _availableObjects.Clear();
        }
    }
}
