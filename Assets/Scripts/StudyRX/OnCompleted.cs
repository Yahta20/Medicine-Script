using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class OnCompleted : Basic
{
    void Start()
    {
        this.UpdateAsObservable()
            .Take(100)  //первые 100 кадров
            .Subscribe(p => { Move(0.02f, 0, 0); },  //передвигаю объект вправо
            () => { GetComponent<SpriteRenderer>().color = Color.blue; });  //после нахожу компонент спрайта и меню цвет

        Observable.Timer(TimeSpan.FromSeconds(5))
            .Subscribe(l => { GetComponent<SpriteRenderer>().color = Color.red;
                Move(0, 0.5f, 0); });
    }

}