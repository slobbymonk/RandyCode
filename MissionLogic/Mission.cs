using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mission : MonoBehaviour
{
    public abstract event EventHandler<MissionEventArgs> MissionCompleted;
    public string _missionID;

    public string _missionTitle;
    public string _missionContent;

    public List<Transform> _missionLocation;

    public Sprite _missionMinimapSprite;


    [HideInInspector] public bool _missionIsActive;

    private DirectionIndicatorManager directionIndicator;


    //Now supports multiple indicators that each get removed individually
    private void Start()
    {
        BeforeStart();

        directionIndicator = FindObjectOfType<DirectionIndicatorManager>();

        if (_missionLocation == null) _missionLocation = new List<Transform>();

        AfterStart();
    }
    protected virtual void BeforeStart() { }
    protected virtual void AfterStart() { }
    public virtual void WhenStartingMission() { }
    /// <summary>
    /// Method <c>RemoveIndicator</c> removes the closest indicator belonging to this mission.
    /// </summary>
    protected void RemoveIndicator(string indicatorID, Transform location)
    {
        directionIndicator.RemoveDirectionIndicator(indicatorID);

        _missionLocation.Remove(location.transform);
    }
    public void RemoveIndicatorAfterMission(string missionID)
    {
        directionIndicator.RemoveDirectionIndicator(missionID);
    }
    public void AddNewMissionLocation(Transform location)
    {
        _missionLocation.Add(location);
    }
    public void AddLocationToMinimap(string id, Sprite sprite, Transform target)
    {
        directionIndicator.AddDirectionIndicator(id, sprite, target);
    }
}
