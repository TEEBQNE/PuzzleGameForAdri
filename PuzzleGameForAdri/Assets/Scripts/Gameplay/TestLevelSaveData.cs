public class TestLevelSaveData : RequiresSaveLoad
{
    /*
        In the shape manager need to have all shape sprites indexed
        Need to have the shapeScript hold a ref to the Shape object that is passed upwards

        Need to determine how I want the refs setup to hold what data

        Overall manager [Shape Sprites]
        Player driven manager [Colors (except for white / black), background / goal color index, all shape data]

        Player driven shapes [Position, scale, shape index, color index, if movable]

        Will need a new prefab that acts like the shapeScript but is not a gameObject
     */

    public override void Load(SaveLoadFolderNames folder, SaveLoadFileNames file)
    {
        throw new System.NotImplementedException();
    }

    public override void PrintCurrentSaveData()
    {
        throw new System.NotImplementedException();
    }

    public override void Save(SaveLoadFolderNames folder, SaveLoadFileNames file, bool isCloudSave = false)
    {
        throw new System.NotImplementedException();
    }
}