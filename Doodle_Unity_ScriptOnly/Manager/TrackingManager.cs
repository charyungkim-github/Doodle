using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;

public class TrackingManager : MonoBehaviour
{
    Manager manager;

    // tracking
    Tuple<InputSourceType, Handedness> headTuple = new Tuple<InputSourceType, Handedness>(InputSourceType.Head, Handedness.Any);
    Vector3 handPosition = Vector3.zero;
    Vector3 vectorNull = Vector3.one * -100f;

    // get gesture
    GestureStatus prevGestureStatus = GestureStatus.None;
    bool canDoubleTap = true;
    bool rightSelected = false;
    bool leftSelected = false;
    bool rightTracked = false;
    bool leftTracked = false;

    // double tap
    PointerHandler pointHandler;
    WaitForSeconds doubleClickTreashHold = new WaitForSeconds(0.5f);
    Coroutine timerCoroutine;
    int clickCount = 0;
    bool enableDragging = false;

    // hand lost
    bool isHandSelected = false;
    bool isHandTracked = false;

    #region TrackingManager Setup/Update

    public void DoStart(Manager _manager)
    {
        // setup from manager
        manager = _manager;

        // double click handler
        pointHandler = GetComponent<PointerHandler>();
        CoreServices.InputSystem.RegisterHandler<IMixedRealityPointerHandler>(pointHandler);
    }

    public void DoUpdate(ModeStatus _currentMode, SubMenuModeStatus _currentSubMenuMode)
    {
        GestureStatus currentGesture = GetGesture();

        if (_currentMode == ModeStatus.Drawing)
        {
            switch (currentGesture)
            {
                case GestureStatus.StartDrag:
                    OnStartDrag();
                    break;

                case GestureStatus.Dragging:
                    OnDragging();
                    break;

                case GestureStatus.DoneDragging:                    
                    DoneDragging();
                    break;
            }
        }

        // check for hand lost
        if (GetLost())
            manager.HandLost();
    }

    #endregion

    #region Drag Action

    void OnStartDrag()
    {
        manager.CreateNewLine();
    }

    void OnDragging()
    {
        manager.AddPointOnLine();
    }

    void DoneDragging()
    {
        manager.NewObjectCreated();
    }

    #endregion
    
    #region Tracking : Gesture

    GestureStatus GetGesture() // get hand gesture
    {
        GestureStatus _gestureStatus = GestureStatus.None;
        isHandTracked = false;
        rightTracked = false; 
        leftTracked = false;
        
        foreach (var controller in CoreServices.InputSystem.DetectedControllers)
        {
            if (controller.ControllerHandedness.ToString() == "Right")
                rightTracked = true;
            else if (controller.ControllerHandedness.ToString() == "Left")
                leftTracked = true;

            if (controller.ControllerHandedness.ToString() == "Right" || controller.ControllerHandedness.ToString() == "Left")
                isHandTracked = true;

            foreach (MixedRealityInteractionMapping inputMapping in controller.Interactions)
            {
                // hand position
                if (isHandTracked)
                    handPosition = inputMapping.PositionData;

                if (inputMapping.Description == "Select")
                {
                    if (inputMapping.BoolData)
                    {
                        if (controller.ControllerHandedness.ToString() == "Left")
                            leftSelected = true;
                        else if (controller.ControllerHandedness.ToString() == "Right")
                            rightSelected = true;

                        if (enableDragging) 
                        {
                            // drawing
                            if (prevGestureStatus != _gestureStatus)
                                _gestureStatus = GestureStatus.Dragging;
                            else
                            {
                                _gestureStatus = GestureStatus.StartDrag;
                            }
                        }

                        isHandSelected = true;
                    }
                    else
                    {
                        if (prevGestureStatus == GestureStatus.Dragging)
                            _gestureStatus = GestureStatus.DoneDragging;

                        isHandSelected = false;
                    }
                }
            }
        }
        
        // check for two hands
        if (leftSelected && rightSelected)
            canDoubleTap = false;
        else
            canDoubleTap = true;

        leftSelected = false; rightSelected = false;

        prevGestureStatus = _gestureStatus;

        return _gestureStatus;
    }

    #endregion

    #region Tracking : Double Click
    
    public void OnTapDown()
    {
        enableDragging = false;
        timerCoroutine = StartCoroutine(CoroutineTimer());
    }

    public void OnTapped()
    {
        if(canDoubleTap)
        {
            clickCount++;
            enableDragging = false;

            if (clickCount == 2)
            {
                StopCoroutine(timerCoroutine);
                clickCount = 0;

                // open setting menu
                manager.DoubleClicked();
            }
            else
            {
                timerCoroutine = StartCoroutine(CoroutineTimer());
            }
        }        
    }
        
    private IEnumerator CoroutineTimer()
    {
        yield return doubleClickTreashHold;

        clickCount = 0;
        enableDragging = true;
    }
    #endregion

    #region Tracking : Get Lost
    
    bool GetLost()
    {
        if (isHandSelected && !isHandTracked)
        {
            isHandSelected = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region Send Values to Manager

    public Vector3 GetHeadPosition()
    {
        if (InputRayUtils.TryGetRay(headTuple.Item1, headTuple.Item2, out Ray headRay))
            return headRay.origin + headRay.direction;

        else
            return vectorNull;
    }
    
    public Vector3 GetHandPosition()
    {
        return handPosition;
    }

    public bool GetIsHandTracked()
    {
        return isHandTracked;
    }

    public bool GetIsHandSelected()
    {
        return isHandSelected;
    }

    public bool GetIsTwoHandTracked()
    {
        return rightTracked && leftTracked;
    }

    #endregion
}