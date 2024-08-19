using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SaveSystem : MonoBehaviour
{
    [SerializeField] private string saveFileName = "SaveData";

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            Save();
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            Load();
        }
    }

    private void Load()
    {
        if (!FileManager.LoadFromFile("SaveData", out var data)) return;
        
        var saveData = new SaveData();
        saveData.LoadFromJson(data);
        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();
        foreach (var saveable in saveables)
        {
            saveable.LoadFromSaveData(saveData);
        }
    }

    private void Save()
    {
        var saveData = new SaveData();
        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();
        foreach (var saveable in saveables)
        {
            saveable.PopulateSaveData(saveData);
        }

        FileManager.WriteToFile(saveFileName, saveData.ToJson());
    }
}