using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomCameraMovement : MonoBehaviour
{
    private const float REDIRECT_INTERVAL = 0.5f;
    private float redirectTimer = 0f;

    private Vector3 currentDirection = Vector3.zero;
    private float currentSpeed = 0f;

    private bool moving = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (this.moving)
        {
            this.transform.Translate(this.currentDirection * this.currentSpeed);
        }

        if (this.redirectTimer > 0f)
        {
            this.redirectTimer -= Time.deltaTime;

            if (this.redirectTimer <= 0f)
            {
                this.ChangeDirection();
                this.redirectTimer = REDIRECT_INTERVAL;
            }
        }
    }

    public void StartMoving()
    {
        this.moving = true;
        redirectTimer = REDIRECT_INTERVAL;
    }

    public void StopMoving()
    {
        this.moving = false;
    }

    private void ChangeDirection()
    {
        this.currentDirection = new Vector3(Random.value, Random.value, Random.value);
        this.currentSpeed = Random.Range(0.01f, 0.05f);
    }
}
