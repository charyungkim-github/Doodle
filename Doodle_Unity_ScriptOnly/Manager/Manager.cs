using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{

    //[Header("Setting UI")]
    public GameObject oDrawingPath;
    public SubMenuScrollController subMenuScrollController;
    
    // manager
    TrackingManager trackingManager;
    DrawingManager drawingManager;
    UIManager uiManager;

    // fps
    WaitForSeconds fpsTimer = new WaitForSeconds(0.5f);

    // mode
    ModeStatus currentMode;
    SubMenuModeStatus currentSubMenuMode;
    DrawingModeStatus currentDrawingMode;
    bool isTransformOn = false;

    void Start()
    {
        // manager
        trackingManager = GetComponent<TrackingManager>();
        drawingManager = GetComponent<DrawingManager>();
        uiManager = GetComponent<UIManager>();

        // setup manager
        trackingManager.DoStart(this);
        drawingManager.DoStart(oDrawingPath);
        uiManager.DoStart(this, oDrawingPath, GameObject.FindGameObjectWithTag("DefaultCursor"));
        subMenuScrollController.DoStart(this, 1, 1);

        // reset mode
        ChangeMode(ModeStatus.Drawing);

        // reset sub menu

        // fps
        StartCoroutine(CalculateFps());
    }

    void Update()
    {
        // update tracking manager (get input..)
        trackingManager.DoUpdate(currentMode, currentSubMenuMode);

        // update ui manager (move object, cursor, delete...)
        uiManager.DoUpdate(currentMode);

        // update sub menu scroll controller
        subMenuScrollController.DoUpdate(currentSubMenuMode);

        // debug
        if (UnityMode.isEditor)
            CheckForInput();
    }

    #region Change Mode

    void ChangeMode(ModeStatus _targetMode)
    {
        switch(_targetMode)
        {
            case ModeStatus.Drawing:
                GoToDrawingMode();
                break;
            case ModeStatus.Transform:
                GoToTransformMode();
                break;
            case ModeStatus.Setting:
                GoToSettingMode();
                break;
        }
    }

    void GoToDrawingMode()
    {
        // set mode
        currentMode = ModeStatus.Drawing;        
        uiManager.PrintMode(currentMode);
        isTransformOn = false;

        // set ui manager
        uiManager.CloseSettingMenu(currentSubMenuMode, GetTargetModeStatus());
        
        // set drawing manager
        drawingManager.TurnOnDrawingPath(true);
        drawingManager.TurnOnInteractable(false);

        // log
        Debug.Log("Mode : " + currentMode.ToString());
    }

    void GoToSettingMode()
    {
        // set mode
        currentMode = ModeStatus.Setting;
        uiManager.PrintMode(currentMode);
        
        // set ui manager
        uiManager.OpenSettingMenu();

        // set drawing manager
        drawingManager.TurnOnDrawingPath(false);

        // log
        Debug.Log("Mode : " + currentMode.ToString());
    }

    void GoToTransformMode()
    {
        // set mode
        currentMode = ModeStatus.Transform;        
        uiManager.PrintMode(currentMode);
        isTransformOn = true;
        
        // set ui manager
        uiManager.CloseSettingMenu(currentSubMenuMode, GetTargetModeStatus());

        // set drawing manager
        drawingManager.TurnOnDrawingPath(true);
        drawingManager.TurnOnInteractable(true);

        // log
        Debug.Log("Mode : " + currentMode.ToString());
    }

    ModeStatus GetTargetModeStatus()
    {
        if (isTransformOn)
            return ModeStatus.Transform;
        else
            return ModeStatus.Drawing;
    }

    #endregion

    #region Public Trigger for ChangeMode

    // from Tracking manager
    public void DoubleClicked()
    {
        if (currentMode == ModeStatus.Setting) // close
            ChangeMode(GetTargetModeStatus());
        else // open
            ChangeMode(ModeStatus.Setting);//GoToSettingMode();
    }

    // from Button
    public void DrawButtonPressed()
    {
        ChangeMode(ModeStatus.Drawing);
    }

    public void EditButtonPressed()
    {
        ChangeMode(ModeStatus.Transform);
    }

    public void NewButtonPressed()
    {
        // clear path
        drawingManager.Clear();

        // set mode
        ChangeMode(ModeStatus.Drawing);
    }

    public void CloseButtonPressed()
    {
        ChangeMode(GetTargetModeStatus());
    }

    #endregion

    #region Change Sub Menu Mode

    void ChangeSubMenuMode(SubMenuModeStatus _targetSubMenuMode)
    {
        switch(_targetSubMenuMode)
        {
            case SubMenuModeStatus.Color:
                GoToColorMode();
                break;
            case SubMenuModeStatus.Size:
                GoToSizeMode();
                break;
            case SubMenuModeStatus.DoneSelection:
                GoToDoneSelectionMode();
                break;
            case SubMenuModeStatus.DoneMoving:
                GoToDoneMovingMode();
                break;
        }
    }

    void GoToColorMode()
    {
        currentSubMenuMode = SubMenuModeStatus.Color;

        // open color sub menu
        uiManager.OpenColorSubMenu();

        // setup scroll controller
        subMenuScrollController.SetupController(currentSubMenuMode);
    }

    void GoToSizeMode()
    {
        currentSubMenuMode = SubMenuModeStatus.Size;

        // open size sub menu
        uiManager.OpenSizeSubMenu();

        // setup scroll controller
        subMenuScrollController.SetupController(currentSubMenuMode);
    }

    void GoToDoneSelectionMode()
    {
        currentSubMenuMode = SubMenuModeStatus.DoneSelection;

        // setup scroll controller
        subMenuScrollController.DoneSelection();
    }

    void GoToDoneMovingMode()
    {
        currentSubMenuMode = SubMenuModeStatus.DoneMoving;
    }

    #endregion

    #region Public Trigger For ChangeSubMenuMode

    // from Tracking Manager
    public void HandLost()
    {
        if (currentMode == ModeStatus.Drawing) // on drawwing, done create
        {
            if (currentDrawingMode == DrawingModeStatus.AddPoint)
                ChangeDrawingMode(DrawingModeStatus.DoneCreate);
        }
        else if (currentMode == ModeStatus.Setting) // on setting, done select
        {
            ChangeSubMenuMode(SubMenuModeStatus.DoneSelection);
        }
    }

    // from Button
    public void ColorButtonPressed()
    {
        ChangeSubMenuMode(SubMenuModeStatus.Color);
    }

    public void SizeButtonPressed()
    {
        ChangeSubMenuMode(SubMenuModeStatus.Size);
    }

    // from Sub Menu Object (Event Trigger)
    public void ColorValueChanged()
    {
        ChangeSubMenuMode(SubMenuModeStatus.DoneSelection);
    }

    public void SizeValueChanged()
    {
        ChangeSubMenuMode(SubMenuModeStatus.DoneSelection);
    }
    
    // from Sub Menu Scroll Controller
    public void SubMenuDoneMoving(SubMenuModeStatus _subMenuMode, int _index)
    {
        if (_subMenuMode == SubMenuModeStatus.Color)
        {
            // update sub menu
            uiManager.UpdateAndCloseColorSubMenu(_index);
            
            // update line color
            drawingManager.ChangeLineColor(Preset.presetColor(_index));
        }
        else if (_subMenuMode == SubMenuModeStatus.Size)
        {
            // update sub menu
            uiManager.UpdateAndCloseSizeSubMenu(_index);

            // update line size
            drawingManager.ChangeLineSize(Preset.presetSize(_index));
        }

        ChangeSubMenuMode(SubMenuModeStatus.DoneMoving);
    }

    #endregion

    #region ChangeDrawingMode

    void ChangeDrawingMode(DrawingModeStatus _targetDrawingMode)
    {
        switch(_targetDrawingMode)
        {
            case DrawingModeStatus.Create:
                GoToCreateMode();
                break;
            case DrawingModeStatus.AddPoint:
                GoToAddPointMode();
                break;
            case DrawingModeStatus.DoneCreate:
                GoToDoneCreateMode();
                break;
        }
    }

    void GoToCreateMode()
    {
        // set mode
        currentDrawingMode = DrawingModeStatus.Create;

        // setup drawing manager
        Vector3 handCursorPosition = uiManager.GetHandCursorPosition();
        drawingManager.CreateNewLine(handCursorPosition);
    }

    void GoToAddPointMode()
    {
        // set mode
        currentDrawingMode = DrawingModeStatus.AddPoint;

        // setup drawing manager
        Vector3 handCursorPosition = uiManager.GetHandCursorPosition();
        drawingManager.AddPointOnLine(handCursorPosition);
    }

    void GoToDoneCreateMode()
    {
        // set mode
        currentDrawingMode = DrawingModeStatus.DoneCreate;

        // setup drawing manager
        drawingManager.NewObjectCreated();

        // log
        Debug.Log("Created New");
    }

    #endregion

    #region Public Trigger For ChangeDrawingMode

    // from Tracking Manager
    public void CreateNewLine()
    {
        ChangeDrawingMode(DrawingModeStatus.Create);
    }

    public void AddPointOnLine()
    {
        ChangeDrawingMode(DrawingModeStatus.AddPoint);
    }

    public void NewObjectCreated()
    {
        ChangeDrawingMode(DrawingModeStatus.DoneCreate);
    }

    #endregion

    #region Get Values From Tracking Manager

    public Vector3 GetHeadPosition()
    {
        return trackingManager.GetHeadPosition();
    }

    public Vector3 GetHandPosition()
    {
        return trackingManager.GetHandPosition();
    }

    public bool GetIsHandTracked()
    {
        return trackingManager.GetIsHandTracked();
    }

    public bool GetIsHandSelected()
    {
        return trackingManager.GetIsHandSelected();
    }
    
    public bool GetIsTwoHandTracked()
    {
        return trackingManager.GetIsTwoHandTracked();
    }

    #endregion

    #region Button Focus

    public void ButtonFocus(RectTransform _target)
    {
        uiManager.ButtonFocus(_target);
    }

    public void ButtonNormal(RectTransform _target)
    {
        uiManager.ButtonNormal(_target);
    }

    #endregion

    #region Delete
        
    public void DeleteTriggerOn(GameObject _oDeleteSelected)
    {
        uiManager.DeleteTriggerOn(_oDeleteSelected);
    }

    public void DeleteTriggerOff()
    {
        uiManager.DeleteTriggerOff();
    }

    #endregion

    #region FPS

    IEnumerator CalculateFps()
    {
        while(true)
        {
            float fps = (1 / Time.deltaTime) * Time.timeScale;
            uiManager.PrintFps(fps);
            yield return fpsTimer;
        }
    }

    #endregion
    
    #region Debug Input

    void CheckForInput()
    {
        // setting menu
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentMode == ModeStatus.Setting)
                ChangeMode(GetTargetModeStatus());
            else
                ChangeMode(ModeStatus.Setting);
        }

        // sub menu
        else if (Input.GetKeyDown(KeyCode.X))
            ChangeSubMenuMode(SubMenuModeStatus.Color);
        else if (Input.GetKeyDown(KeyCode.C))
            ChangeSubMenuMode(SubMenuModeStatus.DoneSelection);
        else if (Input.GetKeyDown(KeyCode.V))
            ChangeSubMenuMode(SubMenuModeStatus.Size);
        else if (Input.GetKeyDown(KeyCode.B))
            ChangeSubMenuMode(SubMenuModeStatus.DoneSelection);

        // sub menu size update
        else if (Input.GetKeyDown(KeyCode.U))
            subMenuScrollController.InitSizeIndex(1);
        else if (Input.GetKeyDown(KeyCode.I))
            subMenuScrollController.InitSizeIndex(2);
        else if (Input.GetKeyDown(KeyCode.O))
            subMenuScrollController.InitSizeIndex(3);
        else if (Input.GetKeyDown(KeyCode.P))
            subMenuScrollController.InitSizeIndex(4);

        // sub menu color update
        else if (Input.GetKeyDown(KeyCode.G))
            subMenuScrollController.InitColorIndex(1);
        else if (Input.GetKeyDown(KeyCode.H))
            subMenuScrollController.InitColorIndex(2);
        else if (Input.GetKeyDown(KeyCode.J))
            subMenuScrollController.InitColorIndex(3);
        else if (Input.GetKeyDown(KeyCode.K))
            subMenuScrollController.InitColorIndex(4);
        else if (Input.GetKeyDown(KeyCode.L))
            subMenuScrollController.InitColorIndex(5);
    }

    #endregion
}
