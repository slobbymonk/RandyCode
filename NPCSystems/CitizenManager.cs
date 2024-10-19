using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CitizenManager : MonoBehaviour
{
    [HideInInspector] public int hitCitizens;

    [Header("Oneliners")]
    [SerializeField] private OneLiners _oneliner;
    [SerializeField] private string _onelinerTriggerId = "Haha";


    [Header("Hit UI")]
    [SerializeField] private GameObject _hitUI;
    [SerializeField] private TMP_Text _hitUIText;

    private RandomStringList randomNameList;

    [Header("Double Kill")]
    [SerializeField] private float _delayForDoubleKill = 2;
    private float lastHit;
    private float doubleKillCooldown = 7;
    private float doubleKillCooledDown;


    [Header("Hit Effects")]
    private Slowmotion slowmotion;

    [SerializeField] private GameObject _mainCamera;
    [SerializeField] private GameObject _loseCamera;

    [SerializeField] private float _slowDownFactor = .02f;
    [SerializeField] private float _timeBeforeContinue = 1f;

    private void Awake()
    {
        if (_oneliner == null) _oneliner = FindObjectOfType<OneLiners>();


        if(_hitUI == null) _hitUI = GameObject.Find("HitPedestrianScreen");
        if (_hitUIText == null && _hitUI != null) _hitUIText = _hitUI.transform.GetChild(0).GetComponent<TMP_Text>();

        randomNameList = FindObjectOfType<RandomStringList>();

        slowmotion = new Slowmotion();

        _mainCamera = Camera.main.gameObject;
    }

    public void AddHitCitizen()
    {
        hitCitizens++;

        if(Time.time - lastHit <= _delayForDoubleKill && doubleKillCooledDown <= 0)
        {
            if (_oneliner != null) _oneliner.PlayOneLiner("DoubleKill", true);
            doubleKillCooledDown = doubleKillCooldown;
        }
        else
        {
            if (Random.Range(0, 2) == 1)
                TextHit();
            else OneLinerHit();
        }

        lastHit = Time.time;
        doubleKillCooledDown -= Time.deltaTime;
    }

    private void OneLinerHit()
    {
        if (_oneliner != null) _oneliner.PlayOneLiner(_onelinerTriggerId, true);
    }

    private void TextHit()
    {
        HandleHitText();

        _hitUIText.text = randomNameList.GetRandomName();
    }

    public int GetHitCitizenAmount()
    {
        return hitCitizens;
    }




    #region Effect
    private void HandleHitText()
    {
        HandleLogic(true, true);

        StartCoroutine(Continue());
    }
    void HandleLogic(bool state, bool slowMotionState)
    {
        _mainCamera.SetActive(!state);
        _loseCamera.SetActive(state);

        _hitUI.SetActive(state);

        slowmotion.ToggleSlowMotion(slowMotionState, _slowDownFactor);
    }
    private IEnumerator Continue()
    {
        yield return new WaitForSecondsRealtime(_timeBeforeContinue);

        HandleLogic(false, false);
    }
    #endregion
}
