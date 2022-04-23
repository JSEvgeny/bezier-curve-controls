using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawTouch : MonoBehaviour
{
    [SerializeField] GameObject lineRendererPrefab;
    [SerializeField] GameObject pointPrefab;
    [SerializeField] float minDistance = 0.1f;
    [SerializeField] int pointsInCurve = 50;
    [SerializeField] float controlPointOffset = 1f;

    BoxCollider2D boundryCollider;
    EdgeCollider2D controlPointCollider;
    EdgeCollider2D lineCollider;

    LineRenderer lineRenderer;
    LineRenderer curveRenderer;
    List<Vector2> controlPoints = new List<Vector2>();

    // Start is called before the first frame update
    void Start()
    {
        GameObject renderObjectLine = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
        GameObject renderObjectCurve = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);

        lineRenderer = renderObjectLine.GetComponent<LineRenderer>();
        curveRenderer = renderObjectCurve.GetComponent<LineRenderer>();

        boundryCollider = GetComponent<BoxCollider2D>();
        controlPointCollider = GetComponent<EdgeCollider2D>();
        lineCollider = renderObjectLine.GetComponent<EdgeCollider2D>();
    }

    void OnMouseDown()
    {
        ResetControlPoints();
        SetStartingPoint();
    }

    void OnMouseDrag()
    {
        // Get mouse position in world space
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isMovedEnough = isMovedFarEnough(mousePosition);

        if (isMovedEnough)
        {
            // // Remove line if dragging outside of object collider
            bool isMovedOutside = isMovedOutsideBounry(mousePosition);

            if (isMovedOutside)
            {
                RemoveLine();
                RemoveCurve();
                return;
            }

            UpdateVectorLine(mousePosition);
        }

        Debug.Log(controlPoints.Count);

        if (controlPoints.Count > 1)
        {
            RenderBezierCurve();
        }
    }

    void OnMouseUp()
    {
        RemoveLine();
        RemoveCurve();
        ResetControlPoints();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collision");

        Vector2 p1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        AddControlPoint(p1);
    }

    void RemoveLine()
    {
        // Remove line
        lineRenderer.positionCount = 0;
    }

    void RemoveCurve()
    {
        // Remove curve
        curveRenderer.positionCount = 0;
    }

    void RenderBezierCurve()
    {
        // Draw curve
        curveRenderer.positionCount = pointsInCurve;

        for (int i = 0; i <= pointsInCurve; i++)
        {
            float t = i / (float)pointsInCurve;
            Vector2 point = CalculateQuadraticBezierCurvePoint(t);
            curveRenderer.material.SetColor("_Color", Color.red);
            curveRenderer.SetPosition(i, point);
        }
    }

    private Vector2 CalculateQuadraticBezierCurvePoint(float t)
    {
        // Start point
        Vector2 p0 = controlPoints[0];
        // Control point
        Vector2 p1 = controlPoints[controlPoints.Count / 2];
        // End point
        Vector2 p2 = controlPoints.Last();

        Debug.Log(p0.x - p1.x > 0);

        Vector2 controlPoint = p0.x - p1.x > 0 ? new Vector2(p1.x + controlPointOffset, p1.y) : new Vector2(p1.x - controlPointOffset, p1.y);

        return CalculateQuadraticBezierCurvePoint(p0, controlPoint, p2, t);
    }

    /**
     * Calculate the point on the quadratic bezier curve
     * 
     * @param p0 - start point
     * @param p1 - control point
     * @param p2 - end point
     * @param t - time
     * @return Vector2 - point on the curve
     */
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

        // Setting both positions to the same point (would point from center otherwise)
        lineRenderer.SetPosition(0, p0);
        lineRenderer.SetPosition(1, p0);

        // Set collider to point
        lineCollider.points = controlPoints.ToArray();
    }

    void UpdateVectorLine(Vector2 mousePosition)
    {
        Vector2 p0 = controlPoints[0];
        lineRenderer.positionCount = controlPoints.Count + 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, mousePosition);

        // Set last control point
        if (controlPoints.Count > 1)
        {
            controlPoints[controlPoints.Count - 1] = mousePosition;
        }

        // Set edge collider points
        lineCollider.points = controlPoints.Concat(new List<Vector2> { mousePosition }).ToArray();
    }

    private bool isMovedOutsideBounry(Vector2 mousePosition)
    {
        return !boundryCollider.bounds.Contains(mousePosition);
    }

    private bool isMovedFarEnough(Vector2 mousePosition)
    {
        return Vector2.Distance(mousePosition, controlPoints[controlPoints.Count - 1]) > minDistance;
    }
}
