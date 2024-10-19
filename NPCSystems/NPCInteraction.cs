using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Represents a localized interaction with the NPC.
/// </summary>
[Serializable]
public struct Interaction
{
    [Tooltip("Unique ID used for localization purposes.")]
    public string id;

    [Tooltip("The default text shown in English or as a fallback.")]
    public string text;

    [Tooltip("How long the interaction text will stay on the screen.")]
    public float timeOnScreen;
}

/// <summary>
/// Represents animation triggers tied to specific NPC states.
/// </summary>
[Serializable]
public struct InteractionAnimation
{
    [Tooltip("The NPC state that triggers this animation.")]
    public NPCInteraction.State state;

    [Tooltip("The name of the animation trigger to fire.")]
    public string animationTrigger;
}

/// <summary>
/// Abstract class to handle NPC interaction logic, which can be extended to create custom NPCs.
/// </summary>
public abstract class NPCInteraction : MonoBehaviour
{
    #region Field
    [Tooltip("Reference to the coroutine running the interaction.")]
    public Coroutine _interactionCoroutine;

    [Tooltip("Current state of the NPC.")]
    [SerializeField] protected State _state;

    [Header("Interactions")]
    [Tooltip("Set of interactions when the NPC is not interactable.")]
    public DialogueData _notInteractable;

    [Tooltip("First set of interactions when the player first talks to the NPC.")]
    public DialogueData _firstInteractions;

    [Tooltip("Interactions when the NPC is waiting for the player to complete a quest.")]
    public DialogueData _waitingForQuestCompletionInteractions;

    [Tooltip("Interactions available after the quest is completed.")]
    public DialogueData _questCompletedInteraction;

    [Tooltip("Interactions available after the NPC has finished interacting.")]
    public DialogueData _finishedInteractions;

    [Header("Text Logic")]

    private GameObject _textPanel;
    private TMP_Text _textField;

    [Tooltip("Speed at which the dialogue text is typed.")]
    [SerializeField] private float _typingSpeed;

    [Header("Mission")]
    [Tooltip("Reference to the mission manager responsible for assigning and tracking quests.")]
    [SerializeField] protected MissionManager missionManager;

    [Tooltip("Mission that the NPC gives to the player.")]
    public Mission _missionToGive;

    [Header("Animation")]
    [Tooltip("Set of animations triggered during different interaction states.")]
    [SerializeField] protected InteractionAnimation[] _interactionAnimations;

    [Tooltip("Animator component responsible for handling NPC animations.")]
    [SerializeField] private Animator _animator;

    [Header("Extra")]
    [Tooltip("Text displayed in the HUD after completing the quest.")]
    [SerializeField] protected TMP_Text _hudText;

    [Tooltip("Text to display in the HUD after the quest is completed.")]
    [SerializeField] protected string _afterQuestCompletionHUDText;

    [Header("Talking Effect")]
    [Tooltip("Strength of the force applied to the NPC's head while talking.")]
    [SerializeField] private Vector2 _talkingForceStrength;

    [Tooltip("Rigidbody component used for applying forces to the NPC's head when talking.")]
    [SerializeField] private Rigidbody _headRb;

    [Tooltip("Position where the NPC is at start in the game world.")]
    public Vector3 _spawnPosition;

    [Tooltip("Determines if the player needs confirmation to interact with the NPC.")]
    [SerializeField] private bool _needsConfirmationToInteract;

    [Tooltip("Indicates if the NPC gives a mission.")]
    [SerializeField] private bool _givesMission = true;

    public enum State
    {
        Disabled,
        NotInteractable,
        Uninteracted,
        Talking,
        WaitingForQuestCompletion,
        QuestCompletedWaitingForFinalTalk,
        Finished
    }

    [Header("Dialogue Variables")]
    public GameObject textPanelPrefab;

    [Header("Cameras")]
    public GameObject gameCam;
    public GameObject dialogueCam;

    public bool canExit;
    public bool inDialogue;
    public bool nextDialogue;

    [Header("Interaction Animations")]
    public VillagerData villagerData;

    public bool villagerIsTalking;

    public TMP_Animated animatedText;
    private DialogueAudio dialogueAudio;
    //private Animator animator;
    public Renderer eyesRenderer;

