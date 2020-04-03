using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{

    public GameObject start;
    public Checkpoint checkpoint1;
    public Checkpoint checkpoint2;
    public Checkpoint checkpoint3;
    //public GameObject finish;

    public CarMovement carMovement;

    private float startTimer = 0;
    private float startTime = 5f;
    private float lastRaceTime = 0f;
    private float raceTimer = 0f;

    private float[] splitTimes = new float[3];

    private bool raceStarted = false;
    private bool raceFinished = false;

    public UnityEngine.UI.Text raceTimeTextBox;
    public UnityEngine.UI.Text lastRaceTimeTextBox;
    public UnityEngine.UI.Text[] splitTimeTextBoxes = new UnityEngine.UI.Text[3];
    public UnityEngine.UI.Text countDowntextBox;


    // Start is called before the first frame update
    void Start()
    {
        SetCarOnStart();
    }

    // Update is called once per frame
    void Update()
    {

        HandleStartTimer();
        HandleRaceTimer();

        if (Input.GetKeyDown(KeyCode.G)) {
            SetCarOnStart();
        }

        HandleGUI();

    }

    public void SetSplitTime(int checkpoint) {

        splitTimes[checkpoint] = raceTimer;
        splitTimeTextBoxes[checkpoint].text = raceTimer.ToString("0.000");

    }

    private void HandleGUI() {
        raceTimeTextBox.text = "Current: " + raceTimer.ToString("0.000");
        lastRaceTimeTextBox.text = "Last: " + lastRaceTime.ToString("0.000");

        if (startTimer > 0)
        {
            countDowntextBox.text = startTimer.ToString("0");
        }
        else
        {
            countDowntextBox.text = "";
        }
    }

    public void FinishRace() {

        lastRaceTime = raceTimer;
        raceStarted = false;
        raceFinished = true;
        carMovement.DisableMovement();

    }

    public float GetRaceTimer()
    {

        return raceTimer;

    }

    private void HandleRaceTimer()
    {

        if (raceStarted == true)
        {
            raceTimer += Time.deltaTime;    
        }

    }

    private void HandleStartTimer() {

        if (startTimer > 0)
        {
            startTimer -= Time.deltaTime;
        }
        else if (raceTimer == 0)
        {
            raceStarted = true;
            carMovement.EnableMovement();
        }

    }

    private void SetCarOnStart() {

        raceStarted = false;
        raceTimer = 0;
        startTimer = startTime;
        carMovement.DisableMovement();

        splitTimeTextBoxes[0].text = "";
        splitTimeTextBoxes[1].text = "";
        splitTimeTextBoxes[2].text = "";

        checkpoint1.SetUnvisited();
        checkpoint2.SetUnvisited();
        checkpoint3.SetUnvisited();

        carMovement.gameObject.transform.position = start.transform.position;
        carMovement.gameObject.transform.rotation = start.transform.rotation;
        carMovement.gameObject.GetComponent<Rigidbody>().velocity = new Vector3();

    }

}
