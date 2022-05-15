using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrawTouch : MonoBehaviour
{
    [SerializeField] GameObject lineRendererPrefab;
    [SerializeField] GameObject curveRendererPrefab;
    [SerializeField] GameObject pointPrefab;
    [SerializeField] float minDistance = 0.1f;
    [SerializeField] int pointsInCurve = 50;
    [SerializeField] bool debug;

    BoxCollider2D boundryCollider;
    EdgeCollider2D controlPointCollider;
    EdgeCollider2D lineCollider;

    LineRenderer lineRenderer;
    LineRenderer curveRenderer;
    [ReadOnlyInspector] [SerializeField] List<Vector2> controlPoints = new List<Vector2>(3);

    [ReadOnlyInspector] [SerializeField] bool hasControlPoint = false;


    // Start is called before the first frame update
    void Start()
    {
        // Initilize line renderer
        GameObject renderObjectLine = InitializeLineRenderer(lineRendererPrefab, ref lineRenderer, 0.05f);
        // Initilize curve renderer
        GameObject renderObjectCurve = InitializeLineRenderer(curveRendererPrefab, ref curveRenderer, 0.03f);

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
            // bool isMovedOutside = isMovedOutsideBounry(mousePosition);

            // if (isMovedOutside)
            // {
            //     RemoveLine();
            //     RemoveCurve();
            //     return;
            // }

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
        hasControlPoint = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collision");

        Vector2 p1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!hasControlPoint) {
            AddControlPoint(p1);
            hasControlPoint = true;
        }
    }

    private GameObject InitializeLineRenderer(GameObject renderPrefab, ref LineRenderer renderer, float lineWidth)
    {
        // Spawn and define line renderer
        GameObject obj = Instantiate(renderPrefab, Vector3.zero, Quaternion.identity);
        renderer = obj.GetComponent<LineRenderer>();
        // Set line renderer properties
        renderer.startWidth = lineWidth;
        renderer.endWidth = lineWidth;
        // Set visibility
        renderer.enabled = debug;

        return obj;
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
            curveRenderer.SetPosition(i, point);
        }
    }

    private Vector2 CalculateQuadraticBezierCurvePoint(float t)
    {
        // Start point
        Vector2 p0 = controlPoints[0];
        // Control point
        Vector2 p1 = controlPoints[1];
        // End point
        Vector2 p2 = controlPoints.Last();

        return CalculateQuadraticBezierCurvePoint(p0, p1, p2, t);
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
        lineRenderer.positionCount = controlPoints.Count + 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, mousePosition);

        Debug.Log("Controll POINTS COUNT: " + controlPoints.Count);

        // Set last control point
        if (controlPoints.Count == 2)
        {
            AddControlPoint(mousePosition);
            UpdateMiddleControlPoint();
        }

        if (controlPoints.Count > 2)
        {
            controlPoints[2] = mousePosition;
            UpdateMiddleControlPoint();
        }

        // Set edge collider points
        lineCollider.points = controlPoints.Concat(new List<Vector2> { mousePosition }).ToArray();
    }

    void UpdateMiddleControlPoint()
    {
        // Assuming we always have 3 control points
        // Middle point
        Vector2 p1 = controlPoints[1];
        // End point
        Vector2 p2 = controlPoints.Last();

        // Get opposite point of the line between p1 and p2
        
    
        Debug.Log("Middle point: " + p1);
        Debug.Log("Last point: " + p2);
    }

    bool isMovedOutsideBounry(Vector2 mousePosition)
    {
        return !boundryCollider.bounds.Contains(mousePosition);
    }

    bool isMovedFarEnough(Vector2 mousePosition)
    {
        return Vector2.Distance(mousePosition, controlPoints[controlPoints.Count - 1]) > minDistance;
    }
}