    public Transform particlesParent;
    #endregion
    private void Awake()
    {
        SetupTextPanel();
    }
    void Start()
    {
        BeforeStart();

        if (_animator == null) _animator = GetComponent<Animator>();

        missionManager = FindObjectOfType<MissionManager>();

        _spawnPosition = transform.position;


        //Villager Logic
        dialogueAudio = GetComponent<DialogueAudio>();
        //animator = GetComponent<Animator>();

        IfACameraIsNotSetMakeItMainCamera();
    }

    private void IfACameraIsNotSetMakeItMainCamera()
    {
        if (gameCam == null)
            gameCam = Camera.main.gameObject;
        if (dialogueCam == null)
            dialogueCam = Camera.main.gameObject;
    }

    #region Text Bubbles
    private void SetupTextPanel()
    {
        // Automatically create the text panel for this NPC
        _textPanel = Instantiate(textPanelPrefab);
        _textField = _textPanel.GetComponentInChildren<TMP_Animated>();

        _textPanel.GetComponent<UIFollowWorldObject>().worldObject = transform;
        _textPanel.GetComponent<TextBalloonFollow>().worldObject = transform;

        animatedText = _textField.GetComponent<TMP_Animated>();
        animatedText.onEmotionChange.AddListener((newEmotion) => EmotionChanger(newEmotion));
        animatedText.onAction.AddListener((action) => SetAction(action));

        if(villagerData != null)
        {
            // Set vilager data
            _textField.color = villagerData.villagerNameColor;
            var allImages = _textPanel.GetComponentsInChildren<Image>();
            foreach (var image in allImages)
            {
                image.color = villagerData.villagerColor;
            }
        }
        else
        {
            _textField.color = Color.red;
            var allImages = _textPanel.GetComponentsInChildren<Image>();
            foreach (var image in allImages)
            {
                image.color = Color.black;
            }
        }

        _textPanel.SetActive(false);
    }

    public void CameraChange(bool isDialogue)
    {
        // Switch between game and dialogue camera
        if(gameCam != dialogueCam)
        {
            gameCam.SetActive(!isDialogue);
            dialogueCam.SetActive(isDialogue);
        }
    }
    #endregion

    #region Interaction Dialogue Logic
    public void StartInteraction()
    {
        Debug.Log("Started Interaction");
        if (_interactionCoroutine == null)
        {
            switch (_state)
            {
                case State.NotInteractable:
                    _interactionCoroutine = StartCoroutine(PlayNotInteractable());
                    break;
                case State.Uninteracted:
                    _interactionCoroutine = StartCoroutine(PlayFirstInteractions());
                    break;
                case State.WaitingForQuestCompletion:
                    _interactionCoroutine = StartCoroutine(PlayWaitingInteractions());
                    break;
                case State.Finished:
                    _interactionCoroutine = StartCoroutine(PlayFinishedInteractions());
                    break;
                case State.QuestCompletedWaitingForFinalTalk:
                    _interactionCoroutine = StartCoroutine(PlayQuestCompletedInteraction());
                    break;
            }

            InteractionChangeState(State.Talking);
        }
    }

    IEnumerator PlayNotInteractable()
    {
        yield return PlayInteraction(_notInteractable);
        InteractionChangeState(State.NotInteractable);
        AfterNotInteractibleInteraction();
    }

    IEnumerator PlayFirstInteractions()
    {
        yield return PlayInteraction(_firstInteractions);
        StartQuest();
    }

    IEnumerator PlayQuestCompletedInteraction()
    {
        GetSomethingAfterQuestCompletionInteractionStarted();
        yield return PlayInteraction(_questCompletedInteraction);
        InteractionChangeState(State.Finished);
        GetSomethingAfterQuestCompletionInteractionCompleted();

        // After completion HUD text
        if (_hudText != null)
        {
            _hudText.gameObject.SetActive(true);
            _hudText.text = _afterQuestCompletionHUDText;
        }
    }

    IEnumerator PlayWaitingInteractions()
    {
        yield return PlayInteraction(_waitingForQuestCompletionInteractions);
        InteractionChangeState(State.WaitingForQuestCompletion);
    }

