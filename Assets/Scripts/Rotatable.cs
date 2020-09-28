using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
/**
 * This script should be assigned to rotatable level
 */
public class Rotatable : MonoBehaviour
{
    public char axis;

    private float rotateTime = .5f;
    private int x = 0;
    private int y = 0;
    private int z = 0;
    // Start is called before the first frame update
    void Start()
    {
        // generic rotating depend on axis
        switch(axis)
        {
            case 'x': x = 1; break;
            case 'y': y = 1; break;
            case 'z': z = 1; break;
        }
    }

    void Update()
    {
        Sequence s = DOTween.Sequence();

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            int direction = Input.GetKeyDown(KeyCode.LeftArrow) ? 1 : -1;

            transform.DOComplete();

            transform.DORotate(new Vector3(90.0f * x * direction, 90.0f * y * direction, 90.0f * z * direction), 
                                rotateTime, 
                                RotateMode.LocalAxisAdd
                            ).SetEase(Ease.OutBack);
        }
    }
}
