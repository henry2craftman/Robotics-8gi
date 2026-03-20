using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 멀티 스레딩 예제 1. 공유자원에 동기화 없이 접근
/// 공유자원의 경쟁상태(Race Condition)를 일부러 만들어 보기 -> Worker Thread Win!
/// </summary>
public class TaskManager : MonoBehaviour
{
    int sharedData;
    CancellationTokenSource cts;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cts = new CancellationTokenSource();

        Task.Run(() => WorkerLoop(cts));
    }

    // Update is called once per frame
    void Update()
    {
        sharedData++;
        Debug.Log($"[Main +1] {sharedData}");
    }

    // 다른 스레드가 실행할 함수
    // 스레드를 만들면 반드시 종료해줘야함.
    async Task WorkerLoop(CancellationTokenSource token)
    {
        // 스레드 종료 요청 없으면 계속 반복
        while (!token.IsCancellationRequested)
        {
            sharedData--;

            await Task.Delay(30); // 30ms 대기
        }
    }

    // 프로그램 종료시 실행되는 LifeCycle 함수
    private void OnDestroy()
    {
        cts.Cancel();
        cts.Dispose();
    }
}
