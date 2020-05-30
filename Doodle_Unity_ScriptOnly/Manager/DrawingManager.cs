using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using System.Linq;

public class DrawingManager : MonoBehaviour
{
    public GameObject linePrefab;

    // drawing path
    Transform tDrawingPath;

    // line
    LineRenderer currentLine;
    Color currentColor;
    float currentSize;

    // position
    Vector3 prevConvertedHandPos = Vector3.zero;
    Vector3 drawingPointerPosition;

    #region DrawingManager Setup/Update

    public void DoStart(GameObject _oDrawingPath)
    {
        tDrawingPath = _oDrawingPath.transform;
    }

    #endregion

    #region Basic Function

    // start create new line, create new line renderer object
    public void CreateNewLine(Vector3 _convertedHandPos)
    {
        // create new line
        currentLine = Instantiate(linePrefab, tDrawingPath).GetComponent<LineRenderer>();

        // set color
        currentLine.startColor = currentColor;
        currentLine.endColor = currentColor;

        // set scale
        currentLine.startWidth = currentSize;
        currentLine.endWidth = currentSize;

        // setup drawing pointer
        drawingPointerPosition = _convertedHandPos;
    }
    
    public void AddPointOnLine(Vector3 _convertedHandPos)
    {
        float thresold = 0.0005f;
        float moveSpeed = 5f;
        
        // block on first
        if (currentLine.positionCount != 0)
            drawingPointerPosition = Vector3.Lerp(drawingPointerPosition, prevConvertedHandPos, Time.deltaTime * moveSpeed);

        // add point
        float distance = Vector3.Distance(_convertedHandPos, prevConvertedHandPos);
        if (distance > thresold)
        {   
            currentLine.positionCount++;
            currentLine.SetPosition(currentLine.positionCount - 1, drawingPointerPosition);
        }

        prevConvertedHandPos = _convertedHandPos;
    }

    // done drawing, add interactable component
    public void NewObjectCreated()
    {
        // enable interaction
        int index = tDrawingPath.childCount - 1;
        AddInteractableCompoent(index);
    }

    // clear button pressed, destroy all
    public void Clear()
    {
        for (int i = 0; i < tDrawingPath.childCount; i++)
        {
            Destroy(tDrawingPath.GetChild(i).gameObject);
        }
    }

    #endregion

    #region Update Line Color/Size

    public void ChangeLineColor(Color _color)
    {
        currentColor = _color;
    }

    public void ChangeLineSize(float _size)
    {
        currentSize = _size;
    }

    #endregion

    #region Line Interactable

    // turn on/off interactable
    public void TurnOnInteractable(bool _on)
    {
        for (int i = 0; i < tDrawingPath.childCount; i++)
        {
            GameObject child = tDrawingPath.GetChild(i).gameObject;
            child.GetComponent<InteractionController>().TurnOn(_on);
        }
    }

    // add interactable component on drawn object
    void AddInteractableCompoent(int _index)
    {
        GameObject child = tDrawingPath.GetChild(_index).gameObject;
        LineRenderer lineRendere = child.GetComponent<LineRenderer>();

        if(lineRendere.positionCount < 2)
        {
            Destroy(child);
        }
        else
        {
            child.AddComponent<BoxCollider>();            
            child.GetComponent<BoundingBox>().BoundsOverride = child.GetComponent<BoxCollider>();
            child.GetComponent<BoxCollider>().isTrigger = true;
            AdjustCollider(lineRendere);
        }
    }
    
    // make collider fit to drawn object
    void AdjustCollider(LineRenderer line)
    {
        // collider
        Vector3[] info = GetLength(line);
        BoxCollider lineCollider = line.gameObject.GetComponent<BoxCollider>();
        lineCollider.size = info[0];
        lineCollider.center = info[1];

        // close button
        GameObject button = line.gameObject.transform.GetChild(0).gameObject;
        
        // position
        float x = lineCollider.center.x + (lineCollider.size.x / 2);
        float y = lineCollider.center.y + (lineCollider.size.y / 2);
        float z = lineCollider.center.z - (lineCollider.size.z / 2);

        button.transform.localPosition = new Vector3(x, y, z);
    }

    Vector3[] GetLength(LineRenderer _line)
    {
        List<float> xPoints = new List<float>();
        List<float> yPoints = new List<float>();
        List<float> zPoints = new List<float>();

        Vector3[] allPoints = new Vector3[_line.positionCount];
        _line.GetPositions(allPoints);

        for (int i = 0; i < allPoints.Length; i++)
        {
            xPoints.Add(allPoints[i].x);
            yPoints.Add(allPoints[i].y);
            zPoints.Add(allPoints[i].z);
        }
        
        float offset = 0.05f;
        float xLen = xPoints.Max() - xPoints.Min() + _line.startWidth + offset;
        float yLen = yPoints.Max() - yPoints.Min() + _line.startWidth + offset;
        float zLen = zPoints.Max() - zPoints.Min() + _line.startWidth + offset;
        Vector3 len = new Vector3(xLen, yLen, zLen);

        float xArr = (xPoints.Max() + xPoints.Min()) / 2;
        float yArr = (yPoints.Max() + yPoints.Min()) / 2;
        float zArr = (zPoints.Max() + zPoints.Min()) / 2;
        Vector3 arr = new Vector3(xArr, yArr, zArr);

        return new Vector3[] { len, arr };
    }

    #endregion

    #region Drawing Path

    public void TurnOnDrawingPath(bool _on)
    {
        tDrawingPath.gameObject.SetActive(_on);
    }

    #endregion
}