    IEnumerator PlayFinishedInteractions()
    {
        yield return PlayInteraction(_finishedInteractions);
        InteractionChangeState(State.Finished);
    }
    string GetTextInCurrentLanguage(DialogueEntry dialogue)
    {
        string dialogueLanguage = PlayerPrefs.GetString("LanguageIndex");
        Dictionary<string, string> translations = dialogue.GetTranslations();

        if (translations.TryGetValue(dialogueLanguage, out string text))
        {
            return text;
        }

        // Optionally return a default language or a fallback if the requested language is not found
        return translations.TryGetValue("en", out string defaultText) ? defaultText : null;
    }
    private IEnumerator PlayInteraction(DialogueData interactionsToPlay)
    {
        if (_textPanel != null)
        {
            _textPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("TextPanel is not assigned.");
            yield break;
        }

        if (_textField == null)
        {
            Debug.LogError("TextField is not assigned.");
            yield break;
        }

        Talk();

        int index = 0;
        foreach (var dialogue in interactionsToPlay.dialogues)
        {
            string currentText = GetTextInCurrentLanguage(dialogue);


            TMP_Animated animatedText = _textField.GetComponent<TMP_Animated>();
            animatedText.ReadText(currentText);

            Canvas.ForceUpdateCanvases();
            _textField.transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = false; 
            _textField.transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = true;

            // Wait for the dialogue to finish revealing before allowing input
            while (animatedText.maxVisibleCharacters < animatedText.text.Length)
            {
                yield return null;
            }

            

            // Wait for the player to press the button to advance to the next line
            while (!InputHandler.GetInput.GetInteractionPressed())
            {
                yield return null; // Wait until the player presses the button
            }
            index++;
            if (index >= interactionsToPlay.dialogues.Count)
            {
                Debug.Log("Index" + index);
                Debug.Log("Dialogues" + interactionsToPlay.dialogues.Count);
                EndInteraction();
            }
        }
        canExit = true;
    }
    public void EndInteraction()
    {
        CameraChange(false);
        Canvas.ForceUpdateCanvases();
        inDialogue = false;

        if (_textPanel != null)
        {
            _textPanel.SetActive(false);
        }

        _interactionCoroutine = null;

        TriggeredAfterInteraction();
    }


    #endregion

    #region Interaction Extra logic
    public void SkipInteraction()
    {
        if (_interactionCoroutine != null && _state == State.Talking)
        {
            StopCoroutine(_interactionCoroutine);
            _interactionCoroutine = null;
        }
        StartQuest();
    }

    protected void StartQuest()
    {
        Debug.Log(gameObject.name + " Started Quest");
        if (_givesMission)
        {
            if (_missionToGive != null) missionManager.AddMission(_missionToGive, this);
            StartQuestExtraLogic();
        }

        InteractionChangeState(State.WaitingForQuestCompletion);
    }

    public void CompletedQuest()
    {
        if (_interactionCoroutine == null)
        {
            AfterQuestionCompletion();

            InteractionChangeState(State.QuestCompletedWaitingForFinalTalk);
            _missionToGive._missionIsActive = false;
        }
    }
    #endregion

    #region Overridable Extra Logic
    protected virtual void StartQuestExtraLogic() { }
    protected virtual void AfterQuestionCompletion() { }
    protected virtual void AfterNotInteractibleInteraction() { }
    protected virtual void TriggeredAfterInteraction() { }

    //Quest completion interaction
    protected virtual void GetSomethingAfterQuestCompletionInteractionStarted() { }
    protected abstract void GetSomethingAfterQuestCompletionInteractionCompleted();
    #endregion


    private void Talk()
    {
        if (_headRb != null)
        {
            var randomForce = Random.Range(_talkingForceStrength.x, _talkingForceStrength.y);
            _headRb.AddForce(transform.up * randomForce, ForceMode.Impulse);
        }
    }

    //Change state without override current state
    private void InteractionChangeState(State state)
    {
        _state = state;

        foreach (var animation in _interactionAnimations)
        {
            if (animation.state == state)
            {
                _animator.SetTrigger(animation.animationTrigger);
            }
        }
    }
    /// <summary>
    /// Change State and override current state
    /// </summary>
    /// <param name="ChangeNPCState"></param>
    public void ChangeNPCState(State newState)
    {
        // Stop any current interaction
        if (_interactionCoroutine != null)
        {
            StopCoroutine(_interactionCoroutine);
            _interactionCoroutine = null;

            // If the NPC was talking, stop talking
            if (_textPanel != null)
            {
                _textPanel.SetActive(false); // Hide the text panel
            }
        }

        // Change the state to the new one
        _state = newState;

        // Trigger the corresponding animation for the new state, if any
        foreach (var animation in _interactionAnimations)
        {
            if (animation.state == newState)
            {
                _animator.SetTrigger(animation.animationTrigger);
            }
        }

        // Perform any additional logic after changing the state, if necessary
        TriggeredAfterStateChange(newState);
    }

