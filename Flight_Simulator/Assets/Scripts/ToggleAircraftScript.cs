using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleAircraftScript : MonoBehaviour
{
    public GameObject[] Aircraft;

    public GameObject BiPlaneCamea;
    public GameObject bigPlaneCam;
    public GameObject piperPlaneCam;

    private Vector3 initalPosBigP;
    private Quaternion rotationBp;

    private Vector3 initalPosPiperP;
    private Quaternion rotationPp;

    private Vector3 initalPosBiP;
    private Quaternion rotationBip;
    private int count = 0;

    //quick script made for demo purposes
    void Start()
    {
        Aircraft = GameObject.FindGameObjectsWithTag("Aeroplane");

        initalPosBigP = Aircraft[1].transform.position;
        rotationBp = Aircraft[1].transform.rotation;

        initalPosPiperP = Aircraft[0].transform.position;
        rotationPp = Aircraft[0].transform.rotation;

        initalPosBiP = Aircraft[2].transform.position;
        rotationBip = Aircraft[2].transform.rotation;


        Aircraft[1].gameObject.SetActive(false);
        Aircraft[2].gameObject.SetActive(false);
    }


    private void Update()
    {
        piperPlaneCam.SetActive(Aircraft[0].activeInHierarchy);
        bigPlaneCam.SetActive(Aircraft[1].activeInHierarchy);
        BiPlaneCamea.SetActive(Aircraft[2].activeInHierarchy);

        if (count > Aircraft.Length - 1)
        {
            count = 0;
        }

        if (count == 0)
        {
            Aircraft[0].SetActive(true);
            Aircraft[1].SetActive(false);
            Aircraft[2].SetActive(false);
        }
        else if (count == 1)
        {
            Aircraft[1].SetActive(true);
            Aircraft[2].SetActive(false);
            Aircraft[0].SetActive(false);
        }
        else if( count == 2)
        {
            Aircraft[2].SetActive(true);
            Aircraft[0].SetActive(false);
            Aircraft[1].SetActive(false);
        }

    }

    public void ChangeAircraft()
    {
        count++;

        if (count == 0)
        {
            Aircraft[1].transform.position = initalPosBigP;
            Aircraft[1].transform.rotation = rotationBp;
            Aircraft[1].GetComponent<AircraftController>().power = 0;
            Aircraft[1].GetComponent<AircraftController>().EngineRPM = 0;

            Aircraft[1].GetComponent<AircraftController>().breakTorque = 1000;

            Aircraft[2].transform.position = initalPosBiP;
            Aircraft[2].transform.rotation = rotationBip;
            Aircraft[2].GetComponent<AircraftController>().power = 0;
            Aircraft[2].GetComponent<AircraftController>().EngineRPM = 0;
            Aircraft[2].GetComponent<AircraftController>().breakTorque = 1000;
        }
        else if (count == 1)
        {
            Aircraft[0].transform.position = initalPosPiperP;
            Aircraft[0].transform.rotation = rotationPp;
            Aircraft[0].GetComponent<AircraftController>().power = 0;
            Aircraft[0].GetComponent<AircraftController>().EngineRPM = 0;
            Aircraft[0].GetComponent<AircraftController>().breakTorque = 1000;

            Aircraft[2].transform.position = initalPosBiP;
            Aircraft[2].transform.rotation = rotationBip;
            Aircraft[2].GetComponent<AircraftController>().power = 0;
            Aircraft[2].GetComponent<AircraftController>().EngineRPM = 0;
            Aircraft[2].GetComponent<AircraftController>().breakTorque = 1000;
        }
        else if (count == 2)
        {
            Aircraft[1].transform.position = initalPosBigP;
            Aircraft[1].transform.rotation = rotationBp;
            Aircraft[1].GetComponent<AircraftController>().power = 0;
            Aircraft[1].GetComponent<AircraftController>().EngineRPM = 0;
            Aircraft[1].GetComponent<AircraftController>().breakTorque = 1000;

            Aircraft[0].transform.position = initalPosPiperP;
            Aircraft[0].transform.rotation = rotationPp;
            Aircraft[0].GetComponent<AircraftController>().power = 0;
            Aircraft[0].GetComponent<AircraftController>().EngineRPM = 0;
            Aircraft[0].GetComponent<AircraftController>().breakTorque = 1000;
        }
    }
}
