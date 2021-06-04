using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Utils
{
    public delegate T GeneratorDelegate<T>(int index, params object[] args);
    public delegate void DestroyerDelegate<T>(T obj, int index, params object[] args);
    public static class Utilities
    {
        #region List Functions
        /// <summary>
        /// Resize the list keeping currently items and adding new ones or removing last ones deppending of the new size.
        /// </summary>
        /// <typeparam name="T">Element Type</typeparam>
        /// <param name="array">Target list</param>
        /// <param name="newLength">New length of the list</param>
        /// <param name="generator">Generator of new elements (used when newLength > currentLength)</param>
        /// <param name="destroyer">Destroyer of elements (used when newLength < currentLength)</param>
        /// <param name="parameters">Parameters to be passed to the Generator/Destroyer</param>
        public static void ResizeList<T>(List<T> array, int newLength, GeneratorDelegate<T> generator = null, DestroyerDelegate<T> destroyer = null, params object[] parameters)
        {
            int length = array.Count;
            if (newLength < length)
            {
                if (destroyer != null)
                {
                    for (int i = newLength; i < length; i++)
                    {
                        destroyer(array[i], i, parameters);
                    }
                }
                array.RemoveRange(newLength, length - newLength);
            }
            else if (newLength > length)
            {
                if (newLength > array.Capacity)// Only to avoid multiple automatic capacity changes.
                    array.Capacity = newLength;
                for (int i = length; i < newLength; i++)
                {
                    T newObj = default(T);
                    if (generator != null)
                    {
                        newObj = generator(i, parameters);
                    }

                    array.Add(newObj);
                }
            }
        }

        /// <summary>
        /// Merge two lists into one putting the sourceA at beginning and sourceB ate end
        /// </summary>
        /// <typeparam name="T">Type of the List</typeparam>
        /// <param name="sourceA">First source list</param>
        /// <param name="sourceB">Seccond source list</param>
        /// <returns></returns>
        public static List<T> MergeList<T>(List<T> sourceA, List<T> sourceB)
        {
            List<T> mergedList = new List<T>(sourceA.Count + sourceB.Count);

            for (int i = 0; i < sourceA.Count; i++)
            {
                mergedList.Add(sourceA[i]);
            }

            for (int i = 0; i < sourceB.Count; i++)
            {
                mergedList.Add(sourceB[i]);
            }

            return mergedList;
        }

        /// <summary>
        /// Generate a new list with randomized order of the elements
        /// </summary>
        /// <typeparam name="T">Element Type</typeparam>
        /// <param name="original">Original list to be shuffled</param>
        /// <returns></returns>
        public static List<T> ShuffleList<T>(List<T> original)
        {
            List<T> shuffledList = new List<T>(original.Count);
            List<T> originalList = new List<T>(original.Count);

            for (int i = 0; i < original.Count; i++)
            {
                originalList.Add(original[i]);
            }

            while (originalList.Count > 0)
            {
                int index = Random.Range(0, originalList.Count);
                shuffledList.Add(originalList[index]);
                originalList.RemoveAt(index);
            }

            return shuffledList;
        }

        /// <summary>
        /// Distribute an amount of elements in possible targets. Repeats a target only when the "amount to spread" > "number of targets"
        /// </summary>
        /// <typeparam name="T">Element Type</typeparam>
        /// <param name="amountToSpread">The number of elements to be spreaded in the targets</param>
        /// <param name="possibleTargets">Possible targets an element can be spreaded</param>
        /// <returns>A list of targets for the elements from index 0 to amountToSpread -1</returns>
        public static List<T> Spread<T>(int amountToSpread, List<T> possibleTargets)
        {
            List<T> finalTargets = new List<T>(amountToSpread);

            // If there is no element to be spread or the list of targets is empty, then return empty list
            if (amountToSpread <= 0 || possibleTargets.Count <= 0) return finalTargets;

            bool[] used = new bool[possibleTargets.Count];

            // The number of times the elements will be spreaded in the possible targets (Usefull when amountToSpread > possibleTargets.Count)
            int numberOfRandomizations = 1 + ((amountToSpread - 1) / possibleTargets.Count);

            // Repeats the spread of the elements in the possible targets until all elements has been distributed
            for (int r = 0; r < numberOfRandomizations; r++)
            {
                // Clear the used array
                for (int i = 0; i < used.Length; i++)
                {
                    used[i] = false;
                }

                // Spread the next possibleTargets.Count elements in the possible targets
                for (int i = 0; (r * possibleTargets.Count + i) < amountToSpread && i < possibleTargets.Count; i++)
                {
                    int drawnIndex = -1;
                    // Drawn a target for the next element
                    do
                    {
                        drawnIndex = Random.Range(0, possibleTargets.Count);
                    } while (used[drawnIndex]);

                    // Mark the target as already drawn
                    used[drawnIndex] = true;
                    
                    finalTargets.Add(possibleTargets[drawnIndex]);
                }

            }

            return finalTargets;
        }
        #endregion

        #region GameObject Functions
        /// <summary>
        /// Change the target layer and all of its children recursively
        /// </summary>
        /// <param name="target">target GameObject</param>
        /// <param name="layer">New GameObject Layer</param>
        public static void ChangeObjectLayer(GameObject target, LayerMask layer)
        {
            target.layer = layer;
            for (int c = 0; c < target.transform.childCount; c++)
            {
                Transform child = target.transform.GetChild(c);
                if (child.gameObject.layer == LayerMask.NameToLayer("Character"))
                {
                    child.gameObject.layer = layer;
                }
                ChangeObjectLayer(child.gameObject, layer);
            }
        }
        #endregion

        #region Physics Functions
        public const int WallMask = 1 << 9;
        /// <summary>
        /// Check if the target is visible from the source position
        /// </summary>
        /// <param name="source">Position of the viewer</param>
        /// <param name="target">Target to check visibility</param>
        /// <param name="maxDistance">The max distance the source can see the target</param>
        /// <param name="blockedBySimilar">Is the visibility blocked by objects of the same layer of the target?</param>
        /// <param name="obstacleLayer">Layer of obstacles hat can block the visibility of the target</param>
        /// <returns></returns>
        public static bool IsTargetVisible(Vector3 source, Transform target, float maxDistance = float.MaxValue, bool blockedBySimilar = true, int obstacleLayer = WallMask)
        {
            Vector3 direction = (target.transform.position - source);
            float distance = direction.magnitude;

            if (distance > maxDistance) return false;

            distance += 0.5f;

            direction.Normalize();

            int targetLayer = 1 << target.gameObject.layer;
            int layer = blockedBySimilar ? targetLayer | obstacleLayer : obstacleLayer;

            Ray ray = new Ray(source, direction);
            
            bool rayCastHitted = Physics.Raycast(ray, out RaycastHit hitInfo, distance, layer);

#if ENABLE_DEBUGGER
            Debug.DrawRay(source, direction * distance, Color.cyan);
#endif
            if (rayCastHitted)
            {
                return hitInfo.transform == target;
            }

            return true;
        }
        #endregion

        #region Delta Time Functions
        /// <summary>
        /// Calculates a float deltatime from a start and end time stamp
        /// </summary>
        /// <param name="startTimeStamp"></param>
        /// <param name="finalTimeStamp"></param>
        /// <returns></returns>
        public static float DeltaTime(int startTimeStamp, int endTimeStamp)
        {
            return (endTimeStamp - startTimeStamp) * 0.001f;
        }

        /// <summary>
        /// Calculates the delta time in miliseconds from seconds
        /// </summary>
        /// <param name="deltaTime">delta time in seconds</param>
        /// <returns></returns>
        public static int DeltaTimeStamp(float deltaTime)
        {
            return Mathf.CeilToInt(deltaTime * 1000);
        }

        /// <summary>
        /// Transform the time stamp in seconds from miliseconds
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static float TimeStampDeltaTime(int timeStamp)
        {
            return timeStamp * 0.001f;
        }
        #endregion

        #region String Generation Functions
        public enum CharacterGenerationType
        {
            All, // Alphabet upper and lower case and numbers
            NumberOnly, // Only number characters
            AlphabetOnly, // Alphabet upper and lower case
            AlphabetUpperOnly,// Alphabet upper case
            AlphabetLowerOnly,// Alphabet lower case
            AlphabetUpperAndNumber,// Alphabet upper case and numbers
            AlphabetLowerAndNumber,// Alphabet lower case and numbers
        }
        /// <summary>
        /// Generates a random string with random characters with the given length
        /// </summary>
        /// <param name="length">The length of the generated code</param>
        /// <param name="generationType">Whith characters should be included in the code</param>
        /// <returns></returns>
        public static string GenerateRandomCode(int length, CharacterGenerationType generationType = CharacterGenerationType.All)
        {
            string code = "";

            for (int i = 0; i < length; i++)
            {
                char c = GenerateRandomCharacter(generationType);
                code += c;
            }

            return code;
        }

        /// <summary>
        /// Generates a random character according to the generation type
        /// </summary>
        /// <param name="generationType">Whith characters should be included</param>
        /// <returns></returns>
        public static char GenerateRandomCharacter(CharacterGenerationType generationType = CharacterGenerationType.All)
        {
            int randomType = Random.Range(0, 100);

            char[] generatedCharacters = new char[]
            {
                RandomNumberCharacter(),
                RandomAlphabetLowerCharacter(),
                RandomAlphabetUpperCharacter()
            };

            switch (generationType)
            {
                case CharacterGenerationType.NumberOnly: return generatedCharacters[0];
                case CharacterGenerationType.AlphabetLowerOnly: return generatedCharacters[1];
                case CharacterGenerationType.AlphabetUpperOnly: return generatedCharacters[2];
                case CharacterGenerationType.AlphabetOnly: return generatedCharacters[randomType % 2 + 1];
                case CharacterGenerationType.AlphabetLowerAndNumber: return generatedCharacters[randomType % 2];
                case CharacterGenerationType.AlphabetUpperAndNumber: return generatedCharacters[(randomType % 2 == 0 ? 0 : 2)];
                default: return generatedCharacters[randomType % generatedCharacters.Length];
            }
        }

        /// <summary>
        /// Generates a random number character
        /// </summary>
        /// <returns></returns>
        public static char RandomNumberCharacter()
        {
            return (char)(Random.Range(0, 10) + '0');
        }

        /// <summary>
        /// Generates a random Alphabet Lower case character
        /// </summary>
        /// <returns></returns>
        public static char RandomAlphabetLowerCharacter()
        {
            return (char)(Random.Range(0, 26) + 'a');
        }

        /// <summary>
        /// Generates a random Alphabet Upper case character
        /// </summary>
        /// <returns></returns>
        public static char RandomAlphabetUpperCharacter()
        {
            return (char)(Random.Range(0, 26) + 'A');
        }
        #endregion

        #region UI Functions
        /// <summary>
        /// Calculates rotation for a 2D object to look at a 2D direction in UI
        /// </summary>
        /// <param name="direction">Direction in UI</param>
        /// <returns></returns>
        public static Quaternion LookAt2D(Vector2 direction)
        {
            direction.Normalize();

            // Invert the angle after 180 degrees
            float dot = Vector2.Dot(Vector2.right, direction);
            float invertFactor = dot < 0 ? 1 : -1;

            return Quaternion.Euler(0, 0, Vector2.Angle(Vector2.up, direction) * invertFactor);
        }

        /// <summary>
        /// Add listener of a event to a button (if there is no EventTrigger attached to it, it will add one)
        /// </summary>
        /// <param name="button">Button to trigger the event</param>
        /// <param name="eventTriggerType">Event to be listened</param>
        /// <param name="callback">Callback to be called when the event is triggered</param>
        public static void AddEventListenerToButton(Button button, EventTriggerType eventTriggerType, Action callback)
        {
            EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
            trigger.AddListener((eventData) => callback?.Invoke());
            EventTrigger.Entry entry = new EventTrigger.Entry() { callback = trigger, eventID = eventTriggerType };

            EventTrigger buttonEventTrigger = button.GetComponent<EventTrigger>();
            if (buttonEventTrigger == null)
            {
                buttonEventTrigger = button.gameObject.AddComponent<EventTrigger>();
            }

            buttonEventTrigger.triggers.Add(entry);
        }
        #endregion

        #region Color Functions
        /// <summary>
        /// Changes the alpha from a color keeping the RGB values
        /// </summary>
        /// <param name="color"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static Color ModifyColorAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
        #endregion

        #region Delay Action
        /// <summary>
        /// Invoke an action after a delay
        /// </summary>
        /// <param name="time"></param>
        /// <param name="executeAfterDelay"></param>
        /// <returns></returns>
        public static IEnumerator DelayAction(float time, Action executeAfterDelay)
        {
            yield return new WaitForSeconds(time);
            executeAfterDelay?.Invoke();
        }
        #endregion
    }
}
