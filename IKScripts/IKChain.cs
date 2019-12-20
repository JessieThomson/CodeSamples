using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKChain : MonoBehaviour
{
    public List<RotationConstraint> nodes;
    public Transform target;
    [Header("IK Params")]
    public bool restrictRotation = true;

    [Range(0, 1)] public float weight = 1f;

    [Header("Debug")] public bool debugLines = true;
    [HideInInspector] public float[] lengths;
    [HideInInspector] public Transform rootNode;

    private float chainLength = 0f;
    private Vector3[] solverLocalPositions = new Vector3[0];
    private Vector3 lastLocalDirection;
    private Vector3 startPosition; //Position of end node to interpolate chain from depending on weight

    /// <summary>
    /// Gets the Quaternion from rotation "from" to rotation "to".
    /// </summary>
    public static Quaternion FromToRotation(Quaternion from, Quaternion to)
    {
        if (to == from) return Quaternion.identity;

        return to * Quaternion.Inverse(from);
    }

    // Use this for initialization
    public void InitialiseChain() //Start()
    {
        if (nodes.Count <= 0) nodes = new List<RotationConstraint>(GetComponentsInChildren<RotationConstraint>()); // Auto fill the joints list
        rootNode = nodes[0].transform;
        solverLocalPositions = new Vector3[nodes.Count];
        lengths = new float[nodes.Count - 1];
        startPosition = nodes[nodes.Count - 1].transform.position;
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Initialise();
            // Find out which local axis is directed at child/target position
            if (i < nodes.Count - 1)
            {
                lengths[i] = (nodes[i].transform.position - nodes[i + 1].transform.position).magnitude;
                chainLength += Vector3.Magnitude(nodes[i + 1].transform.position - nodes[i].transform.position);

                Vector3 nextPosition = nodes[i + 1].transform.position;
                nodes[i].axis = Quaternion.Inverse(nodes[i].transform.rotation) * (nextPosition - nodes[i].transform.position);
            }
            else
            {
                nodes[i].axis = Quaternion.Inverse(nodes[i].transform.rotation) * (nodes[nodes.Count - 1].transform.position - rootNode.position);
            }
            solverLocalPositions[i] = Quaternion.Inverse(GetParentSolverRotation(i)) * (nodes[i].transform.position - GetParentSolverPosition(i));
        }
    }

    #region SolverFunctions

    private Vector3 GetParentSolverPosition(int index)
    {
        if (index > 0) return nodes[index - 1].transform.position;
        if (nodes[0].transform.parent == null) return Vector3.zero;
        return nodes[0].transform.parent.position;
    }

    private Quaternion GetParentSolverRotation(int index)
    {
        if (index > 0) return nodes[index - 1].transform.rotation;
        if (nodes[0].transform.parent == null) return Quaternion.identity;
        return nodes[0].transform.parent.rotation;
    }

    private void SolverMove(int index, Vector3 offset)
    {
        for (int i = index; i < nodes.Count; i++)
        {
            nodes[i].transform.position += offset;
        }
    }

    private void SolverRotate(int index, Quaternion rotation, bool recursive)
    {
        for (int i = index; i < nodes.Count; i++)
        {
            nodes[i].transform.rotation = rotation * nodes[i].transform.rotation;
            if (!recursive) return;
        }
    }

    private void SolverRotateChildren(int index, Quaternion rotation)
    {
        for (int i = index + 1; i < nodes.Count; i++)
        {
            nodes[i].transform.rotation = rotation * nodes[i].transform.rotation;
        }
    }

    private void SolverMoveChildrenAroundPoint(int index, Quaternion rotation)
    {
        for (int i = index + 1; i < nodes.Count; i++)
        {
            Vector3 dir = nodes[i].transform.position - nodes[index].transform.position;
            nodes[i].transform.position = nodes[index].transform.position + rotation * dir;
        }
    }

    #endregion SolverFunctions

    public void ForwardReach(Vector3 targetPosition)
    {
        // Lerp last bone's solverPosition to position - IKPositionWeight is how close you want the chain to move to the target
        nodes[nodes.Count - 1].transform.position = Vector3.Lerp(startPosition, targetPosition, weight); // <- put this for each node? Need a weight for each to make some nodes heavier
        for (int i = nodes.Count - 2; i > -1; i--)
        {
            if (i > 0)
            {
                nodes[i].transform.position = nodes[i + 1].transform.position + Vector3.Normalize(nodes[i].transform.position - nodes[i + 1].transform.position) * lengths[i];
            }
        }
    }

    public void BackwardReach()
    {
        nodes[0].transform.position = rootNode.position;

        // Applying rotation limits bone by bone
        for (int i = 1; i < nodes.Count; i++)
        {
            nodes[i].transform.position = nodes[i - 1].transform.position + Vector3.Normalize(nodes[i].transform.position - nodes[i - 1].transform.position) * lengths[i - 1];
            if (debugLines)
            {
                Debug.DrawLine(nodes[i].transform.position, nodes[i].transform.position + (nodes[i].transform.rotation * Vector3.forward) * lengths[i - (1 * i)], Color.red, 1f);
                Debug.DrawLine(nodes[i].transform.position, nodes[i].transform.position + (nodes[i].transform.rotation * Vector3.up) * lengths[i - (1 * i)], Color.blue, 1f);
            }
        }
    }

    public void CheckRotation()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i; j < nodes.Count - 1; j++)
            {
                Quaternion fromTo = Quaternion.FromToRotation(nodes[j].transform.rotation * nodes[j].axis, nodes[j + 1].transform.position - nodes[j].transform.position);
                nodes[j].transform.rotation = fromTo * nodes[j].transform.rotation;
                if (j < nodes.Count - 1)
                {
                    // Positioning the next bone to its default local position
                    nodes[j + 1].transform.position = nodes[j].transform.position + nodes[j].transform.rotation * solverLocalPositions[j + 1];
                }
            }
        }
        if (restrictRotation)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                // Rotation Constraints

                Quaternion rotation = nodes[i].transform.rotation;
                nodes[i].CheckRotation();
                nodes[i].LimitTwistRotation();

                if (Quaternion.Angle(rotation, nodes[i].transform.rotation) > 2f) // If restriction changes angle, redjust rest of chain
                {
                    Quaternion fromTo = FromToRotation(rotation, nodes[i].transform.rotation);
                    SolverRotateChildren(i, fromTo);

                    if (i < nodes.Count - 1)
                    {
                        // Positioning the next bone to its default local position
                        nodes[i + 1].transform.position = nodes[i].transform.position + nodes[i].transform.rotation * solverLocalPositions[i + 1];
                    }
                }
                else
                {
                    if (i < nodes.Count - 1)
                    {
                        Quaternion afterLimit = Quaternion.FromToRotation(nodes[i].transform.rotation * nodes[i].axis, nodes[i + 1].transform.position - nodes[i].transform.position);
                        afterLimit = afterLimit * nodes[i].transform.rotation;
                        Quaternion fromTo = afterLimit * Quaternion.Inverse(nodes[i].transform.rotation);
                        nodes[i].transform.rotation = afterLimit;
                        SolverRotateChildren(i, fromTo);

                        nodes[i + 1].transform.position = nodes[i].transform.position + nodes[i].transform.rotation * solverLocalPositions[i + 1];
                    }
                }
            }
            // Reconstruct solver rotations to protect from invalid Quaternions
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].transform.rotation = Quaternion.LookRotation(nodes[i].transform.rotation * Vector3.forward, nodes[i].transform.rotation * Vector3.up);
            }
        }
        
    }

    // Update is called from IKChainRoot
    public void UpdateChain()
    {
            ForwardReach(target.position); 
            BackwardReach();
            CheckRotation();
    }
   
}