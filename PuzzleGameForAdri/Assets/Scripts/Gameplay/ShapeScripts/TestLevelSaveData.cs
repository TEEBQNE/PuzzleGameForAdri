using UnityEngine;

public class TestLevelSaveData : RequiresSaveLoad
{
    [SerializeField] private ShapeManager _shapeManager = null;

    public override void Load(SaveLoadFolderNames folder, SaveLoadFileNames file)
    {
        _shapeManager.LoadLevelData(SaveLoad<SaveLoadStructures.Level>.Load(folder, file) ?? new SaveLoadStructures.Level());
    }

    // ToDo TJC: Need to add a print to the manager which calls the child shapes
    // Also need the prints in the child shapes
    public override void PrintCurrentSaveData()
    {
        SaveLoadStructures.Level data = _shapeManager.SaveLevelData();

        Debug.Log("Goal: " + data.goalBackgroundColor);
        Debug.Log("Start: " + data.startingBackgroundColor);

        string colors = "";

        foreach(Color color in data.shapeColors)
        {
            colors += "Color: " + color + " ";
        }

        Debug.Log(colors);

        foreach(SaveLoadStructures.Shape shapes in data.shapes)
        {
            Debug.Log("Position: " + shapes.position + " Scale: " + shapes.scale + " Rotation: " + shapes.rotation + " Color: " + shapes.colorIndex + " Shape Index: " + shapes.shapeIndex + " Can Move: " + shapes.canBeMoved);
        }
    }

    public override void Save(SaveLoadFolderNames folder, SaveLoadFileNames file)
    {
        SaveLoad<SaveLoadStructures.Level>.Save(_shapeManager.SaveLevelData(), folder, file);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            PrintCurrentSaveData();
        }

    }
}