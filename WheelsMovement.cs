using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelsMovement : MonoBehaviour
{
    public GameObject FrontLeftWheel;
    public GameObject FrontRightWheel;
    public GameObject RearLeftWheel;
    public GameObject RearRightWheel;

    private WheelCollider RLCollider;
    private WheelCollider RRCollider;
    private WheelCollider FLCollider;
    private WheelCollider FRCollider;

    public GameObject FrontLeftWheelMesh;
    public GameObject FrontRightWheelMesh;
    public GameObject RearLeftWheelMesh;
    public GameObject RearRightWheelMesh;

    private float FLrotationX = 0;
    private float FRrotationX = 0;

    private Vector3 FLBaseLocalPos;
    private Vector3 FRBaseLocalPos;
    private Vector3 RLBaseLocalPos;
    private Vector3 RRBaseLocalPos;

    // Start is called before the first frame update
    void Start()
    {
        RLCollider = RearLeftWheel.GetComponent<WheelCollider>();
        RRCollider = RearRightWheel.GetComponent<WheelCollider>();

        FLCollider = FrontLeftWheel.GetComponent<WheelCollider>();
        FRCollider = FrontRightWheel.GetComponent<WheelCollider>();

        FLBaseLocalPos = FrontLeftWheelMesh.transform.localPosition;
        FRBaseLocalPos = FrontRightWheelMesh.transform.localPosition;
        RLBaseLocalPos = RearLeftWheelMesh.transform.localPosition;
        RRBaseLocalPos = RearRightWheelMesh.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        HandleWheelPosition(FrontLeftWheelMesh, FLBaseLocalPos, FLCollider);
        HandleWheelPosition(FrontRightWheelMesh, FRBaseLocalPos, FRCollider);
        HandleWheelPosition(RearLeftWheelMesh, RLBaseLocalPos, RLCollider);
        HandleWheelPosition(RearRightWheelMesh, RRBaseLocalPos, RRCollider);
    }

    private void FixedUpdate()
    {
        HandleFrontWheelsRotation();
        HandleRearWheelsRotation();
    }


    private void HandleFrontWheelsRotation() {

        FLrotationX += FLCollider.rpm * 6 * Time.fixedDeltaTime;
        FRrotationX += FRCollider.rpm * 6 * Time.fixedDeltaTime;

        FrontLeftWheelMesh.transform.localEulerAngles = new Vector3(
            FLrotationX,
            FLCollider.steerAngle,
            0);

        FrontRightWheelMesh.transform.localEulerAngles = new Vector3(
            -FRrotationX,
            180 + FRCollider.steerAngle,
            0);
    }

    private void HandleRearWheelsRotation()
    {
        RearLeftWheelMesh.transform.Rotate(Vector3.right, RLCollider.rpm * 6 * Time.fixedDeltaTime);
        RearRightWheelMesh.transform.Rotate(Vector3.right, -RRCollider.rpm * 6 * Time.fixedDeltaTime);

    }

    private void HandleWheelPosition(GameObject mesh, Vector3 localBasePos, WheelCollider wheelCollider) {

        //WheelHit hit;
        //wheelCollider.GetGroundHit(out hit);
        float radius = wheelCollider.radius;

        RaycastHit hit;
        Physics.Raycast(wheelCollider.transform.position, -Vector3.up, out hit);


        float distance = Vector3.Distance(wheelCollider.transform.position , hit.point);
        float offset = radius - distance;

        if (wheelCollider.isGrounded)
        {
            mesh.transform.localPosition = localBasePos + new Vector3(0, offset + wheelCollider.suspensionDistance / 2, 0);
        }
        else
        {
            mesh.transform.localPosition = localBasePos + new Vector3(0, -wheelCollider.suspensionDistance / 2, 0);
        }

    }
}
