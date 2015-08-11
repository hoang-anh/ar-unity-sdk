using UnityEngine;
using System.Collections;
using AR_Wrapper;

public class Test_Script : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{

	    StartCoroutine(waitBeforeLoadScene(3.0f));

	}

    private IEnumerator waitBeforeLoadScene(float second)
    {
        yield return new WaitForSeconds(second);
        SimpleSceneController.Instance.SwitchScene("scene2");
    }

    // Update is called once per frame
	void Update () {
	
	}
}