    protected virtual void TriggeredAfterStateChange(State newState)
    {
        // This method can be overridden in child classes to provide additional behavior when state changes
    }

    protected virtual void BeforeStart() { }

    private void Update()
    {
        DuringUpdate();
    }

    protected virtual void DuringUpdate() { }

    #region Localisation
    private void OnEnable()
    {
        if (LanguageController.instance != null)
        {
            LanguageController.instance.LanguageChanged += OnLanguageChanged;
        }
    }

    private void OnDisable()
    {
        if (LanguageController.instance != null)
        {
            LanguageController.instance.LanguageChanged -= OnLanguageChanged;
        }
    }
    private void OnLanguageChanged(object sender, EventArgs e)
    {
        // Reinitialize dialogue to reflect the new language
        //InitializeDialogue();

        // If there's an active interaction, stop it and restart with updated text
        if (_interactionCoroutine != null)
        {
            StopCoroutine(_interactionCoroutine);
            _interactionCoroutine = StartCoroutine(ResumeCurrentInteraction());
        }
    }
    #endregion
    private IEnumerator ResumeCurrentInteraction()
    {
        switch (_state)
        {
            case State.NotInteractable:
                yield return PlayInteraction(_notInteractable);
                break;
            case State.Uninteracted:
                yield return PlayInteraction(_firstInteractions);
                break;
            case State.WaitingForQuestCompletion:
                yield return PlayInteraction(_waitingForQuestCompletionInteractions);
                break;
            case State.Finished:
                yield return PlayInteraction(_finishedInteractions);
                break;
            case State.QuestCompletedWaitingForFinalTalk:
                yield return PlayInteraction(_questCompletedInteraction);
                break;
        }
    }


    #region Interaction Animations - Integrated from Mix & Jam's Dialogue system

    
    //Change facial expressions when a tag calls it
    public void EmotionChanger(Emotion e)
    {
        if (_animator != null)
            _animator.SetTrigger(e.ToString());

        if(eyesRenderer != null)
        {
            if (e == Emotion.suprised)
                eyesRenderer.material.SetTextureOffset("_BaseMap", new Vector2(.33f, 0));

            if (e == Emotion.angry)
                eyesRenderer.material.SetTextureOffset("_BaseMap", new Vector2(.66f, 0));

            if (e == Emotion.sad)
                eyesRenderer.material.SetTextureOffset("_BaseMap", new Vector2(.33f, -.33f));
        }
    }

    //Set custom actions when a tag calls it
    public void SetAction(string action)
    {

        if (action == "shake")
        {
            Debug.Log("Action has been played: " + action);
            FindObjectOfType<CameraShake>().ShakeCamera(.3f, .1f);
        }
        else
        {
            PlayParticle(action);

            if (action == "sparkle")
            {
                dialogueAudio.effectSource.clip = dialogueAudio.sparkleClip;
                dialogueAudio.effectSource.Play();
            }
            else if (action == "rain")
            {
                dialogueAudio.effectSource.clip = dialogueAudio.rainClip;
                dialogueAudio.effectSource.Play();
            }
        }
    }

    //Play a particle when called for
    public void PlayParticle(string x)
    {
        if (particlesParent.Find(x + "Particle") == null)
            return;
        particlesParent.Find(x + "Particle").GetComponent<ParticleSystem>().Play();
    }
    //Reset everything to do with tags
    public void Reset()
    {
        if (_animator != null)
            _animator.SetTrigger("normal");
            eyesRenderer.material.SetTextureOffset("_BaseMap", Vector2.zero);
    }

    //Turn towards player
    public void TurnToPlayer(Vector3 playerPos)
    {
        transform.DOLookAt(playerPos, Vector3.Distance(transform.position, playerPos) / 5);
        string turnMotion = isRightSide(transform.forward, playerPos, Vector3.up) ? "rturn" : "lturn";
        if(_animator != null)
            _animator.SetTrigger(turnMotion);
    }
    public bool isRightSide(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 right = Vector3.Cross(up.normalized, fwd.normalized);        // right vector
        float dir = Vector3.Dot(right, targetDir.normalized);
        return dir > 0f;
    }
    #endregion
}