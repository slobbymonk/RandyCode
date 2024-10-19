using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManagerView : MonoBehaviour
{
    [Header("Cards")]
    [SerializeField] private GameObject _missionCardPrefab;
    [SerializeField] private Transform _missionCardsHolder;
    private Dictionary<string, MissionCardView> _activeCards;

    [Header("Card Animation")]
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float slideOutDuration = 0.5f;
    [SerializeField] private float slideDistance = 1000f;

    [Header("Connected Managers")]
    [SerializeField] private MissionManager missionManager;
    [SerializeField] private DirectionIndicatorManager indicatorManager;

    private bool cardsAreActive;

    private void Awake()
    {
        if (missionManager == null) missionManager = GetComponent<MissionManager>();
        missionManager.StartedMission += StartNewMission;
        missionManager.FinishedMission += FinishMission;

        _activeCards = new Dictionary<string, MissionCardView>();

        if (_missionCardsHolder == null) _missionCardsHolder = transform;

        // Hide existing cards and set their positions to off-screen
        foreach (Transform child in _missionCardsHolder)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (InputHandler.GetInput._cardsAreActive)
        {
            if (!cardsAreActive)
            {
                ShowAllCards();
                cardsAreActive = true;
            }
        }
        else
        {
            if (cardsAreActive)
            {
                HideAllCards();
                cardsAreActive = false;
            }
        }
    }

    private void StartNewMission(object sender, MissionEventArgs args)
    {
        MakeNewMissionCard(args.Mission._missionID, args.Mission._missionTitle, args.Mission._missionContent);
        int i = 0;
        foreach (var location in args.Mission._missionLocation)
        {
            if (i != 0)
                indicatorManager.AddDirectionIndicator(args.Mission._missionID + i, args.Mission._missionMinimapSprite, location);
            else
                indicatorManager.AddDirectionIndicator(args.Mission._missionID, args.Mission._missionMinimapSprite, location);
            i++;
        }
    }

    private void FinishMission(object sender, MissionEventArgs args)
    {
        RemoveMissionCard(args.Mission._missionID);
        indicatorManager.RemoveDirectionIndicator(args.Mission._missionID);
    }

    public void MakeNewMissionCard(string missionID, string missionTitle, string missionContent)
    {
        var newCard = Instantiate(_missionCardPrefab, _missionCardsHolder);

        var cardTransform = newCard.transform;
        cardTransform.position = _missionCardsHolder.position - (Vector3.right * slideDistance); // Set initial off-screen position
        newCard.SetActive(false); // Initially hide the card

        var newCardComponent = newCard.GetComponent<MissionCardView>();
        _activeCards.Add(missionID, newCardComponent);
        newCardComponent._title.text = missionTitle;
        newCardComponent._content.text = missionContent;

        // Do not animate here immediately, as it will be handled by ShowAllCards()
    }

    public void RemoveMissionCard(string missionID)
    {
        if (_activeCards.TryGetValue(missionID, out var card))
        {
            Destroy(card.gameObject);
        }
        Debug.Log(card);
        _activeCards.Remove(missionID);
    }

    public void ShowAllCards()
    {
        foreach (var card in _activeCards.Values)
        {
            AnimateCardIn(card.transform);
        }
    }

    public void HideAllCards()
    {
        foreach (var card in _activeCards.Values)
        {
            AnimateCardOut(card.transform);
        }
    }

    #region Animation

    private void AnimateCardIn(Transform card)
    {
        // Stop any ongoing animation on the card to prevent conflicts
        card.DOKill();

        // Move the card off-screen to the left initially
        Vector3 offScreenPosition = new Vector3(_missionCardsHolder.position.x - slideDistance, card.position.y, card.position.z);
        card.position = offScreenPosition;

        // Now, activate the card after it's off-screen
        card.gameObject.SetActive(true);

        // Ensure the Vertical Layout Group calculates positions before animating
        Canvas.ForceUpdateCanvases();

        // Slide the card in (only adjusting X)
        card.DOMoveX(_missionCardsHolder.position.x, slideInDuration).SetEase(Ease.OutCubic);
    }

    private void AnimateCardOut(Transform card)
    {
        // Stop any ongoing animation on the card to prevent conflicts
        card.DOKill();

        // Slide the card out (only adjusting X)
        Vector3 offScreenPosition = new Vector3(_missionCardsHolder.position.x - slideDistance, card.position.y, card.position.z);
        card.DOMoveX(offScreenPosition.x, slideOutDuration).SetEase(Ease.InCubic).OnComplete(() => card.gameObject.SetActive(false));
    }

    #endregion

    public List<MissionCardView> GetAllActiveCards()
    {
        return new List<MissionCardView>(_activeCards.Values);
    }

}
