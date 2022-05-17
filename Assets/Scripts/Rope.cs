using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    LineRenderer lineRenderer;
    List<RopeSegment> segments = new List<RopeSegment>();
    [SerializeField] float ropeSegLength = 0.25f;
    [SerializeField] int segmentLength = 35;
    [SerializeField] float lineWidth = 0.1f;
    [SerializeField] Vector2 forceGravity = new Vector2(0, -1f);

    [SerializeField] int constaintsToApply = 50;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        Vector2 ropeStartPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        for (int i = 0; i < segmentLength; i++)
        {
            segments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= ropeSegLength;
        }
    }

    // Update is called once per frame
    void Update()
    {
        DrawRope();
    }

    void FixedUpdate()
    {
        Simulate();
    }

    void Simulate()
    {
        // Simulate the rope
        for (int i = 0; i < segmentLength - 1; i++)
        {
            RopeSegment firstSegment = segments[i];
            Vector2 velocity = firstSegment.currentPosition - firstSegment.previousPosition;
            firstSegment.previousPosition = firstSegment.currentPosition;
            firstSegment.currentPosition += velocity;
            firstSegment.currentPosition += forceGravity * Time.deltaTime;
            segments[i] = firstSegment;
        }

        // Constraints
        for(int i = 0; i < constaintsToApply; i++)
        {
            ApplyConstraint();
        }
    }

    void ApplyConstraint()
    {
        RopeSegment firstSegment = segments[0];
        firstSegment.currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        segments[0] = firstSegment;

        for (int i = 0; i < segmentLength - 1; i++)
        {
            RopeSegment firstSeg = segments[i];
            RopeSegment nextSeg = segments[i + 1];

            float dist = (firstSeg.currentPosition - nextSeg.currentPosition).magnitude;
            float error = Mathf.Abs(dist - ropeSegLength);
            Vector2 changeDir = Vector2.zero;

            if (dist > ropeSegLength) // Too long
            {
                changeDir = (firstSeg.currentPosition - nextSeg.currentPosition).normalized;
            }
            else if (dist < ropeSegLength) // Too short
            {
                changeDir = (nextSeg.currentPosition - firstSeg.currentPosition).normalized;
            }

            Vector2 changeAmount = changeDir * error;
            if (i != 0)
            {
                firstSeg.currentPosition -= changeAmount * 0.5f;
                segments[i] = firstSeg;
                nextSeg.currentPosition += changeAmount * 0.5f;
                segments[i + 1] = nextSeg;
            } else {
                nextSeg.currentPosition += changeAmount;
                segments[i + 1] = nextSeg;
            }
        }
    }

    void DrawRope()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[segmentLength];
        for (int i = 0; i < segmentLength; i++)
        {
            ropePositions[i] = segments[i].currentPosition;
        }
        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }
}

internal struct RopeSegment
{
    public Vector2 currentPosition;
    public Vector2 previousPosition;

    public RopeSegment(Vector2 position)
    {
        this.currentPosition = position;
        this.previousPosition = position;
    }
}