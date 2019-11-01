using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Unit : MonoBehaviour {

    public Transform target;
    Vector3[] path;
    int targetIndex;

    // Raycast
    public LayerMask grass, puddle, road, goal;
    int fireHeight = 50;
    float rayDistance = 100;

    // Speed penalties
    public float speed = 20;
    public float waterSpeed = 10;
    public float grassSpeed = 15;

    Stopwatch sw = new Stopwatch();


    private void Start() {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        sw.Start();
    }

    private void Update() {
        CollisionHandler();
        TargetReached();
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccesful) {
        
        if (pathSuccesful) {
            path = newPath;                 // Set path equal to new path   
            StopCoroutine("FollowPath");    // Stop old ongoing Coroutines  
            StartCoroutine("FollowPath");   // Start Coroutine
        }
    }

    IEnumerator FollowPath() {
        Vector3 currentWaypoint = path[0];
        while (true) {
            if (transform.position == currentWaypoint) {
                targetIndex++;   // Move to next waypoint
                if (targetIndex >= path.Length) {
                    yield break;   // Exit Coroutine
                }
                currentWaypoint = path[targetIndex];   // Set waypoint equal to path with the current targetIndex
            }

            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);   // Each frame move Transform closer to currentwaypoint
            yield return null;   // Move to next frame
        }
    }

    void TargetReached() {
        Ray ray = new Ray(transform.position + Vector3.up * fireHeight, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, goal)) {
            print("AI reached target in: " + sw.ElapsedMilliseconds / 1000 + " s");
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

    public void OnDrawGizmos() {
        if (path != null) {
            for (int i = targetIndex; i < path.Length; i++) {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i == targetIndex) {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }

}
