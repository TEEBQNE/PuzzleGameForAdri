using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadStructures
{
    [System.Serializable]
    public class PlayerUserData
    {
        public PlayerUserData()
        {
            userId = "";
            username = "";
        }

        public PlayerUserData(string id, string user)
        {
            userId = id;
            username = user;
        }

        public string userId;
        public string username;
    }

    [System.Serializable]
    public class LevelCompletion
    {
        public LevelCompletion(string id, string guid, bool completed)
        {
            levelGuid = id;
            isCompleted = completed;
            userId = guid;
        }

        public string levelGuid;
        public string userId;
        public bool isCompleted;
    }

    [System.Serializable]
    public class LevelPackCompletion
    {
        public LevelPackCompletion()
        {
            levelsCompleted = new List<LevelCompletion>();
        }

        public LevelPackCompletion(List<LevelCompletion> completed)
        {
            levelsCompleted = completed;
        }

        public List<LevelCompletion> levelsCompleted = new List<LevelCompletion>();
    }

    [System.Serializable]
    public class LevelPack
    {
        public LevelPack(List<Level> lvls, string pack, string authorName, string dateAuthored, string id)
        {
            levels = lvls;
            packName = pack;
            packAuthorName = authorName;
            packDateAuthored = dateAuthored;
            creatorId = id;
        }

        public List<Level> levels = new List<Level>();
        public string packName;
        public string packAuthorName;
        public string packDateAuthored;
        public string creatorId;

        // do we want basic design for the pack (?) A - non moving level can be used for the background?
            // background color, shapes, shape rotation, colors, etc.
    }

    [System.Serializable]
    public class Level
    {
        public Level()
        {
            shapes = new List<Shape>();
            shapeColors = new List<Color> { Color.white, Color.black };
            startingBackgroundColor = 0;
            goalBackgroundColor = 0;
        }

        public Level(List<Shape> childShapes, List<Color> colors, int startingColor, int goalColor)
        {
            shapes = childShapes;
            shapeColors = colors;
            startingBackgroundColor = startingColor;
            goalBackgroundColor = goalColor;
        }

        public List<Shape> shapes;
        public List<Color> shapeColors;
        public int startingBackgroundColor;
        public int goalBackgroundColor;
    }

    [System.Serializable]
    public class Shape
    {
        public Shape()
        {
            childShapes = new List<int>();
            position = Vector2.zero;
            scale = Vector2.zero;
            rotation = Quaternion.identity;
            colorIndex = 0;
            shapeIndex = ShapeNames.SQUARE;
            canBeMoved = true;
        }

        public Shape(List<int> shapes, Vector2 pos, Vector2 sz, Quaternion rot, int colIdx = 0, ShapeNames shapeIdx = 0, bool beMoved = true)
        {
            childShapes = shapes;
            position = pos;
            scale = sz;
            rotation = rot;
            colorIndex = colIdx;
            shapeIndex = shapeIdx;
            canBeMoved = beMoved;
        }

        public Vector2 position;
        public Vector2 scale;
        public Quaternion rotation;
        public int colorIndex;
        public ShapeNames shapeIndex;
        public bool canBeMoved;
        public List<int> childShapes;
    }
}