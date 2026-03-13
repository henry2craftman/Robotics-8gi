using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Navigation Agent가 특정 타겟을 따라가게 한다.
/// 속성: Target, Agent List
/// </summary>
public class NavigationManager : MonoBehaviour
{
    public Transform target;
    public List<NavMeshAgent> agents;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (agents != null || agents.Count > 0)
        //{
        //    // Navigation Agent가 특정 타겟을 따라가게 한다.
        //    foreach (NavMeshAgent agent in agents)
        //    {
        //        agent.SetDestination(target.position);
        //    }            
        //}

        if(Input.GetMouseButtonDown(0)) // 0: 왼쪽버튼, 1: 마우스휠클릭, 2: 오른쪽버튼
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // nearPlane의 위치 기준으로 광선을 발사

            RaycastHit hitInfo; // 충돌체(collider)가 있는 물체에 부딪힌 정보

            if(Physics.Raycast(ray, out hitInfo, 100)) // 100m의 광선을 발사
            {
                Debug.LogWarning("클릭된 위치: " + hitInfo.point);

                Debug.DrawLine(ray.origin, hitInfo.point, Color.green, 2f); // 테스트용 라인 그리기

                if (agents != null || agents.Count > 0)
                {
                    // Navigation Agent가 특정 타겟을 따라가게 한다.
                    foreach (NavMeshAgent agent in agents)
                    {
                        agent.SetDestination(hitInfo.point);
                    }
                }
            }
        }
    }
}
