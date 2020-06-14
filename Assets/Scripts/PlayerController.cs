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

    public Transform currentCube;
    public Transform clickedCube;
    public Transform indicator;

    [Space]

    public List<Transform> finalPath = new List<Transform>();

    private float blend;

    void Start()
    {
        GetPlayerCurrentPosition();
    }

    void Update()
    {
        GetPlayerCurrentPosition();

        if (currentCube.GetComponent<Walkable>().movingGround)
        {
            transform.parent = currentCube.parent;
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

    void FindPath()
    {
        List<Transform> nextCubes = new List<Transform>();
        List<Transform> pastCubes = new List<Transform>();

        foreach (WalkPath path in currentCube.GetComponent<Walkable>().possiblePaths)
        {
            if (path.active)
            {
                nextCubes.Add(path.target);
                path.target.GetComponent<Walkable>().previousBlock = currentCube;
            }
        }

        pastCubes.Add(currentCube);

        ExploreCube(nextCubes, pastCubes);
        BuildPath();
    }

    void ExploreCube(List<Transform> nextCubes, List<Transform> visitedCubes)
    {
        Transform current = nextCubes.First();
        nextCubes.Remove(current);

        if (current == clickedCube)
        {
            return;
        }

        foreach (WalkPath path in current.GetComponent<Walkable>().possiblePaths)
        {
            if (!visitedCubes.Contains(path.target) && path.active)
            {
                nextCubes.Add(path.target);
                path.target.GetComponent<Walkable>().previousBlock = current;
            }
        }

        visitedCubes.Add(current);

        if (nextCubes.Any())
        {
            ExploreCube(nextCubes, visitedCubes);
        }
    }

    void BuildPath()
    {
        Transform cube = clickedCube;
        while (cube != currentCube)
        {
            finalPath.Add(cube);
            if (cube.GetComponent<Walkable>().previousBlock != null)
                cube = cube.GetComponent<Walkable>().previousBlock;
            else
                return;
        }

        finalPath.Insert(0, clickedCube);
        
        FollowPath();
    }

    void FollowPath()
    {
        Sequence s = DOTween.Sequence();

        walking = true;

        for (int i = finalPath.Count - 1; i > 0; i--)
        {
            float time = finalPath[i].GetComponent<Walkable>().isStair ? 1.5f : 1;

            s.Append(transform.DOMove(finalPath[i].GetComponent<Walkable>().GetWalkPoint(), .2f * time).SetEase(Ease.Linear));

            if(!finalPath[i].GetComponent<Walkable>().dontRotate)
               s.Join(transform.DOLookAt(finalPath[i].position, .1f, AxisConstraint.Y, Vector3.up));
        }

        if (clickedCube.GetComponent<Walkable>().isButton)
        {
            s.AppendCallback(()=>GameManager.instance.RotateRightPivot());
        }

        s.AppendCallback(() => Clear());
    }

    void Clear()
    {
        foreach (Transform t in finalPath)
        {
            t.GetComponent<Walkable>().previousBlock = null;
        }
        finalPath.Clear();
        walking = false;
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
                currentCube = playerHit.transform;

                // if (playerHit.transform.GetComponent<Walkable>().isStair)
                // {
                //     DOVirtual.Float(GetBlend(), blend, .1f, SetBlend);
                // }
                // else
                // {
                //     DOVirtual.Float(GetBlend(), 0, .1f, SetBlend);
                // }
            }
        }
    }
    /* Move player to target platform
    */
    public void MovePlayer() {
        DOTween.Kill(gameObject.transform);
        finalPath.Clear();
        FindPath();
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
                    clickedCube = mouseHit.transform;
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
