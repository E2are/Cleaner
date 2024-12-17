using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DoorLightController : MonoBehaviour

{
    // Start is called before the first frame update
    public LightState lightState;
    [Range(0.0f, 10.0f)]
    public float FlickerFrequency;
    public float FlickerIntensity;
    public Light light;
    public GameObject LOD0;
    public GameObject LOD1;
    public GameObject LOD2;
    public Material LOD0_MaterialOn;
    public Material LOD0_MaterialOff;
    public Material LOD1_MaterialOn;
    public Material LOD1_MaterialOff;
    public Material LOD2_MaterialOn;
    public Material LOD2_MaterialOff;
    float myRandomNumber;
    float timer;
    float currentIntensity;

    void Start()
    {
        timer = 10f;
        currentIntensity = 10.0f;
        FlickerIntensity = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {

        if (lightState == LightState.On)
        {
            TurnOn();
        }
        else if (lightState == LightState.Off)
        {
            TurnOff();
        }
        else if (lightState == LightState.Flickering)
        {
            Flicker();
        }
    }
    void TurnOn()
    {
        LOD0.GetComponent<Renderer>().material = LOD0_MaterialOn;
        LOD1.GetComponent<Renderer>().material = LOD1_MaterialOn;
        LOD2.GetComponent<Renderer>().material = LOD2_MaterialOn;
        light.enabled = true;
        light.intensity = 10.0f;
    }
    void TurnOff()
    {
        LOD0.GetComponent<Renderer>().material = LOD0_MaterialOff;
        LOD1.GetComponent<Renderer>().material = LOD1_MaterialOff;
        LOD2.GetComponent<Renderer>().material = LOD2_MaterialOff;
        light.enabled = false;
        light.intensity = 10.0f;
    }
    void Flicker()
    {
        light.enabled = true;
        myRandomNumber = Random.Range(0f, 10f);
        if (myRandomNumber < FlickerFrequency)
        {
            light.intensity = FlickerIntensity;
            LOD0.GetComponent<Renderer>().material = LOD0_MaterialOff;
            LOD1.GetComponent<Renderer>().material = LOD1_MaterialOff;
            LOD2.GetComponent<Renderer>().material = LOD2_MaterialOff;
        }
        else
        {
            light.intensity = 10.0f;
            LOD0.GetComponent<Renderer>().material = LOD0_MaterialOn;
            LOD1.GetComponent<Renderer>().material = LOD1_MaterialOn;
            LOD2.GetComponent<Renderer>().material = LOD2_MaterialOn;
        }

    }
}
public enum LightState { On, Off, Flickering };
