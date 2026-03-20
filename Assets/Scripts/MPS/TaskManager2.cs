using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 멀티 스레딩 예제2. 공유자원 접근 관리
/// shareData에 접근 권한을 상호배제(Mutex, Mutual Exclusion) 기능을 사용하여 관리
/// C#의 Mutex: lock 키워드
/// ex. 화장실에 문을 잠그고 나올 때 까지 다른 스레드는 대기한다.
/// </summary>
public class TaskManager2 : MonoBehaviour
{
    int sharedData;
    CancellationTokenSource cts;
    object lockObj = new object(); // 자물쇠 역할

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cts = new CancellationTokenSource();

        Task.Run(() => WorkerLoop(cts));
    }

    // Update is called once per frame
    void Update()
    {
        lock (lockObj)  // 화장실 문 잠구기
        {
            sharedData++;
        }               // 화장실 문 열기

        Debug.Log($"[Main +1] {sharedData}");
    }

    // 다른 스레드가 실행할 함수
    // 스레드를 만들면 반드시 종료해줘야함.
    async Task WorkerLoop(CancellationTokenSource token)
    {
        // 스레드 종료 요청 없으면 계속 반복
        while (!token.IsCancellationRequested)
        {
            lock(lockObj) // 락 걸기
            {
                sharedData--;
            }             // 락 해제하기

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
