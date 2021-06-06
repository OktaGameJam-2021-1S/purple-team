using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomActivateUtil : MonoBehaviour
{
    public GameObject go;

    void Awake()
    {
        if (go == null)
            go = transform.GetChild(0).gameObject;

        go.SetActive(false);
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1, 5));
            go.SetActive(true);
            yield return new WaitForSeconds(Random.Range(0.1f, 2f));
            go.SetActive(false);
        }
    }
}
