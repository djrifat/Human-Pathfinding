using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using UnityEngine;

public class Player : MonoBehaviour {

    // Some variables
    public Transform obj;
    float xInput = 0, yInput = 0;

    // Rigidbody
    private Rigidbody rb;

    Stopwatch sw = new Stopwatch();

    // Raycast
    public LayerMask grass, puddle, road, goal;
    int fireHeight = 50;
    float rayDistance = 100;

    // Speed penalties
    public float speed = 20;
    public float waterSpeed = 10;
    public float grassSpeed = 15;

    void Start() {
        rb = GetComponent<Rigidbody>();
        sw.Start();
    }

    // Call input and movement methods
    void Update() {
        GetInput();
        Movement();
        CollisionHandler();
        TargetReached();
    }

    // Get axis input
    void GetInput() {
        // More direct input, use for 2D
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        // Movement more floaty, use for 3D
        //xInput = Input.GetAxis("Horizontal");
        //yInput = Input.GetAxis("Vertical");
    }

    // Move player around in the Scene
    void Movement() {
        Vector3 tempVect = new Vector3(xInput, 0, yInput);
        tempVect = tempVect.normalized * speed * Time.deltaTime;
        rb.MovePosition(obj.transform.position + tempVect);
        //obj.transform.position += tempVect;
    }

    void TargetReached() {
        Ray ray = new Ray(transform.position + Vector3.up * fireHeight, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, goal)) {
            print("Player reached target in: " + sw.ElapsedMilliseconds / 1000 + " s");
            sw.Stop();
        }

    }

    // Raycast collision detection
    // Checks collision based on LayerMask
    public void CollisionHandler() {
        Ray ray = new Ray(transform.position + Vector3.up * fireHeight, Vector3.down);
        RaycastHit hit;

        // Puddle
        if (Physics.Raycast(ray, out hit, rayDistance, puddle)) {
            //Debug.Log("Walking on water");
            speed = waterSpeed;
        }
        // Grass
        else if (Physics.Raycast(ray, out hit, rayDistance, grass)) {
            //Debug.Log("Walking on the grass");
            speed = grassSpeed;
        }
        // Road and puddle
        else if (Physics.Raycast(ray, out hit, rayDistance, road) && Physics.Raycast(ray, out hit, rayDistance, puddle)) {
            speed = waterSpeed;
        }

        else if (Physics.Raycast(ray, out hit, rayDistance, grass) && Physics.Raycast(ray, out hit, rayDistance, puddle)) {
            speed = waterSpeed;
        }
        else {
            //Debug.Log("Not walking on water/grass");
            speed = 20;
        }

        // Road 
        if (Physics.Raycast(ray, out hit, rayDistance, road)) {
            //Debug.Log("Walking on the road");
            speed = 20;
        }   
    }


}
