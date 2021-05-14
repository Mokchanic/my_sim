using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class MummyAgent : Agent
{
		private Transform tr;
		private Rigidbody rb;
		public Transform targetTr;

		public Renderer floorRd;
		private Material originMt;
		public Material BadMt;
		public Material GoodMt;

		//초기화 작업을 위해 한번 호출됨
		public override void Initialize()
		{
			tr = GetComponent<Transform>();
			rb = GetComponent<Rigidbody>();
			originMt = floorRd.material;
		}

		//에피소드가 시작될 때 마다 호출
		public override void OnEpisodeBegin()
		{
			//물리력 초기화
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;

			//에이전트 위치 불규칙 변경
			tr.localPosition = new Vector3(Random.Range(-4.0f, 4.0f)
									, 0.05f
									, Random.Range(-4.0f, 4.0f));
			targetTr.localPosition = new Vector3(Random.Range(-4.0f, 4.0f)
										,0.55f
										,Random.Range(-4.0f, 4.0f));

			StartCoroutine(RevertMaterial());
		
		}

		IEnumerator RevertMaterial()
		{
			yield return new WaitForSeconds(0.2f);
			floorRd.material = originMt;
		}

		//환경 정보를 관측 및 수집 정책 결정을 위해 브레인에 전달
		public override void CollectObservations(Unity.MLAgents.Sensors.VectorSensor sensor)
		{
			sensor.AddObservation(targetTr.localPosition); //(x, y, z)
			sensor.AddObservation(tr.localPosition); //(x, y, z)
			sensor.AddObservation(rb.velocity.x); //(x)
			sensor.AddObservation(rb.velocity.z); //(z) total 8
		}

		//브레인(정책)으로 부터 전달 받은 행동을 실행함
		public override void OnActionReceived(float[] vectorAction)
		{
			float h = Mathf.Clamp(vectorAction[0], -1.0f, 1.0f);
			float v = Mathf.Clamp(vectorAction[1], -1.0f, 1.0f);
			Vector3 dir = (Vector3.forward * v) + (Vector3.right * h);
			rb.AddForce(dir.normalized * 50.0f);

			//지속적 이동을 위한 마이너스 보상
			SetReward(-0.001f);
		}

		//개발자가 직접 명령을 내릴때 호출하는 메소드(테스트 혹은 모방학습에 사용)
		public override void Heuristic(float[] actionsOut)
		{
			actionsOut[0] = Input.GetAxis("Horizontal"); //좌 우
			actionsOut[1] = Input.GetAxis("Vertical"); //상, 하
			Debug.Log($"[0] = {actionsOut[0]} [1]={actionsOut[1]}");
		}

		//충돌 및 보상 처리
		void OnCollisionEnter(Collision coll) 
		{
			if (coll.collider.CompareTag("DEAD_ZONE"))
			{
				floorRd.material = BadMt;
				SetReward(-1.0f);
				EndEpisode();
			}

			if (coll.collider.CompareTag("TARGET"))
			{
				floorRd.material = GoodMt;
				SetReward(1.0f);
				EndEpisode();
			}
		}
}
