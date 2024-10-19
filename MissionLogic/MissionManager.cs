using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    private Dictionary<Mission, NPCInteraction> _missions;

    public event EventHandler<MissionEventArgs> StartedMission;
    public event EventHandler<MissionEventArgs> FinishedMission;

    private void Awake()
    {
        _missions = new Dictionary<Mission, NPCInteraction>();
    }

    public void AddMission(Mission mission, NPCInteraction npc)
    {
        if (!_missions.ContainsKey(mission)) // If it doesn't contain the new mission yet, then add it
        {
            _missions.Add(mission, npc);
            mission.MissionCompleted += MissionCompleted;

            StartedMission?.Invoke(this, new MissionEventArgs(mission));

            mission._missionIsActive = true;


            mission.WhenStartingMission();
        }
    }

    public void RemoveMission(Mission mission)
    {
        if (_missions.ContainsKey(mission)) // If it contains the mission, then delete it
        {
            mission._missionIsActive = false;

            mission.MissionCompleted -= MissionCompleted; // Unsubscribe from the event
            _missions.Remove(mission);

            FinishedMission?.Invoke(this, new MissionEventArgs(mission));
        }
    }

    public bool CheckIfMissionIsActive(Mission mission)
    {
        return _missions.ContainsKey(mission); // Return true if the mission is active
    }

    private void MissionCompleted(object sender, MissionEventArgs args)
    {
        // Gets called when a mission is completed and gives you the mission that was completed
        Mission completedMission = args.Mission;

        Debug.Log("Mission completed: " + completedMission);

        completedMission.RemoveIndicatorAfterMission(completedMission._missionID);

        Debug.Log(_missions[completedMission]);

        if (_missions[completedMission] != null)
            _missions[completedMission].CompletedQuest();

        RemoveMission(completedMission);
    }
}
