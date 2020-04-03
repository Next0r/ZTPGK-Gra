using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public GameObject FrontLeftWheel;
    public GameObject FrontRightWheel;
    public GameObject RearLeftWheel;
    public GameObject RearRightWheel;

    private WheelCollider RLCollider;
    private WheelCollider RRCollider;
    private WheelCollider FLCollider;
    private WheelCollider FRCollider;

    public float motorTorque = 700;
    public float maxSteeringAngle = 35;
    private float currentMotorTorque;
    public float brakeTorque = 500;

    private Vector3 lastPosition;
    private Vector3 currentPosition;
    public float speed = 0;
    //private float[] gearRatios = { 3.1f, 2f, 1.2f, 0.9f, 0.7f };
    private float[] gearRatios = { 3.6f, 3.1f, 2.5f, 2.1f, 1.6f, 1.2f };
    private float[] gearUpshiftSpeeds;
    private float[] gearDownshiftSpeeds;
    private float gearShiftSpeedCorrection = -15f;

    private float diffRatio = 4.0f;
    private float shiftMargin = 1000;       // rpm margin
    private float shiftPoint = 0.8f;        // shiftPoint * maxRpm +- margin = rpm where gear should be shifted
    private float reverseShiftSpeed = 5f;   // at which speed reverse shift is possible
    public bool reverse = false;

    public int gear = 1;
    
    public float rpm;
    public float maxRpm = 7000f;
    public float minRpm = 1500f;
    private float rmpNoise = 75f;

    private float gearShiftTimer = 0;
    private float gearShiftTime = 0.5f;

    private float engineBrake = 0.00f;

    public GameObject centerOfMass;
    private Rigidbody carRB;

    public GameObject rpmMeterGUIElement;
    public GameObject rpmMeterNeedleGUIElement;
    public UnityEngine.UI.Text speedMeterTextBox;
    public UnityEngine.UI.Text gearTextBox;

    public GameObject[] dashLightGUI = new GameObject[5];

    private UnityEngine.UI.Image rpmMeterNeedleImage;
    private UnityEngine.UI.Image[] dashLightsImages; 

    private float rpmNeedleBaseRot = 0f;

    private AudioSource engineAudioSource;
    private float baseEngineAudioPitch = 0.6f;
    private float maxEngineAudioPitch = 1.7f;

    private float soundFac = 0f;
    //private float accelerationCoefficient = 0f;     // for smooth acceleration
    //private float accelerationLerp = 0.2f;          // acceleration change

    private bool movementEnabled = true;

    // Start is called before the first frame update
    void Start()
    {
        RLCollider = RearLeftWheel.GetComponent<WheelCollider>();
        RRCollider = RearRightWheel.GetComponent<WheelCollider>();

        FLCollider = FrontLeftWheel.GetComponent<WheelCollider>();
        FRCollider = FrontRightWheel.GetComponent<WheelCollider>();

        lastPosition = gameObject.transform.position;
        currentPosition = gameObject.transform.position;

        rpmMeterNeedleImage = rpmMeterNeedleGUIElement.GetComponent<UnityEngine.UI.Image>();

        gearUpshiftSpeeds = new float[gearRatios.Length];
        gearDownshiftSpeeds = new float[gearRatios.Length];

        engineAudioSource = gameObject.GetComponent<AudioSource>();

        float wheelCircumference = RLCollider.radius * 2f * Mathf.PI;

        // Calculate gear upshif and downshift speeds
        for (int i = 0; i < gearRatios.Length; i++) {
            gearUpshiftSpeeds[i] = (shiftPoint * maxRpm + shiftMargin) / diffRatio / gearRatios[i] / 60 * wheelCircumference * 3.6f + gearShiftSpeedCorrection;
            gearDownshiftSpeeds[i] = (shiftPoint * maxRpm - shiftMargin) / diffRatio / gearRatios[i] / 60 * wheelCircumference * 3.6f + gearShiftSpeedCorrection;
        }

        carRB = gameObject.GetComponent<Rigidbody>();
        carRB.centerOfMass = centerOfMass.transform.localPosition;

        // get dash light images
        dashLightsImages = new UnityEngine.UI.Image[dashLightGUI.Length];
        for (int i = 0; i < dashLightGUI.Length; i++) {
            dashLightsImages[i] = dashLightGUI[i].GetComponent<UnityEngine.UI.Image>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        HandleMotor();
        HandleSteering();
        UpdateMotorTorqueAndRpm();
        UpdateGear();
    }

    private void FixedUpdate()
    {
        RecordSpeed();
        HandleGUIAndSound();
    }

    public void DisableMovement() {
        movementEnabled = false;
    }

    public void EnableMovement() {
        movementEnabled = true;
    }

    private void HandleGUIAndSound() {

        float rotationMultiplier = 0.033f;

        // rpm
        float displayedRpm = Mathf.Max(Mathf.Min(rpm, maxRpm), minRpm) + Mathf.Sin(Time.time * 50f) * rmpNoise;

        Quaternion rot = Quaternion.Euler(0, 0, rpmNeedleBaseRot - displayedRpm * rotationMultiplier);
        rpmMeterNeedleImage.transform.rotation = Quaternion.Lerp(rpmMeterNeedleImage.transform.rotation, rot, 0.03f);


        // rpm audio
        float maxRot = 360 - maxRpm * rotationMultiplier;
        float minRot = 360 - minRpm * rotationMultiplier;

        float lerpFac = 0.1f;

        soundFac = soundFac * (1 - lerpFac) + (minRot - rot.eulerAngles.z) / (minRot - maxRot) * lerpFac;
        engineAudioSource.pitch = baseEngineAudioPitch + soundFac * (maxEngineAudioPitch - baseEngineAudioPitch);

        // speed
        speedMeterTextBox.text = speed.ToString("0");

        // gear
        if (reverse == false)
        {
            gearTextBox.text = gear.ToString();
        }
        else {
            gearTextBox.text = "R";
        }

        // dash lights
        for (int i = 0; i < dashLightsImages.Length; i++) {
            float rpmFromRot = (360 - rpmMeterNeedleImage.transform.eulerAngles.z) * (maxRpm / (maxRpm * rotationMultiplier));
            Debug.Log(rpmFromRot);

            if (rpmFromRot > (6000 + i * 400))
            {
                dashLightsImages[i].enabled = true;
            }
            else
            {
                dashLightsImages[i].enabled = false;
            }
        }


    }

    private void UpdateMotorTorqueAndRpm() 
    {
        float avgWheelRpm = Mathf.Abs((RLCollider.rpm + RRCollider.rpm + FRCollider.rpm + FLCollider.rpm) / 4);
        rpm = avgWheelRpm * diffRatio * gearRatios[gear - 1];
        float x = (rpm + 1f) / maxRpm;

        currentMotorTorque = motorTorque * gearRatios[gear - 1] * (-Mathf.Pow(x - 0.25f, 2) + 1);

        if (rpm > maxRpm) {
            currentMotorTorque = 0;
        }

    }

    private void UpdateGear() {

        if (gearShiftTimer >= 0)
        {
            gearShiftTimer -= Time.deltaTime;
            return;
        }

        if (speed < reverseShiftSpeed && Input.GetKeyDown(KeyCode.R))
        {
            reverse = reverse ? false : true;
            gear = 1;
            gearShiftTimer = gearShiftTime;
        }

        // Do not shift gears if reverse is on
        if (reverse == true) {
            return;
        }

        if (rpm > shiftPoint * maxRpm + shiftMargin && gear < gearRatios.Length && speed > gearUpshiftSpeeds[gear - 1])
        {
            gear += 1;
            gearShiftTimer = gearShiftTime;
        }
        else if (rpm < shiftPoint * maxRpm - shiftMargin && gear > 1 && speed < gearDownshiftSpeeds[gear - 1])
        {
            gear -= 1;
            gearShiftTimer = gearShiftTime;
        }

    }

    private void RecordSpeed() {
        float distance = Vector3.Distance(lastPosition, currentPosition);
        lastPosition = currentPosition;
        currentPosition = gameObject.transform.position;
        speed = distance / Time.fixedDeltaTime * 3.6f;
    }


    private void HandleSteering() { 
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        FRCollider.steerAngle = steering;
        FLCollider.steerAngle = steering;
    }

    private void HandleMotor()
    {
  
        float motor = currentMotorTorque * Input.GetAxis("Vertical");

        if (movementEnabled == false) {
            FLCollider.motorTorque = 0;
            FLCollider.brakeTorque = motorTorque;

            FRCollider.motorTorque = FLCollider.motorTorque;
            RLCollider.motorTorque = FLCollider.motorTorque;
            RRCollider.motorTorque = FLCollider.motorTorque;

            FRCollider.brakeTorque = FLCollider.brakeTorque;
            RLCollider.brakeTorque = FLCollider.brakeTorque;
            RRCollider.brakeTorque = FLCollider.brakeTorque;
            return;
        }
       

        if (Input.GetAxis("Vertical") > 0)
        {
            if (gearShiftTimer > 0) {

                //float x = (1 - (gearShiftTimer / gearShiftTimer)) * 0.75f; ;


                motor = motor * 0.3f + motor * (1 - (gearShiftTimer / gearShiftTimer)) * 0.7f;

            }

            //accelerationCoefficient = accelerationCoefficient * (1 - accelerationLerp) + 1 * accelerationLerp;

            if (reverse == false)
            {
                FLCollider.motorTorque = motor;// * accelerationCoefficient;
            }
            else {
                FLCollider.motorTorque = -motor;// * accelerationCoefficient;
            }

            FLCollider.brakeTorque = 0;
        }
        else if (Input.GetAxis("Vertical") < 0)
        {
            //accelerationCoefficient = 0;

            FLCollider.motorTorque = 0;

            FLCollider.brakeTorque = motorTorque * brakeTorque;
        }
        else {
            //accelerationCoefficient = 0;

            FLCollider.motorTorque = 0;

            FLCollider.brakeTorque = motorTorque * engineBrake;
        }

        FRCollider.motorTorque = FLCollider.motorTorque;
        RLCollider.motorTorque = FLCollider.motorTorque;
        RRCollider.motorTorque = FLCollider.motorTorque;

        FRCollider.brakeTorque = FLCollider.brakeTorque;
        RLCollider.brakeTorque = FLCollider.brakeTorque;
        RRCollider.brakeTorque = FLCollider.brakeTorque;

    }

}
