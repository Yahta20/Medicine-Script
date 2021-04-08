using System.Collections;
using UnityEngine;
using UniRx;
using System;

public class SafeUpdate : Basic
{
    // Start is called before the first frame update
    void Start()
    {
        //Observable.Timer(TimeSpan.FromSeconds(5))
        //    .Subscribe(z => Destroy(gameObject));

        //Observable.Interval(TimeSpan.FromMilliseconds(500))
        //    .Subscribe(j => Move(.2f, 0, 0))
        //    .AddTo(this);  //если объект удалён(уничтотжен), то отказываюсь от подписки

        //Observable.Interval(TimeSpan.FromMilliseconds(500))
        //    .TakeUntilDestroy(this)  // Исключение при удалении //пока объект не уничтожен выполняю действие
        //    .Subscribe(h => Move(.2f, 0, 0));


        //вручную отказываться от подписки
        IDisposable mover = Observable.Interval(TimeSpan.FromMilliseconds(500))
           .TakeUntilDestroy(this)  
           .Subscribe(h => Move(.2f, 0, 0));

        Observable.Timer(TimeSpan.FromSeconds(5))
            .Subscribe(h =>
            {
                mover.Dispose();  //отказываюсь от подписки
                Destroy(gameObject);
            });
    }

}