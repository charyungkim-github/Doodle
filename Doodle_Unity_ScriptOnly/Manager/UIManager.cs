using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Menu")]    
    public Transform tSettingMenu;
    public GameObject oSubMenu;

    [Header("PlayerView")]
    public Transform tPlayerView;

    [Header("Buttons")]
    public GameObject oEditButton;
    public GameObject oDrawButton;
    public GameObject oCloseButton;
    public GameObject oDeleteButton;

    [Header("Icon")]
    public Image imgColorIcon;
    public RectTransform tSizeIcon;

    [Header("Block Panel")]
    public RectTransform tBlockPanel; // default 500 selected 1060

    [Header("Cursor")]
    public Transform tHandCursor; // hand cursor
    public Sprite spriteCursorNormal;
    public Sprite spriteCursorSelect;

    // manager
    Manager manager;

    // animator
    Animator animSettingMenu;
    Animator animColorSubMenu;
    Animator animSizeSubMenu;

    // drawing path
    GameObject oDrawingPath;

    // delete
    GameObject oDeleteSelected = null;
    bool preIsDeleteSelected = false;
    bool isDeleteTriggerEnter = false;

    // move object
    Transform tCamera;
    Vector3 lookAtPosition = Vector3.zero;
    Vector3 lookAtDirection = Vector3.zero;
    bool objectCanMove = false;

    // cursor
    GameObject oHeadCursor;
    bool preIsHandSelected = false;

    // text
    Text txtMode;
    Text txtFps;

    #region UIManager Update/Setup

    public void DoStart(Manager _manager, GameObject _oDrawingPath, GameObject _oHeadCursor)
    {
        // set component from manager
        manager = _manager;
        oDrawingPath = _oDrawingPath;
        oHeadCursor = _oHeadCursor;

        // set camera component
        tCamera = Camera.main.transform;

        // set animator component
        animSettingMenu = tSettingMenu.GetComponent<Animator>();
        animColorSubMenu = oSubMenu.GetComponentsInChildren<Animator>()[0];
        animSizeSubMenu = oSubMenu.GetComponentsInChildren<Animator>()[1];

        // set text component
        txtMode = tPlayerView.GetComponentsInChildren<Text>()[0];
        txtFps = tPlayerView.GetComponentsInChildren<Text>()[1];
    }

    public void DoUpdate(ModeStatus _currentMode)
    {
        MoveObject(_currentMode);
        ChangeCursor(_currentMode);
        CheckForDelete();
    }

    #endregion

    #region Setting Menu Open/Close

    public void OpenSettingMenu()
    {
        // change position
        SetSettingPanelPosition();

        // open
        animSettingMenu.SetTrigger(AnimatorTrigger.onTrigger);
        oDrawingPath.SetActive(false);
        tPlayerView.gameObject.SetActive(false);
    }

    public void CloseSettingMenu(SubMenuModeStatus _currentSubMenuMode, ModeStatus _targetMode)
    {
        // change ui
        SetSettingPanelUI(_targetMode);

        // close
        animSettingMenu.SetTrigger(AnimatorTrigger.offTrigger);
        oDrawingPath.SetActive(true);
        tPlayerView.gameObject.SetActive(true);

        // if color, size sub menu on, close
        if (_currentSubMenuMode == SubMenuModeStatus.Color)
            CloseColorSubMenu();
        else if (_currentSubMenuMode == SubMenuModeStatus.Size)
            CloseSizeSubMenu();

        // check delete button
        oDeleteButton.SetActive(false);
    }

    #endregion

    #region Color Sub Menu Open/Close

    public void OpenColorSubMenu()
    {
        // enable color sub menu
        animColorSubMenu.SetTrigger(AnimatorTrigger.onTrigger);
        oCloseButton.SetActive(false);
        SetBlockPanel(true, "sub");
    }

    public void UpdateAndCloseColorSubMenu(int _selectedIndex)
    {
        // update color
        Color c = Preset.presetColor(_selectedIndex);

        // change icon
        imgColorIcon.color = c;

        // change cursor
        tHandCursor.GetComponent<SpriteRenderer>().color = c;

        // close
        CloseColorSubMenu();
    }

    void CloseColorSubMenu()
    {
        // close color sub menu
        animColorSubMenu.SetTrigger(AnimatorTrigger.offTrigger);
        oCloseButton.SetActive(true);
        SetBlockPanel(false, "sub");
    }

    #endregion

    #region Size Sub Menu Open/Close
   
    public void OpenSizeSubMenu()
    {
        // enable size sub menu
        animSizeSubMenu.SetTrigger(AnimatorTrigger.onTrigger);
        oCloseButton.SetActive(false);
        SetBlockPanel(true, "sub");
    }

    public void UpdateAndCloseSizeSubMenu(int _selectedIndex)
    {
        // update size
        float s = Preset.presetSize(_selectedIndex);

        // change icon
        tSizeIcon.anchoredPosition = new Vector2(tSizeIcon.anchoredPosition.x, CustomPositions.sizeIconPosition(_selectedIndex));

        // change curor
        tHandCursor.localScale = Vector3.one * Preset.presetCursorSize(s);

        // close
        CloseSizeSubMenu();
    }    

    void CloseSizeSubMenu()
    {
        // close size sub menu
        animSizeSubMenu.SetTrigger(AnimatorTrigger.offTrigger);
        oCloseButton.SetActive(true);
        SetBlockPanel(false, "sub");
    }

    #endregion

    #region Move Object : PlayerView, Cursor, SettingPanel

    void MoveObject(ModeStatus currentMode)
    {
        Vector3 headPosition = manager.GetHeadPosition();
        Vector3 handPosition = manager.GetHandPosition();
        lookAtPosition = tCamera.rotation * Vector3.forward;
        lookAtDirection = tCamera.rotation * Vector3.up;

        // move player view
        MovePlayerView(headPosition);

        if (currentMode == ModeStatus.Drawing) // move hand cursor
        {
            // move cursor
            MoveHandCursor(handPosition);
        }
        else if (currentMode == ModeStatus.Setting) // move setting menu
        {
            // move setting panel
            MoveSettingPanelFollow(headPosition);
        }
    }

    void MovePlayerView(Vector3 _headPosition)
    {
        tPlayerView.LookAt(tPlayerView.position + lookAtPosition, lookAtDirection);
        tPlayerView.position = _headPosition;
    }

    void MoveHandCursor(Vector3 _handPosition)
    {
        float xOffset = -0.1f;
        float yOffset = 0.1f;
        float zOffset = 0.7f;
        Vector3 convertedHandPosition = _handPosition + tHandCursor.TransformDirection(xOffset, yOffset, zOffset);
        tHandCursor.LookAt(tHandCursor.position + lookAtPosition, lookAtDirection);
        tHandCursor.position = convertedHandPosition;
    }

    void MoveSettingPanelFixed(Vector3 _headPosition)
    {
        tSettingMenu.LookAt(tSettingMenu.position + lookAtPosition, lookAtDirection);
        tSettingMenu.position = _headPosition;
    }

    void MoveSettingPanelFollow(Vector3 _headPosition)
    {
        if (canMove(_headPosition))
            objectCanMove = true;

        if (objectCanMove)
        {
            float moveSpeed = 3f;
            tSettingMenu.LookAt(tSettingMenu.position + lookAtPosition, lookAtDirection);
            tSettingMenu.position = Vector3.Lerp(tSettingMenu.position, _headPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(_headPosition, tSettingMenu.position) < 0.01f)
                objectCanMove = false;
        }
    }

    bool canMove(Vector3 _currentHeadPosition)
    {
        float deltaThresold = 0.2f;
        float distance = Vector3.Distance(_currentHeadPosition, tSettingMenu.position);

        if (distance > deltaThresold)
            return true;
        else
            return false;
    }

    #endregion

    #region Send Values to Manager

    public Vector3 GetHandCursorPosition()
    {
        return tHandCursor.position;
    }

    #endregion

    #region Cursor Component

    void ChangeCursor(ModeStatus _currentMode)
    {
        if (_currentMode == ModeStatus.Drawing)
        {
            // turn off head cursor
            if (oHeadCursor.activeSelf)
                oHeadCursor.SetActive(false);

            // turn on hand cursor
            if (manager.GetIsHandTracked())
                tHandCursor.gameObject.SetActive(true);
            else
                tHandCursor.gameObject.SetActive(false);

            // change hand cursor sprite
            ChangeCursorSprite();
        }
        else
        {
            // turn on head cursor
            if (!oHeadCursor.activeSelf)
                oHeadCursor.SetActive(true);


            // turn off hand cursor
            if (tHandCursor.gameObject.activeSelf)
                tHandCursor.gameObject.SetActive(false);
        }
    }

    void ChangeCursorSprite()
    {
        bool isHandSelected = manager.GetIsHandSelected();

        if (isHandSelected != preIsHandSelected)
        {
            Sprite targetSprite = isHandSelected ? spriteCursorSelect : spriteCursorNormal;
            tHandCursor.GetComponent<SpriteRenderer>().sprite = targetSprite;
        }

        preIsHandSelected = isHandSelected;
    }

    #endregion

    #region Set Panel

    void SetSettingPanelPosition()
    {
        Vector3 headPosition = manager.GetHeadPosition();
        MoveSettingPanelFixed(headPosition);
    }

    void SetSettingPanelUI(ModeStatus _modeStatus)
    {
        if (_modeStatus == ModeStatus.Transform)
        {
            oEditButton.SetActive(false);
            oDrawButton.SetActive(true);
            SetBlockPanel(true, "main");
        }
        else
        {
            oEditButton.SetActive(true);
            oDrawButton.SetActive(false);
            SetBlockPanel(false, "main");
        }
    }

    void SetBlockPanel(bool _on, string _tag)
    {
        tBlockPanel.sizeDelta = (_tag == "main") ? new Vector2(500, 350) : new Vector2(1060, 350);
        tBlockPanel.gameObject.SetActive(_on);
    }

    #endregion

    #region Delete

    void CheckForDelete()
    {
        bool isDeleteSelected = DeleteObjectParams.isSelectedOnDeleteObject;
        bool isTwoHandTracked = manager.GetIsTwoHandTracked();

        if (isDeleteSelected != preIsDeleteSelected)
        {
            if (isDeleteSelected)
            {
                // turn on button
                oDeleteButton.SetActive(true);
            }
            else // if still on trigger => erase
            {
                if (isDeleteTriggerEnter && oDeleteSelected != null && !isTwoHandTracked)
                {
                    Destroy(oDeleteSelected);
                    Debug.Log("Destroy");
                }

                // turn off button
                oDeleteButton.SetActive(false);
            }
        }

        preIsDeleteSelected = isDeleteSelected;
    }

    public void DeleteTriggerOn(GameObject _oDeleteSelected)
    {
        if (_oDeleteSelected == DeleteObjectParams.selectedDeleteObject)
        {
            isDeleteTriggerEnter = true;
            oDeleteSelected = _oDeleteSelected;
        }
    }

    public void DeleteTriggerOff()
    {
        isDeleteTriggerEnter = false;
    }

    #endregion

    #region ButtonScale

    public void ButtonFocus(RectTransform _target)
    {
        Vector2 targetScale = new Vector2(1.1f, 1.1f);
        StartCoroutine(CoroutineLerpScale(_target, targetScale, 0.06f));
    }

    public void ButtonNormal(RectTransform _target)
    {
        Vector2 targetScale = Vector2.one;
        StartCoroutine(CoroutineLerpScale(_target, targetScale, 0.06f));
    }

    IEnumerator CoroutineLerpScale(RectTransform _target, Vector2 _destScale, float _time)
    {
        float t = 0;
        Vector2 startScale = _target.localScale;
        while (t < 1)
        {
            t += (Time.deltaTime * (1 / _time));
            _target.localScale = Vector2.Lerp(startScale, _destScale, t);

            yield return null;
        }

        _target.localScale = _destScale;
    }

    #endregion
          
    #region MODE/FPS

    public void PrintMode(ModeStatus _mode)
    {
        txtMode.text = _mode.ToString();
    }

    public void PrintFps(float _fps)
    {
        txtFps.text = "FPS " + _fps.ToString("N2");
    }

    #endregion
}
