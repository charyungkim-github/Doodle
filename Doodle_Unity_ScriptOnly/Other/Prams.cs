using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

#region Enum

public enum ModeStatus
{
    Drawing,
    Transform,
    Setting
};

public enum SubMenuModeStatus
{
    Color,
    Size,
    DoneSelection,
    DoneMoving
}

public enum DrawingModeStatus
{
    Create,
    AddPoint,
    DoneCreate,
    ChangeBrush
}

public enum GestureStatus
{
    StartDrag,
    Dragging,
    DoneDragging,
    None
};



#endregion

#region AnimatorTrigger

public static class AnimatorTrigger
{
    public static string onTrigger = "On";
    public static string offTrigger = "Off";
}

#endregion

#region Preset

public static class Preset
{
    private static Color[] _presetColors = new Color[]
    {
        new Color(0, 0, 0), // empty

        new Color(1, 1, 1), // white    
        new Color(0.7411765f, 0.2627451f, 0.3137255f), // red    
        new Color(0f, 0.764706f, 0.3411765f),  // green
        new Color(1f, 0.7686275f, 0f), // yellow
        new Color(0f, 0.7176471f, 1f), // blue   
    };

    public static int presetColorCount = _presetColors.Length - 1;

    public static Color presetColor(int _index)
    {
        return _presetColors[_index];
    }

    public static Color[] presetColors
    {
        get 
        {
            return _presetColors;
        }
        set 
        {
            _presetColors = value;
        }
    }

    // preset for size
    private static float[] _presetSize = new float[]
    {
            0,
            0.003f, 0.01f, 0.02f, 0.03f
    };

    public static float presetSize(int _index)
    {
        return _presetSize[_index];
    }

    public static int presetSizeCount = _presetSize.Length - 1;

    // preset for cursor size
    private static float[] _presetCursorSize = new float[]
    {
            0,
            0.0005f, 0.001f, 0.002f, 0.003f
    };

    public static float presetCursorSize(float _size)
    {
        for (int i = 1; i < _presetSize.Length; i++)
        {
            if (_size == presetSize(i))
                return _presetCursorSize[i];
        }
        return -1;
    }
}

#endregion

#region Custom Position

public static class CustomPositions
{
    // size value position on content
    public static float sizeContentHeight = 175f;
    public static float sizeContentArrangeHeight = 5f;

    // size value position on icon size
    private static float[] _sizeIconPosition = new float[]
    {
        0f,
        -173f, -103f, -31f, 0f
    };

    public static float sizeIconPosition(int _index)
    {
        return _sizeIconPosition[_index];
    }
}

#endregion

#region Delete Object Params

public static class DeleteObjectParams
{
    private static bool _isSelectedOnDeleteObject = false;
    public static bool isSelectedOnDeleteObject
    {
        get
        {
            return _isSelectedOnDeleteObject;
        }
        set
        {
            _isSelectedOnDeleteObject = value;
        }
    }

    private static GameObject _selectedDeleteObject = null;
    public static GameObject selectedDeleteObject
    {
        get
        {
            return _selectedDeleteObject;
        }
        set
        {
            _selectedDeleteObject = value;
        }
    }
}

#endregion

#region Unity Mode

public static class UnityMode
{
    public static bool isEditor = _isDebug();

    private static bool _isDebug()
    {
        #if UNITY_EDITOR
                return true;
        #else
              return false;
        #endif
    }
}

#endregion