using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;


public class JackalAgent : Agent
{
    // Start is called before the first frame update
	private Rigidbody rb;
	private Transform tr;    
	void Start()
    {
        rb = GetComponent<Rigidbody>();
		tr = GetComponent<Transform>();
    }

	public Transform Target;
	public override void OnEpisodeBegin()
	{
		if (tr.transform.localPosition.y < 0)
        {
            this.rb.angularVelocity = Vector3.zero;
            this.rb.velocity = Vector3.zero;
            this.tr.localPosition = new Vector3( 0, 0.5f, 0);
        }

		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

        // Move the target to a new spot
        Target.localPosition = new Vector3(0.0f,
                                           0.0f,
                                           0.0f);
		//  jackal 
		this.tr.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		this.tr.localRotation = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
    }

	public override void CollectObservations(Unity.MLAgents.Sensors.VectorSensor sensor)
	{
		sensor.AddObservation(Target.localPosition);
		sensor.AddObservation(tr.localPosition);
		sensor.AddObservation(rb.velocity.x);
		sensor.AddObservation(rb.velocity.z);
	}

	public float forceMultiplier = 50;
	public override void OnActionReceived(float[] vectorAction)
	{
		float h = Mathf.Clamp(vectorAction[0], -1.0f, 1.0f);
		float v = Mathf.Clamp(vectorAction[1], -1.0f, 1.0f);
		Vector3 dir = (Vector3.forward * v) + (Vector3.right * h);
		rb.AddForce(dir.normalized * forceMultiplier);
	}

	void OnCollisionEnter(Collision coll)
	{
		if (coll.collider.CompareTag("DEAD_ZONE"))
		{
			SetReward(+1.0f);
	
			EndEpisode();	
		}

		if (coll.collider.CompareTag("TARGET"))
		{
			SetReward(+1.0f);
	
			EndEpisode();	
		}
		
	}


	public override void Heuristic(float[] actionsOut)
	{
		actionsOut[0] = Input.GetAxis("Horizontal"); //좌,우 화살표 키 //-1.0 ~ 0.0 ~ 1.0
		actionsOut[1] = Input.GetAxis("Vertical");   //상,하 화살표 키 //연속적인 값
		Debug.Log($"[0]={actionsOut[0]} [1]={actionsOut[1]}");
	}

}
