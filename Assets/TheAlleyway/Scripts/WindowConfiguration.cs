using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WindowConfiguration : MonoBehaviour
{
	public bool LightsOn = false;
	public bool Bars = false;
	public GameObject LightsObject;
	public GameObject BarsObject;
    int counter = 1;
    // Start is called before the first frame update
    void Start()
    {
        counter = 1;
    }

    // Update is called once per frame
    void Update()
    {
		if (!Application.isPlaying) {
			if (LightsOn) {
				LightsObject.SetActive (true);
			} else {
				LightsObject.SetActive (false);
			}
			if (Bars) {
				BarsObject.SetActive (true);
			} else {
				BarsObject.SetActive (false);
			}
        }
        else
        {
            if (!LightsOn && counter == 1)
            {
                counter = 0;
                Destroy(LightsObject);
            }
            if (!Bars && counter == 1)
            {
                counter = 0;
                Destroy(BarsObject);
            }
        }
    }
}
