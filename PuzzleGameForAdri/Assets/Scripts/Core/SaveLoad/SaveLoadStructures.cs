using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadStructures
{
    [System.Serializable]
    public class LevelCompletion
    {
        public LevelCompletion(Guid userId, Guid levelGuid, bool isCompleted)
        {
            _levelGuid = levelGuid;
            _isCompleted = isCompleted;
            _userId = userId;
        }

        public Guid LevelGuid => _levelGuid;
        public Guid UserId => _userId;
        public bool IsCompleted => _isCompleted;

        private Guid _levelGuid;
        private Guid _userId;
        private bool _isCompleted;
    }

    [System.Serializable]
    public class LevelPackCompletion
    {
        public LevelPackCompletion(List<LevelCompletion> levelsCompleted)
        {
            _levelsCompleted = levelsCompleted;
        }

        public List<LevelCompletion> LevelsCompleted => _levelsCompleted;

        private List<LevelCompletion> _levelsCompleted = new List<LevelCompletion>();
    }

    [System.Serializable]
    public class LevelPack
    {
        public LevelPack(List<Level> levels, string packname, string packAuthorName, string packDateAuthored)
        {
            _levels = levels;
            _packName = packname;
            _packAuthorName = packAuthorName;
            _packDateAuthored = packDateAuthored;
        }

        public List<Level> Levels => _levels;
        public string PackName => _packName;
        public string PackAuthorName => _packAuthorName;
        public string PackDateAuthored => _packDateAuthored;
        public Guid CreatorId => _creatorId;

        private List<Level> _levels = new List<Level>();
        private string _packName;
        private string _packAuthorName;
        private string _packDateAuthored;
        private Guid _creatorId;

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