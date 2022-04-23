using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawTouch : MonoBehaviour
{
    [SerializeField] GameObject lineRendererPrefab;
    [SerializeField] GameObject pointPrefab;
    [SerializeField] float nextPointThreshold = 0.1f;
    [SerializeField] int pointsInCurve = 50;

    BoxCollider2D boundryCollider;

    LineRenderer lineRenderer;
    LineRenderer curveRenderer;
    List<Vector2> controlPoints = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = Instantiate(lineRendererPrefab, transform.position, transform.rotation).GetComponent<LineRenderer>();
        curveRenderer = Instantiate(lineRendererPrefab, transform.position, transform.rotation).GetComponent<LineRenderer>();
        boundryCollider = GetComponent<BoxCollider2D>();
    }

    void OnMouseDown()
    {
        SetStartingPoint();
        RenderBezierCurve();
    }

    void OnMouseDrag()
    {
        // Get mouse position in world space
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isMovedEnough = isMovedFarEnough(mousePosition);

        if (isMovedEnough)
        {
            // Remove line if dragging outside of object collider
            bool isMovedOutside = isMovedOutsideBounry(mousePosition);

            if (isMovedOutside)
            {
                RenderBezierCurve();
                RemoveLine();
                return;
            }
            UpdateVectorLine(mousePosition);
        }
    }

    private bool isMovedOutsideBounry(Vector2 mousePosition)
    {
        return !boundryCollider.bounds.Contains(mousePosition);
    }

    private bool isMovedFarEnough(Vector2 mousePosition)
    {
        return Vector2.Distance(mousePosition, controlPoints[controlPoints.Count - 1]) > nextPointThreshold;
    }

    void OnMouseUp()
    {
        RenderBezierCurve();
        RemoveLine();
        ResetControlPoints();
    }

    void UpdateLine(Vector2 newFingerPosition)
    {
        // Save finger position
        controlPoints.Add(newFingerPosition);
        lineRenderer.positionCount += 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, newFingerPosition);
    }

    void RemoveLine()
    {
        // Remove line
        lineRenderer.positionCount = 0;
    }

    void RenderBezierCurve()
    {
        // Draw curve
        curveRenderer.positionCount = pointsInCurve;

        for (int i = 0; i <= pointsInCurve; i++)
        {
            float t = i / (float)pointsInCurve;
            Vector2 point = CalculateQuadraticBezierCurvePoint(t);
            curveRenderer.SetPosition(i, point);
        }
    }

    private Vector2 CalculateQuadraticBezierCurvePoint(float t)
    {
        Vector2 p0 = controlPoints[0];
        Vector2 p1 = controlPoints[controlPoints.Count / 2];
        Vector2 p2 = controlPoints.Last();

        return CalculateQuadraticBezierCurvePoint(p0, p1, p2, t);
    }

    private Vector2 CalculateQuadraticBezierCurvePoint(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        return BezierCurve.Quadratic(p0, p1, p2, t);
    }


    void AddControlPoint(Vector2 newFingerPosition)
    {
        // Add new control point
        controlPoints.Add(newFingerPosition);
    }

    void ResetControlPoints()
    {
        // Reset control points
        controlPoints.Clear();
    }

    void SetStartingPoint()
    {
        Vector2 p0 = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        AddControlPoint(p0);

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, p0);
    }

    void UpdateVectorLine(Vector2 mousePosition)
    {
        Vector2 p0 = controlPoints[0];
        lineRenderer.positionCount = controlPoints.Count + 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, mousePosition);
    }
}
