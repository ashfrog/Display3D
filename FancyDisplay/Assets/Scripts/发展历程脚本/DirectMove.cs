using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectMove : MonoBehaviour
{
    public float moveSpeed = 5.0f;

    public Vector3 direct = Vector3.left;

    public float destroyx = -100f;


    private void Start()
    {

    }

    private void Update()
    {
        transform.Translate(direct * moveSpeed * Time.deltaTime);
        CheckIfOffScreen();
    }

    private void CheckIfOffScreen()
    {
        if (transform.position.x < destroyx)
        {
            Destroy(gameObject);
        }
    }
}
