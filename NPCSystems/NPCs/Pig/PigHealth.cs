using UnityEngine;

public class PigHealth : MonoBehaviour
{
    [SerializeField] private int _maxHealthPoints;
    private int currentHealthPoints;

    [SerializeField] private float _hitCooldown = 3;
    private float cooled;

    [SerializeField] private PigDrone _pigDrone;
    [SerializeField] private WaypointMovement _waypointMovement;

    [HideInInspector] public CatchPigMission _catchPigMission;

    [SerializeField] private ParticleSystem _damageParticle;

    private bool _once;

    private void Awake()
    {
        currentHealthPoints = _maxHealthPoints;

        if(_pigDrone == null) _pigDrone = GetComponent<PigDrone>();
        if(_waypointMovement == null) _waypointMovement = GetComponent<WaypointMovement>();
    }
    private void Update()
    {
        if (!_once)
        {
            if (cooled > 0)
            {
                cooled -= Time.deltaTime;
            }

            if (currentHealthPoints <= 0)
            {
                _pigDrone.ChangeActiveState(false);

                Invoke("FinishMission", 2);

                _once = true;
            }
        }
    }
    void FinishMission()
    {
        _catchPigMission.CaughtPig();
    }
    public void LoseHealthPoint()
    {
        if (_pigDrone.droneIsActive)
        {
            if (cooled <= 0)
            {
                if (_waypointMovement.TriggerWaypoint())
                {
                    currentHealthPoints--;
                    cooled = _hitCooldown;

                    var emmision = _damageParticle.emission;
                    emmision.rateOverTime = (_maxHealthPoints - currentHealthPoints) * 1.5f;
                }
            }
        }
    }

    public float GetHealth()
    {
        return currentHealthPoints;
    }
}



