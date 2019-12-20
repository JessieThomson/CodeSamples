using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationConstraint : MonoBehaviour
{
    //Slider for the minimum twist angle
    [Range(0f, -180f)]
    public float twistMinLimit;

    //Slider for the maximum twist angle
    [Range(0f, 180)]
    public float twistMaxLimit;

    //Limiting angle for cone constraint
    [Range(0f, 180f)]
    public float limit;

    // The main axis of the rotation limit.
    public Vector3 axis = Vector3.forward;

    public Vector3 orthoAxis = Vector3.up;

    private void OnValidate()
    {
        orthoAxis = Vector3.ProjectOnPlane(orthoAxis, axis).normalized;
        axis = axis.normalized;
    }

    // The transform of the previous node in the chain
    public Transform prevJointTransform;

    // Orthogonal axis to the forward axis on the previous node in the chain
    [HideInInspector] public Vector3 prevOrthoAxis = Vector3.zero;

    [HideInInspector]
    public Quaternion defaultLocalRotation;

    public Vector3 crossAxis { get { return Vector3.Cross(axis, orthoAxis); } }

    //Current node initial world space axes
    [HideInInspector] public Vector3 wsAxis; //
    [HideInInspector] public Vector3 wsOrthoAxis; //
    [HideInInspector] public Vector3 wsCrossAxis; //

    //Accessor methods for current world space axes 
    [HideInInspector] public Vector3 currWSAxis { get { return transform.TransformDirection(axis); } }
    [HideInInspector] public Vector3 currWSOrthoAxis { get { return transform.TransformDirection(orthoAxis); } }
    [HideInInspector] public Vector3 currWSCrossAxis { get { return transform.TransformDirection(crossAxis); } }

    // Accessor methods for previous node world space axes
    public Vector3 wsPrevAxis { get { return prevJointTransform.TransformDirection(previousAxis); } } //
    public Vector3 wsPrevOrthoAxis { get { return prevJointTransform.TransformDirection(prevOrthoAxis); } } 

    [HideInInspector]
    public Vector3 previousAxis;

    public void Initialise()
    {
        wsAxis = transform.TransformDirection(axis);
        wsOrthoAxis = transform.TransformDirection(orthoAxis);
        wsCrossAxis = transform.TransformDirection(crossAxis);

        prevOrthoAxis = prevJointTransform.InverseTransformDirection(wsOrthoAxis);
        //Find closest axis to the direction between the current node and the next node to find the axis
        previousAxis = prevJointTransform.InverseTransformDirection(wsAxis); 
    }

    /// <summary>
    /// Check the cone constraint angle
    /// </summary>
    public void CheckRotation()
    {
        Quaternion newRot = Quaternion.identity;

        float angle = Mathf.Acos(Vector3.Dot(wsPrevAxis, currWSAxis)) * Mathf.Rad2Deg;

        if (!float.IsNaN(angle) && Mathf.Abs(angle) > limit)
        {
            //Interpolate from Quaternion Identity to the new restricted rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, newRot, (angle - limit) / angle);
        }
    }

    /// <summary>
    /// Check the twist (Yaw rotation) of the nodes
    /// </summary>
    public void LimitTwistRotation()
    {
        if (axis == Vector3.zero) return; // Ignore with zero axes
        if (twistMaxLimit == 0 && twistMinLimit == 0f) return; // Assuming initial rotation is in the reachable area

        //Project previous nodes ortho axis into current node local transformations
        Vector3 worldSpacePrevOrtho = prevJointTransform.TransformVector(prevOrthoAxis);
        Vector3 localSpacePrevOrtho = transform.InverseTransformVector(worldSpacePrevOrtho);
        //Flatten the axes so that they can be compared in same plane
        Vector3 projectedOrtho = Vector3.ProjectOnPlane(localSpacePrevOrtho, axis).normalized;

        Vector3 ortho3 = Vector3.Cross(orthoAxis, axis);
        //Find angle between the axes
        float angle = Mathf.Atan2(Vector3.Dot(projectedOrtho, ortho3), Vector3.Dot(projectedOrtho, orthoAxis)) * Mathf.Rad2Deg;
        float adjust = 0;

        //Limit the rotation if it's outside the bounds
        if (angle > twistMaxLimit)
        {
            adjust = twistMaxLimit - angle;
            transform.Rotate(axis, adjust, Space.Self); 
        }

        if (angle < twistMinLimit)
        {
            adjust = twistMinLimit - angle; 
            transform.Rotate(axis, adjust, Space.Self); 
        }
    }

}