using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class SimpleCreate : MonoBehaviour
{


    void Start()
    {
        Observable.Create<int>(observer =>
        {
            observer.OnNext(0);
            observer.OnNext(1);
            observer.OnNext(3);
            observer.OnNext(4);
            observer.OnNext(5);

            return Disposable.Empty;
        })
            .Subscribe(u =>
            {
                Debug.Log(u);
            });
    }

}