using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

/* This script need to be assigned to a player object */
[SelectionBase]
public class PlayerController : MonoBehaviour
{
    public bool walking = false;

    [Space]

    public Transform currentPlatform;
    public Transform targetPlatform;
    public Transform indicator;

    private float blend;

    void Start()
    {
        GetPlayerCurrentPosition();
    }

    void Update()
    {
        GetPlayerCurrentPosition();

        if (currentPlatform.GetComponent<Walkable>().movingGround)
        {
            transform.parent = currentPlatform.parent;
        }
        else
        {
            transform.parent = null;
        }

        // Handle on click event
        if (Input.GetMouseButtonDown(0))
        {   
            if (isClickedOnPlatform()) {
                MovePlayer();
            }
        }
    }

    void ExecuteMovevements(List<Transform> path)
    {
        Sequence s = DOTween.Sequence();

        walking = true;

        for (int i = path.Count - 1; i >= 0; i--)
        {
            float time = path[i].GetComponent<Walkable>().isStair ? 1.5f : 1;

            // Break moving if path to next node not active
            if (path[i].GetComponent<Walkable>().possiblePaths.FindAll(item => item.active == false).Count > 0) {
                return;
            }

            s.Append(transform.DOMove(path[i].GetComponent<Walkable>().GetWalkPoint(), .2f * time).SetEase(Ease.Linear));

            if(!path[i].GetComponent<Walkable>().dontRotate)
               s.Join(transform.DOLookAt(path[i].position, .1f, AxisConstraint.Y, Vector3.up));
        }
    }

    public void GetPlayerCurrentPosition()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit playerHit;
        Debug.DrawRay(transform.position, fwd * 50, Color.red, 0.5f);

        if (Physics.Raycast(transform.position, fwd, out playerHit, 50))
        {
            if (playerHit.transform.GetComponent<Walkable>() != null)
            {
                currentPlatform = playerHit.transform;
            }
        }
    }

    /* Move player to target platform
    */
    public void MovePlayer() {
        DOTween.Kill(gameObject.transform);
        ClearOldDijkstraAttributes();
        DijkstraSearch();
        List<Transform> path = GetShortestPath();
        ExecuteMovevements(path);
        walking = false;
    }

    public List<Transform> GetShortestPath()
    {
        DijkstraSearch();
        var shortestPath = new List<Transform>();
        shortestPath.Add(targetPlatform);
        BuildShortestPath(shortestPath, targetPlatform);
        // shortestPath;
        return shortestPath;
    }

    private void BuildShortestPath(List<Transform> list, Transform node)
    {
        if (node.GetComponent<Walkable>().NearestToStart == null) return;

        // Fix always go back one platform before moving
        if (node.GetComponent<Walkable>().NearestToStart == currentPlatform) return;

        list.Add(node.GetComponent<Walkable>().NearestToStart);
        BuildShortestPath(list, node.GetComponent<Walkable>().NearestToStart);
    }

    private void DijkstraSearch()
    {
        currentPlatform.GetComponent<Walkable>().MinCostToStart = 0;
        var prioQueue = new List<Transform>();
        prioQueue.Add(currentPlatform);
        do {
            prioQueue = prioQueue.OrderBy(x => x.GetComponent<Walkable>().MinCostToStart).ToList();
            var node = prioQueue.First();
            prioQueue.Remove(node);
            foreach (var cnn in node.GetComponent<Walkable>().possiblePaths.OrderBy(x => x.Cost))
            {
                var childNode = cnn.target;
                if (childNode.GetComponent<Walkable>().Visited)
                    continue;
                if (childNode.GetComponent<Walkable>().MinCostToStart == 0 ||
                    node.GetComponent<Walkable>().MinCostToStart + cnn.Cost < childNode.GetComponent<Walkable>().MinCostToStart)
                {
                    childNode.GetComponent<Walkable>().MinCostToStart = node.GetComponent<Walkable>().MinCostToStart + cnn.Cost;
                    childNode.GetComponent<Walkable>().NearestToStart = node;
                    if (!prioQueue.Contains(childNode))
                        prioQueue.Add(childNode);
                }
            }
            node.GetComponent<Walkable>().Visited = true;
            if (node == targetPlatform)
                return;
        } while (prioQueue.Any());
    }

    private void ClearOldDijkstraAttributes() {
        Walkable[] walkableNodes = FindObjectsOfType<Walkable>();
        foreach (Walkable node in walkableNodes) {
            node.Visited = false;
            node.MinCostToStart = 0;
            node.NearestToStart = null;
        }
    }

    /*  Check if click hits a platform
        set the target platform and return true 
    */
    public bool isClickedOnPlatform() {
        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition); RaycastHit mouseHit;

            if (Physics.Raycast(mouseRay, out mouseHit))
            {
                if (mouseHit.transform.GetComponent<Walkable>() != null)
                {
                    targetPlatform = mouseHit.transform;
                    return true;
                }
            }
        }
        return false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if(transform.childCount > 0) {
            Ray ray = new Ray(transform.GetChild(0).position, -transform.up);
            Gizmos.DrawRay(ray);
        }
    }

    // float GetBlend()
    // {
    //     return GetComponentInChildren<Animator>().GetFloat("Blend");
    // }
    // void SetBlend(float x)
    // {
    //     GetComponentInChildren<Animator>().SetFloat("Blend", x);
    // }

}
