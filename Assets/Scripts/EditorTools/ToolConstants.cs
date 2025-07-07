using UnityEngine;

namespace Tools.ToolConstants
{
    public static class ToolConstants
    {
        public static float _rectWidth = 1f;
        public static float _rectHeight = 1f;

        public static Color _faceColour = new Color(1, 0, 0, 0.23f);
        public static Color _outlineColour = Color.black;
    }

    public static class ColourConstants
    {
        public static Color[] _characterColours = new Color[3]
        {
            ToolConstants._faceColour,
            new Color(0, 1, 0.1f, 0.23f),
            new Color(0, 0.1f, 1f, 0.23f)
        };

        public static Color getCharacterColours(CharacterType typeOfChar)
        {
            if (_characterColours[(int)typeOfChar] == null)
            { return _characterColours[0]; }

            Color returningColour = _characterColours[(int)typeOfChar];
            return returningColour;
        }
    }
}

