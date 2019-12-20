using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKChainRoot : MonoBehaviour
{
    /// <summary>
    /// Add all the IK chains in the system, the chains will be run in order 0 -> n
    /// </summary>
    public IKChain[] chains;

    public IKChain rootChain; //Kept separate to update start nodes of any chains attached

    // Use this for initialization
    void Start()
    {
        rootChain.InitialiseChain();
        for (int i = 0; i < chains.Length; i++)
        {
            chains[i].InitialiseChain();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (chains.Length > 0)
        {
            //Split the update of the chains/root chain 
             rootChain.UpdateChain();

            for (int i = 0; i < chains.Length; i++)
            {
                chains[i].UpdateChain();
            }
        }
        else //if only root chain included - solve root chain like a regular chain
        {
            rootChain.UpdateChain();
        }
    }
}