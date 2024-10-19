using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class DialogueDataExporter
{
    [MenuItem("Tools/Export Dialogue Data to CSV")]
    public static void ExportDialogueData()
    {
        string folderPath = "Assets/DialogueDatas"; // Change this to your folder path
        string csvFilePath = "Assets/DialogueDatas/DialogueData.csv"; // Change this to your desired output path
        string[] guids = AssetDatabase.FindAssets("t:DialogueData", new[] { folderPath });
        List<DialogueEntry> allDialogueEntries = new List<DialogueEntry>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogueData dialogueData = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (dialogueData != null)
            {
                allDialogueEntries.AddRange(dialogueData.dialogues);
            }
        }

        List<string> csvRows = new List<string>();
        // Create header
        csvRows.Add($"ID;English;Spanish;Dutch;French;Portuguese;Chinese");

        // Fill in each dialogue entry
        foreach (var entry in allDialogueEntries)
        {
            // Create a new row with each language in its own cell
            string row = $"{EscapeCsvValue(entry.ID)};{EscapeCsvValue(entry.en)};" +
                $"{EscapeCsvValue(entry.es)};{EscapeCsvValue(entry.nl)};" +
                $"{EscapeCsvValue(entry.fr)};{EscapeCsvValue(entry.pt)};{EscapeCsvValue(entry.zh)}";
            csvRows.Add(row);
        }

        // Write to CSV file
        File.WriteAllLines(csvFilePath, csvRows);
        AssetDatabase.Refresh(); // Refresh the asset database to show the new file
        Debug.Log($"Dialogue data exported to {csvFilePath}");
    }

    private static string EscapeCsvValue(string value)
    {
        // Escape double quotes by doubling them and wrap the value in quotes if necessary
        if (value.Contains(";") || value.Contains("\""))
        {
            value = "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        return value;
    }
}
