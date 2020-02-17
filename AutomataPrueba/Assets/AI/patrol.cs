using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class patrol : AI_Agent
{
    [SerializeField]
    Transform target;
    Vector3[] waypoints;
    public int maxWaypoints = 10;
    public float angularVelocity = 0.5f;
    public float speed = 2f;

    int actualWaypoint = 0;
    float halfAngle = 30.0f;
    float coneDistance = 5.0f;
    Color gizmoColor = Color.white;


    void initPositions()
    {
        List<Vector3> waypointsList = new List<Vector3>();
        float anglePartition = 360.0f / (float)maxWaypoints;
        for (int i = 0; i < maxWaypoints; ++i)
        {
            Vector3 v = transform.position + 5 * Vector3.forward * Mathf.Cos(i * anglePartition)
                + 5 * Vector3.right * Mathf.Sin(i * anglePartition);
            waypointsList.Add(v);

        }
        waypoints = waypointsList.ToArray();
    }

    private void OnDrawGizmos()
    {
        if (UnityEditor.EditorApplication.isPlaying)
        {
            for (int i = 0; i < maxWaypoints; i++)
            {
                Gizmos.DrawSphere(waypoints[i], 1.0f);
            }
        }


        Vector3 rightSide = Quaternion.Euler(Vector3.up * halfAngle) * transform.forward * coneDistance;
        Vector3 leftSide = Quaternion.Euler(Vector3.up * -halfAngle) * transform.forward * coneDistance;

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * coneDistance);
        Gizmos.DrawLine(transform.position,
          transform.position + rightSide);
        Gizmos.DrawLine(transform.position,
        transform.position + leftSide);


        Gizmos.DrawLine(transform.position + leftSide,
         transform.position + transform.forward * coneDistance);

        Gizmos.DrawLine(transform.position + rightSide,
        transform.position + transform.forward * coneDistance);

        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position + Vector3.up * 2, 0.5f);
    }

    void idle()
    {
        gizmoColor = Color.white;

        coneDistance = 5;
        halfAngle = 30;

        if (Input.GetKeyDown(KeyCode.A))
        {
            setState(getState("goto"));
        }
    }


    void goTo(Vector3 pos)
    {
        gizmoColor = Color.yellow;

        float maxYaw = Vector3.SignedAngle(transform.forward,
        pos - transform.position,

         Vector3.up);
        float vel = Mathf.Min(angularVelocity, Mathf.Abs(maxYaw));
        vel *= Mathf.Sign(maxYaw);

        transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
            transform.eulerAngles.y + vel,
            transform.eulerAngles.z);

        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void goToWaypoint()
    {
        goTo(waypoints[actualWaypoint]);

        if (Vector3.Distance(transform.position, waypoints[actualWaypoint]) <= 1.0f)
        {
            setState(getState("nextwp"));
        }
        else if (checkInCone(target.position))
        {
            coneDistance = 10;
            halfAngle = 60;
            setState(getState("player"));
        }

    }

    void calculateNextWaypoint()
    {
        actualWaypoint = (++actualWaypoint) % waypoints.Length;
        setState(getState("goto"));

    }

    bool checkInCone(Vector3 pos)
    {
        if (Vector3.Angle(transform.forward, pos - transform.position) <= halfAngle &&
            Vector3.Distance(transform.position, pos) <= coneDistance)
            return true;

        return false;
    }

    void goToPlayer()
    {
        goTo(target.position);

        gizmoColor = Color.red;

        if (!checkInCone(target.position))
        {
            coneDistance = 5;
            halfAngle = 30;
            setState(getState("goto"));
        }
        else if (Vector3.Distance(transform.position, target.position) <= 4f)
        {
            setState(getState("idlewar"));
        }
    }


    float angleToGo;
    float totalAngle;
    float countAngle = 0;

    void idleWar()
    {
        gizmoColor = Color.green;

        setState(getState("chooseOrbit"));
    }

    float angleCount = 0;

    void chooseOrbit()
    {
        angleToGo = Random.Range(60, 181);
        angleCount = 0;
        totalAngle = transform.rotation.eulerAngles.y + angleToGo;
        /*
         * Decido la rotación
         * 
         */
        setState(getState("OrbitRight"));

    }

    void OrbitRight()
    {
        gizmoColor = Color.blue;

        float dist = Vector3.Distance(target.position, transform.position);
        transform.position += dist
            * transform.forward;

        float mag = Mathf.Min(angleToGo, angularVelocity); 
        angleCount += Mathf.Min(angleToGo, angularVelocity);

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y + mag,
            transform.rotation.eulerAngles.z);

        transform.position += dist
            * -transform.forward;

   
        if (angleCount >= totalAngle)
        {
            setState(getState("fight"));
        }
    } 


    void OrbitLeft()
    {
        gizmoColor = Color.blue;

        transform.position += Vector3.Distance(target.position, transform.position) 
            * transform.forward;
        countAngle += Mathf.Min(angleToGo, angularVelocity);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y - Mathf.Min(angleToGo, angularVelocity),
            transform.rotation.eulerAngles.z);
        transform.position += Vector3.Distance(target.position, transform.position)
            * -transform.forward;


        if (countAngle <= totalAngle)
        {
            setState(getState("fight"));
        }
    }

    void fight()
    {
        gizmoColor = Color.red;

        DamageMessage dmgMsg = new DamageMessage();

        dmgMsg.SetMessageAndSendMessage(transform, target.transform, typeof(life), 10f);

        setState(getState("idle"));
    }

    void Start()
    {
 
        initPositions();
        actualWaypoint = 0;
        initState("idle", idle);
        initState("goto", goToWaypoint);
        initState("nextwp", calculateNextWaypoint);
        initState("player", goToPlayer);
        initState("idlewar", idleWar);
        initState("chooseOrbit", chooseOrbit);
        initState("OrbitRight", OrbitRight);
        initState("fight", fight);

        setState(getState("idle"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
