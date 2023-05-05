using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelSwitchScript : MonoBehaviour
{
    public int sceneBuildIndex;

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {

        /*
        print("Trigger Entered");

        if (other.tag == "Player")
        {
            print("Switching Scene to " + sceneBuildIndex);
            SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
        }
        */

        if (other.tag == "Player")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
