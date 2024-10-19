using System;
using UnityEngine;

public class DoStuntsMission : Mission
{
    public override event EventHandler<MissionEventArgs> MissionCompleted;

    private int stuntsToDo;


    private void Awake()
    {
        stuntsToDo = _missionLocation.Count;
    }

    void Update()
    {
        if(stuntsToDo == 0)
        {
            MissionCompleted?.Invoke(this, new MissionEventArgs(this));
        }
    }

    public bool StuntDone(Transform stunt)
    {
        if (_missionIsActive)
        {
            stuntsToDo--;

            int index = 0;
            foreach (var stunts in _missionLocation)
            {
                if(transform == stunt)
                    break;
                index++;
            }

            RemoveIndicator(_missionID + index, stunt);
            return true;
        }
        else
        {
            return false;
        }
    }
}

