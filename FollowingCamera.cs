using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    public GameObject cameraTarget;
    public float cameraDistance = 5f;
    public float cameraHeight = 2f;

    private float cameraLerp = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        UpdateCameraPosAndRot();
    }

    private void UpdateCameraPosAndRot() {

        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,
            cameraTarget.transform.position + 
            (-cameraTarget.transform.forward) * cameraDistance + 
            cameraTarget.transform.up * cameraHeight, cameraLerp);
        gameObject.transform.rotation = Quaternion.LookRotation(cameraTarget.transform.position - gameObject.transform.position);
    }
}
