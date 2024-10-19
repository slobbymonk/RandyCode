using System;
using UnityEngine;

public class BreakCubeMission : Mission
{
    [SerializeField] private GameObject _cubeToBreak;

    public override event EventHandler<MissionEventArgs> MissionCompleted;


    void Update()
    {
        if (_cubeToBreak == null)
        {
            // Trigger the MissionCompleted event if the cube is broken
            MissionCompleted?.Invoke(this, new MissionEventArgs(this));
        }
    }
}
