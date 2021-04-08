using System.Collections;
using UnityEngine;
using UniRx;
using System;

public class DeadUpdate : Basic
{
    void Start()
    {
        //Observable.Timer - передаёт значение только один раз после задоного времени
        Observable.Timer(TimeSpan.FromSeconds(5))  //запускается таймер на 5 сек и после достижения 5 сек уничтожаю объект
            .Subscribe(n => Destroy(gameObject));

        //Observable.Interval - передаёт значение каждый раз после истечения заданого времени
        Observable.Interval(TimeSpan.FromMilliseconds(500))  //запускается таймер и каждые 0,5 сек двигаю объект
            .Subscribe(b => Move(0.2f, 0, 0));
    }

}