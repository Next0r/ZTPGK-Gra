using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsManager : MonoBehaviour
{
    private GameObject[] waypoints;
    public GameObject car;
    public bool isCarOnTrack = true;
    private GameObject recentWaypoint;
    public GameObject infoTextObject;
    private UnityEngine.UI.Text infoText;
    private float offTrackTimer = 0;
    public float offTrackTimerLimit = 5f;
    private Rigidbody carRigidbody;
    public bool hideWaypoints = true;

    // Start is called before the first frame update
    void Start()
    {
        infoText = infoTextObject.GetComponent<UnityEngine.UI.Text>();
        carRigidbody = car.GetComponent<Rigidbody>();
        GetAllWaypoints();

        if (hideWaypoints == true) {
            HideWaypoints();
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        CheckCarOnTrack();
        ReturnOnTrack();

        if (Input.GetKeyDown(KeyCode.B)) {
            PutCarOnTrack();
        }

    }

    private void HideWaypoints() {
        for (int i = 0; i < waypoints.Length; i++) {
            MeshRenderer meshRenderer = waypoints[i].GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
        }
    }

    private void PutCarOnTrack() {
        car.transform.position = recentWaypoint.transform.position;
        car.transform.rotation = recentWaypoint.transform.rotation;
        carRigidbody.velocity = new Vector3();
    }

    private void ReturnOnTrack() {

        if (offTrackTimer > offTrackTimerLimit) {
            offTrackTimer = 0;
            PutCarOnTrack();
            infoText.text = "";
            return;
        }

        if (isCarOnTrack == false)
        {
            offTrackTimer += Time.deltaTime;
            infoText.text = "You have left the track! \n Returning to track in: " + (offTrackTimerLimit - offTrackTimer).ToString("0.0") + "s";
        }
        else
        {
            offTrackTimer = 0;
            infoText.text = "";
        }

    }

    private void GetAllWaypoints() {
        waypoints = new GameObject[gameObject.transform.childCount];

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            waypoints[i] = gameObject.transform.GetChild(i).gameObject;
        }
         
    }

    private void CheckCarOnTrack() {

        isCarOnTrack = false;

        for (int i = 0; i < waypoints.Length; i++) {

            float waypointRadius = waypoints[i].GetComponent<SphereCollider>().radius;
            float distance = Vector3.Distance(car.transform.position, waypoints[i].transform.position);
            
            if (distance < waypointRadius)
            {
                isCarOnTrack = true;
                recentWaypoint = waypoints[i];
                break;
            }

        }

    }



}
