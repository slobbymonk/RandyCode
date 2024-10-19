using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue Container")]
public class DialogueData : ScriptableObject
{
    public List<DialogueEntry> dialogues = new List<DialogueEntry>();
}

[Serializable]
public class DialogueEntry
{
    public string ID; // Unique identifier for each entry
    [TextArea(4, 4)] public string en; // English translation
    public string es; // Spanish translation
    public string nl; // Dutch translation
    public string fr; // French translation
    public string pt; // Portuguese translation
    public string zh; // Chinese translation

    // Remove timeOnScreen variable
    // ...

    // Method to create a dictionary of translations
    public Dictionary<string, string> GetTranslations()
    {
        return new Dictionary<string, string>
        {
            { "en", en },
            { "es", es },
            { "nl", nl },
            { "fr", fr },
            { "pt", pt },
            { "zh", zh }
        };
    }
}




/*
public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;
    public static DialogueManager Instance => instance;

    private DialogueData dialogueData = new DialogueData();
    private string filePath;

    public string CurrentLanguage = "en";

    private void Start()
    {
        if (instance == null)
        {
            // Subscribe to the LanguageChanged event
            LanguageController.instance.LanguageChanged += OnLanguageChanged;

            instance = this;
            DontDestroyOnLoad(gameObject);
            filePath = Path.Combine(Application.dataPath, "DialogueData.json");
            LoadDialogueData();

        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        if(PlayerPrefs.GetString("LanguageIndex") != null)
            CurrentLanguage = PlayerPrefs.GetString("LanguageIndex");
    }
    private void LoadDialogueData()
    {
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            dialogueData = JsonUtility.FromJson<DialogueData>(jsonData);
        }
        else
        {
            SaveDialogueData(); // Create a new JSON file if it doesn't exist
        }
    }

    public void SaveDialogueData()
    {
        Debug.Log("Saving dialogues to file.");
        string jsonData = JsonUtility.ToJson(dialogueData, true);
        File.WriteAllText(filePath, jsonData);
    }

    *//*public string GetTranslation(string dialogueId, string languageCode)
    {
        DialogueEntry entry = dialogueData.dialogues.Find(d => d.id == dialogueId);
        if (entry == null)
        {
            Debug.LogError($"Dialogue ID {dialogueId} not found in JSON");
            return null;
        }

        var field = typeof(DialogueEntry).GetField(languageCode);
        if (field == null)
        {
            Debug.LogError($"Language code {languageCode} not found in DialogueEntry");
            return entry.en; // Fallback to English if the language field is not found
        }

        return field.GetValue(entry) as string;
    }*/


/*public void AddOrUpdateDialogue(string dialogueId, string enText, string esText = null, string nlText = null, string frText = null, string deText = null)
{
    DialogueEntry entry = dialogueData.dialogues.Find(d => d.id == dialogueId);
    if (entry == null)
    {
        entry = new DialogueEntry
        {
            //id = dialogueId,
            en = enText,
            es = esText,
            nl = nlText,
            fr = frText,
            de = deText
        };
        dialogueData.dialogues.Add(entry);
    }
    else
    {
        entry.en = enText;
        entry.es = esText ?? entry.es;
        entry.nl = nlText ?? entry.nl;
        entry.fr = frText ?? entry.fr;
        entry.de = deText ?? entry.de;
    }

    SaveDialogueData();
    Debug.Log($"JSON file saved at: {Path.Combine(Application.dataPath, "DialogueData.json")}");
}*/

/*public void AddAllDialoguesToJSON()
{
    DialogueExtractor.ExtractAllDialogues();
}*//*



private void OnDestroy()
{
    // Unsubscribe from the LanguageChanged event when this object is destroyed
    if (LanguageController.instance != null)
    {
        LanguageController.instance.LanguageChanged -= OnLanguageChanged;
    }
}

private void OnLanguageChanged(object sender, EventArgs e)
{
    Debug.Log("Changed language");
    // Update CurrentLanguage from PlayerPrefs and refresh the dialogue on all NPCs
    CurrentLanguage = PlayerPrefs.GetString("LanguageIndex");
    //RefreshAllNPCDialogues();
}

*//*private void RefreshAllNPCDialogues()
{
    // Find all NPCInteraction instances and reinitialize their dialogue
    NPCInteraction[] allNPCInteractions = GameObject.FindObjectsOfType<NPCInteraction>();
    foreach (NPCInteraction npcInteraction in allNPCInteractions)
    {
        npcInteraction.InitializeDialogue();
    }
}*//*
}*/