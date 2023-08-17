using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadStructures
{
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

    public class LevelPackCompletion
    {
        public LevelPackCompletion(List<LevelCompletion> levelsCompleted)
        {
            _levelsCompleted = levelsCompleted;
        }

        public List<LevelCompletion> LevelsCompleted => _levelsCompleted;

        private List<LevelCompletion> _levelsCompleted = new List<LevelCompletion>();
    }

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

    public class Level
    {
        public Level(List<Shape> shapes, List<Color> shapeColors, int startingBackgroundColor, int goalBackgroundColor)
        {
            _shapes = shapes;
            _shapeColors = shapeColors;
            _startingBackgroundColor = startingBackgroundColor;
            _goalBackgroundColor = goalBackgroundColor;
        }

        public List<Shape> Shapes => _shapes;
        public List<Color> ShapeColors => _shapeColors;
        public int StartingBackgroundColor => _startingBackgroundColor;
        public int GoalBackgroundColor => _goalBackgroundColor;

        private List<Shape> _shapes = new List<Shape>();
        private List<Color> _shapeColors = new List<Color>();    // [0] has to be white and [1] has to be black
        private int _startingBackgroundColor;
        private int _goalBackgroundColor;
    }

    public class Shape
    {
        public Shape(List<Shape> childShapes, Vector2 position, Vector2 scale, float rotation = 0.0f, int colorIndex = 0, int shapeIndex = 0, bool canBeMoved = true)
        {
            _childShapes = childShapes;
            _position = position;
            _scale = scale;
            _rotation = rotation;
            _colorIndex = colorIndex;
            _shapeIndex = shapeIndex;
            _canBeMoved = canBeMoved;
        }

        public List<Shape> ChildShapes => _childShapes;
        public Vector2 Position => _position;
        public Vector2 Scale => _scale;
        public float Rotation => _rotation;
        public int ColorIndex => _colorIndex;
        public int ShapeIndex => _shapeIndex;
        public bool CanBeMoved => _canBeMoved;

        public List<Shape> _childShapes = new List<Shape>();
        private Vector2 _position;
        private Vector2 _scale;
        private float _rotation;
        private int _colorIndex;
        private int _shapeIndex;
        private bool _canBeMoved;
    }
}