using System.Collections.Generic;
using UnityEngine;
public class RopeVisual : MonoBehaviour
{
    public List<Transform> ropeCubes; // Assign all the cubes in the rope to this array
    private LineRenderer lineRenderer;
    public Transform player;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = ropeCubes.Count;
        
        for (int i = 0; i < ropeCubes.Count - 1; i++)
        {
            ropeCubes[i].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void AddPlayerToRopeVisual()
    {
        for (int i = 0; i < ropeCubes.Count; i++)
        {
            ropeCubes[i].GetComponent<MeshRenderer>().enabled = false;
        }
        
        ropeCubes.Add(player);
        lineRenderer.positionCount = ropeCubes.Count;
    }

    public void RemovePlayerFromRopeVisual()
    {
        ropeCubes.Remove(player);
        lineRenderer.positionCount = ropeCubes.Count;
        
        ropeCubes[^1].GetComponent<MeshRenderer>().enabled = true;
    }

    private void Update()
    {
        for (int i = 0; i < ropeCubes.Count; i++)
        {
            lineRenderer.SetPosition(i, ropeCubes[i].position);
        }
    }
    
    
}
