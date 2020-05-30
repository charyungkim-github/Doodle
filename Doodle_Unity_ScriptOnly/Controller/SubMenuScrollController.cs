using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubMenuScrollController : MonoBehaviour
{
    [Header("Content Transform")]
    public RectTransform colorContentTransform;
    public RectTransform sizeContentTransform;

    [Header("Infnite Scroll Item")]
    public GameObject colorItemPrefab;

    // manager
    Manager manager;

    // current content transform
    RectTransform contentTransform;

    // mode
    bool isColorMode = false;

    // snap
    bool doSnap = false;
    float snapSpeed = 20f;
    float distance = 0;
    int clampMin, clampMax = 0;

    // selected index
    float targetY = 0;
    int selectedIndex = 0;

    // infinite scroll
    List<GameObject> createdItems = new List<GameObject>();    
    float prePosY = 0;
    int topIndex, topCount, bottomIndex, bottomCount = 0;
    bool blockInfiniteScroll = false;
    float blockPosY = 0;

    // size value
    float sizeContentHeight = CustomPositions.sizeContentHeight;
    float sizeContentArrangeHeight = CustomPositions.sizeContentArrangeHeight;

    #region SubMenuScrollController Setup/Update

    public void DoStart(Manager _manager, int _colorTargetIndex, int _sizeTargetIndex)
    {
        // setup from manager
        manager = _manager;

        // setup infinite scroll
        SetupInfiniteScroll();

        // setup target index
        InitColorIndex(_colorTargetIndex);
        InitSizeIndex(_sizeTargetIndex);
    }

    public void DoUpdate(SubMenuModeStatus _currentSubMenuMode)
    {
        if (_currentSubMenuMode != SubMenuModeStatus.DoneMoving)
        {
            // snap to item
            SnapToTarget();

            // infinite scroll
            if (isColorMode)
            {
                // color infinite scroll
                UpdateInfiniteScroll();

                // block color scroll if change too fast
                if (blockInfiniteScroll)
                {
                    contentTransform.anchoredPosition = new Vector2(0, blockPosY);
                    blockInfiniteScroll = false;
                }
            }
        }
    }

    #endregion

    #region Init Index

    public void InitColorIndex(int _targetIndex)
    {
        // set preset color
        int colorCount = Preset.presetColorCount + 1;
        Color[] convertedColors = new Color[colorCount];

        convertedColors[0] = Color.white; // empty
        for (int i = 1; i < colorCount; i++)
        {
            convertedColors[i] = Preset.presetColor(_targetIndex);

            _targetIndex++;
            if (_targetIndex >= colorCount)
                _targetIndex = 1;
        }

        // save converted color
        Preset.presetColors = convertedColors;

        // set object color
        for (int i = 0; i < colorContentTransform.childCount; i++)
        {
            GameObject colorObject = colorContentTransform.GetChild(i).gameObject;
            int colorIndex = GetIndexFromGameObject(colorObject);

            colorObject.GetComponent<Image>().color = Preset.presetColor(colorIndex);
        }

        // update
        manager.SubMenuDoneMoving(SubMenuModeStatus.Color, 1);
    }

    public void InitSizeIndex(int _targetIndex)
    {
        float y = (_targetIndex - 1) * sizeContentHeight;
        if (_targetIndex == Preset.presetSizeCount)
            y += sizeContentArrangeHeight;

        sizeContentTransform.anchoredPosition = new Vector2(sizeContentTransform.anchoredPosition.x, y);

        // update
        manager.SubMenuDoneMoving(SubMenuModeStatus.Size, _targetIndex);
    }

    #endregion

    #region Start/Move/Stop SubMenu Controller

    public void SetupController(SubMenuModeStatus _subMenuStatus) // Setup Controller
    {
        // set mode
        isColorMode = (_subMenuStatus == SubMenuModeStatus.Color) ? true : false;

        // set transform
        contentTransform = isColorMode ? colorContentTransform : sizeContentTransform;

        // set distance
        distance = isColorMode ? colorItemPrefab.GetComponent<RectTransform>().sizeDelta.y : sizeContentHeight;

        // set clamp
        clampMin = isColorMode ? (int)(distance * -3) : (int) sizeContentHeight * -1;
        clampMax = isColorMode ? (int)(distance * 3) : (int) ((sizeContentHeight * (Preset.presetSizeCount - 1)) + sizeContentArrangeHeight);
    }

    public void DoneSelection() // Done Selection
    {
        float currentY = contentTransform.anchoredPosition.y;
        int anchorIndex = GetCloseIndex(currentY);
        float offsetY = 0;

        if (!isColorMode)
        {
            // one way scroll on size
            anchorIndex = Mathf.Clamp(anchorIndex, 0, Preset.presetSizeCount);

            // arrange list index position
            if (anchorIndex+1 == Preset.presetSizeCount)
                offsetY = CustomPositions.sizeContentArrangeHeight;
        }

        targetY = anchorIndex * distance + offsetY ;
        selectedIndex = GetSelectedIndex(anchorIndex);
        doSnap = true;
    }

    void SnapToTarget() // Move
    {
        float positionY = contentTransform.anchoredPosition.y;

        if (doSnap)
        {
            if (Mathf.Abs(contentTransform.anchoredPosition.y - targetY) < 0.1f)
            {
                // set target posiiton
                positionY = targetY;

                // end snap
                doSnap = false;

                // update and close
                DoneMoving();
            }
            else
            {
                // set target position
                positionY = Mathf.Lerp(contentTransform.anchoredPosition.y, targetY, snapSpeed * Time.deltaTime);
            }
        }

        // clamp on size mode
        if (!isColorMode)
            positionY = Mathf.Clamp(positionY, clampMin, clampMax);

        // update position
        contentTransform.anchoredPosition = new Vector2(0, positionY);
    }

    void DoneMoving() // Done Moving
    {
        if (isColorMode)
            manager.SubMenuDoneMoving(SubMenuModeStatus.Color, selectedIndex);
        else
            manager.SubMenuDoneMoving(SubMenuModeStatus.Size, selectedIndex);
    }

    #endregion

    #region Get Selected

    int GetCloseIndex(float _currentY)
    {
        int delta = _currentY < 0 ? -1 : 1;
        int prevAnchorIndex = (int)(_currentY / distance);
        int aftAnchorIndex = (int)(_currentY / distance + delta);

        if (Mathf.Abs(_currentY - prevAnchorIndex * distance) < Mathf.Abs(_currentY - aftAnchorIndex * distance))
            return prevAnchorIndex;
        else
            return aftAnchorIndex;
    }

    int GetSelectedIndex(int _anchorIndex)
    {
        int presetCount = isColorMode ? Preset.presetColorCount : Preset.presetSizeCount + 1;
        _anchorIndex += 1;

        if (_anchorIndex > 0) // move up
            return UpdateIndex(_anchorIndex % presetCount);
           
        else // move down
            return presetCount + (_anchorIndex % presetCount);
    }

    #endregion

    #region Infinite Scroll

    void SetupInfiniteScroll()
    {
        int childCount = colorContentTransform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject childObject = colorContentTransform.GetChild(i).gameObject;

            // make item
            createdItems.Add(childObject);

            // setup index
            if (i == 0)
                topIndex = GetIndexFromGameObject(childObject);
            else if(i == childCount - 1)
                bottomIndex = GetIndexFromGameObject(childObject);
        }

        // setup count
        topCount = bottomCount = (childCount - 1) / 2;
    }

    int GetIndexFromGameObject(GameObject _gameObject)
    {
        string replaceString = _gameObject.name.Contains("Color") ? "Color_" : "Size_";
        string indexString = _gameObject.name.Replace(replaceString, "");
        return Convert.ToInt32(indexString);
    }

    void UpdateInfiniteScroll()
    {
        float curPosY = contentTransform.anchoredPosition.y;
        float deltaPosY = curPosY - prePosY;
        float absDeltaPosY = Mathf.Abs(deltaPosY);
        float diffMax = 1.3f;

        if (absDeltaPosY > distance)
        {   
            // block scroll on delta change fast
            if(absDeltaPosY > (distance * diffMax))
            {
                blockInfiniteScroll = true;

                if(deltaPosY < 0)
                    curPosY = prePosY - distance;
                else
                    curPosY = prePosY + distance;

                blockPosY = curPosY;
            }

            // make item
            if (deltaPosY < 0)
                MakeItem(true);
            else
                MakeItem(false);

            prePosY = curPosY;
        }
    }

    void MakeItem(bool _isTop)
    {
        if (_isTop)
        {
            // update index
            topCount++;
            topIndex = UpdateIndex(--topIndex);
        }
        else
        {
            // updateIndex;
            bottomCount++;
            bottomIndex = UpdateIndex(++bottomIndex);
        }

        // created top item
        int currentCreateIndex = _isTop ? 0 : createdItems.Count;
        GameObject item = Instantiate(colorItemPrefab, contentTransform);
        createdItems.Insert(currentCreateIndex, item);

        // set components
        int currentCount = _isTop ? topCount : -bottomCount;
        int currentIndex = _isTop ? topIndex : bottomIndex;
        item.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, currentCount * distance);
        item.name = "Color_" + currentIndex.ToString();
        item.GetComponent<Image>().color = Preset.presetColor(currentIndex);

        // delete bottom item
        int currentDeleteIndex = _isTop ? createdItems.Count - 1 : 0;
        Destroy(createdItems[currentDeleteIndex]);
        createdItems.RemoveAt(currentDeleteIndex);

        if (_isTop)
        {
            bottomCount--;
            bottomIndex = UpdateIndex(--bottomIndex);
        }
        else
        {
            topCount--;
            topIndex = UpdateIndex(++topIndex);
        }
    }

    int UpdateIndex(int _index)
    {
        if (_index < 1)
            return Preset.presetColorCount;
        else if (_index > Preset.presetColorCount)
            return 1;
        else
            return _index;
    }

    #endregion
}
                               
                               