using System;
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
    [SerializeField] ImpactType impactType = ImpactType.Pop;

    // Joints: connected objects
    [SerializeField] GameObject jointPointPrefab;
    [SerializeField] GameObject tensionPointPrefab;
    [SerializeField] float jointMaxVerticalDistance = 5f;
    [SerializeField] Vector2 jointPoision = new Vector2(0, 0);
    [SerializeField] GameObject skateboard;

    [ReadOnlyInspector] [SerializeField] List<Vector2> controlPoints = new List<Vector2>();
    [ReadOnlyInspector] [SerializeField] bool hasControlPoint = false;

    BoxCollider2D boundryCollider;
    LineRenderer lineRenderer;
    LineRenderer curveRenderer;
    SpringJoint2D springJoint;

    GameObject middlePoint;
    Vector2 springJointForce = new Vector2(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        // Initilize line renderer
        lineRenderer = InitializeLineRenderer(lineRendererPrefab, 0.05f);
        // Initilize curve renderer
        curveRenderer = InitializeLineRenderer(curveRendererPrefab, 0.03f);

        boundryCollider = GetComponent<BoxCollider2D>();

        springJoint = InitializeJoint(jointPointPrefab, tensionPointPrefab, jointPoision);
    }

    void OnMouseDown()
    {
        ResetControlPoints();
        SetStartingPoint();
        springJointForce = springJoint.GetReactionForce(Time.deltaTime);
    }

/*     void FixedUpdate() {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Cast a ray straight down.
        RaycastHit2D hit = Physics2D.Raycast(controlPoints[0], mousePosition - controlPoints[0]);

        // If it hits something...
        if (hit.collider != null)
        {
            Debug.Log("Hit: " + hit.collider.name);
        }
    } */

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

        bool hasAllPoints = controlPoints.Count == 3;

        if (hasAllPoints)
        {
            RenderBezierCurve();
            springJoint.connectedBody.transform.position = controlPoints[1];
        }
    }

    void OnMouseUp()
    {
        RemoveLine();
        RemoveCurve();
        ResetControlPoints();
        hasControlPoint = false;
        Destroy(middlePoint);
        Debug.Log("FORCE: " + springJointForce);
        skateboard.GetComponent<SkateboardController>().ApplyForces(springJointForce);
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

    LineRenderer InitializeLineRenderer(GameObject renderPrefab, float lineWidth)
    {
        // Spawn and define line renderer
        GameObject obj = Instantiate(renderPrefab, Vector3.zero, Quaternion.identity);
        obj.transform.SetParent(transform);
        LineRenderer renderer = obj.GetComponent<LineRenderer>();
        // Set line renderer properties
        renderer.startWidth = lineWidth;
        renderer.endWidth = lineWidth;
        // Set visibility
        renderer.enabled = debug;

        return renderer;
    }

    SpringJoint2D InitializeJoint(GameObject jointPointPrefab, GameObject tensionPointPrefab, Vector2 position)
    {
        // Spawn and define joint
        GameObject jointPoint = Instantiate(jointPointPrefab, position, Quaternion.identity);
        SpringJoint2D springJoint = jointPoint.GetComponent<SpringJoint2D>();
        // Set parent
        jointPoint.transform.SetParent(transform);
        // Set joint properties
        GameObject tensionPoint = Instantiate(tensionPointPrefab, position, Quaternion.identity);
        tensionPoint.transform.SetParent(transform);
        Rigidbody2D tensionPointRb = tensionPoint.GetComponent<Rigidbody2D>();
        springJoint.connectedBody = tensionPointRb;

        return springJoint;
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

        Vector3[] curvePoints = new Vector3[pointsInCurve];
        curvePoints = curvePoints.Select((_, index) => (Vector3)CalculateQuadraticBezierCurvePoint(index / (float)pointsInCurve)).ToArray();

        curveRenderer.SetPositions(curvePoints);
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
    }

    bool CheckIfRaycastHitsCollider(Vector2 mousePosition, string colliderName, out RaycastHit2D[] hits) {

        Vector2 startPoint = controlPoints[0];
        Vector2 direction = mousePosition - startPoint;

        // Cast a ray straight down.
        hits = Physics2D.RaycastAll(startPoint, direction, direction.magnitude);
        Debug.DrawRay(startPoint, direction);

        return hits.Any(hit => hit.collider.name==colliderName);
    }

    string[] GetRaycastHitCollidersNames(RaycastHit2D[] hits) {
        return hits.Select(hit => hit.collider.name).ToArray();
    }

    void UpdateVectorLine(Vector2 mousePosition)
    {
        lineRenderer.positionCount = controlPoints.Count + 1;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, mousePosition);

        Debug.Log("Control POINTS COUNT: " + controlPoints.Count);

        RaycastHit2D[] hits;
        bool hitPopCollider = CheckIfRaycastHitsCollider(mousePosition, "Pop Collider", out hits);
        Debug.Log(hits.ToString());

        if (impactType == ImpactType.Pop && hitPopCollider)
        {
            if (!hasControlPoint)
            {
                AddControlPoint(mousePosition);
                hasControlPoint = true;

                middlePoint = Instantiate(pointPrefab, mousePosition, Quaternion.identity);

                return;
            }
        }

        if (hasControlPoint && controlPoints.Count == 2) {
            AddControlPoint(mousePosition);

            return;
        }

        if (controlPoints.Count > 2)
        {
            controlPoints[2] = mousePosition;
            UpdateMiddleControlPoint(mousePosition);
            // springJoint.connectedBody.transform.position = controlPoints[1];
            middlePoint.transform.position = controlPoints[1];
        }
    }

    void UpdateMiddleControlPoint(Vector2 mousePosition)
    {
        // Assuming we always have 3 control points
        // Middle point
        Vector2 p1 = controlPoints[1];

        controlPoints[1] = - new Vector2(mousePosition.x, (mousePosition.y + 2f ) * 0.5f);
        Debug.DrawLine(p1, controlPoints[1], Color.red, 10f);
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
