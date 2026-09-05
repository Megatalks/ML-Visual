using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RacingAgent : Agent
{
    private Vector3 startPosition;
    private PlayerMovement movement;
    private CarCollision collisionHandler;

    void Start()
    {
        startPosition = transform.localPosition;

        // change ray length
        RayPerceptionSensorComponent3D raySensor = GetComponent<RayPerceptionSensorComponent3D>();

        if (raySensor != null)
        {
            // overriding the capped ray length
            raySensor.RayLength = 5000f;
            Debug.Log($"Ray length successfully forced to: {raySensor.RayLength}");
        }

        // get movement and collision handler objects for reward function and observatiions
        movement = GetComponent<PlayerMovement>();
        collisionHandler = GetComponent<CarCollision>();
    }

    public override void OnEpisodeBegin() // env.reset()
    {
        // clean up environment: destroy obstacles
        CharacterSpawner.MovingObstacle[] obstacles = FindObjectsByType<CharacterSpawner.MovingObstacle>(FindObjectsSortMode.None);
        foreach (var obs in obstacles)
        {
            Destroy(obs.gameObject);
        }

        if (movement != null)
        {
            movement.ResetToStart(startPosition);
        }

        //transform.localPosition = startPosition;
        //GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        //GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // reset collision handler state so car can move again
        if (collisionHandler != null)
        {
            collisionHandler.IsMoving = true;
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        // build observation space
        // use sensor.AddObservation();
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(movement.CurrentForwardSpeed);
        sensor.AddObservation(movement.CurrentDriftSpeed);
        sensor.AddObservation(movement.CurrentVisualAngle);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers) // env.step
    {
        int action = actionBuffers.DiscreteActions[0];

        // clear the input buffer
        VirtualInput.ResetInputs();

        // execute action here
        ExecuteMovement(action);

        // also calculate reward
        float speedReward = (movement.CurrentForwardSpeed / 70f) * 0.05f;
        AddReward(speedReward);

        // define end conditions
        if (HasCrashed())
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.A)) discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.D)) discreteActions[0] = 2;
    }

    private void ExecuteMovement(int action) {
        switch (action) {
            case 0:
                break;
            case 1:
                VirtualInput.Accel = true;
                break;
            case 2:
                VirtualInput.Brake = true;
                break;
            case 3:
                VirtualInput.MoveLeft = true;
                break;
            case 4:
                VirtualInput.MoveRight = true;
                VirtualInput.Accel = true;
                break;
            case 5:
                VirtualInput.MoveRight = true;
                VirtualInput.Brake = true;
                break;
            case 6:
                VirtualInput.MoveLeft = true;
                break;
            case 7:
                VirtualInput.MoveLeft = true;
                VirtualInput.Accel = true;
                break;
            case 8:
                VirtualInput.MoveLeft = true;
                VirtualInput.Brake = true;
                break;
        }
    }


    private float CalculateReward() { return 0.1f; }
    private bool HasCrashed () { return false; }

}

public static class VirtualInput
{
    public static bool MoveLeft { get; set; }
    public static bool MoveRight { get; set; }
    public static bool Accel { get; set; }
    public static bool Brake { get; set; }

    public static void ResetInputs()
    {
        MoveLeft = MoveRight = Accel = Brake = false;
    }
}