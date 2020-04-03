using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{

    public int number;
    public RaceManager raceManager;
    private bool visited = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUnvisited() {
        visited = false;
    }

    private void OnTriggerEnter(Collider other)
    {


        if (other.CompareTag("Car") && visited == false) {

            if (number == 3)
            {
                raceManager.FinishRace();
                return;
            }

            raceManager.SetSplitTime(number);
            visited = true;
        }
    }

}
