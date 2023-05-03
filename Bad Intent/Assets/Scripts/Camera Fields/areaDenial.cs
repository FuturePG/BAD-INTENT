using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class areaDenial : MonoBehaviour
{
    public float playerDetect;
    [SerializeField] float playerDetectedLimit = 3f;



    private void OnTriggerEnter(Collider other)
    {
        Debug.Log ("Detected") ;
        


        if (playerDetect > playerDetectedLimit)
        {
            SceneManager.LoadScene("Main Menu");
        }
    }
}
