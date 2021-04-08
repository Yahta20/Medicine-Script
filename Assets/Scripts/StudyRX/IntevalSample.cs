using System.Collections;
using UnityEngine;
using UniRx;
using System;

public class IntevalSample : Basic
{
    private int intervalFrameCount = 1;  //тут устанавливается интервал счёта кадров

    void Start()
    {
        Observable.Interval(TimeSpan.FromMilliseconds(50))  //каждые пол секунды
            .Subscribe(r =>
            {
                GetComponent<SpriteRenderer>().color = Color.Lerp(Color.yellow, Color.blue, r / 100f);//с жёлтого передвигаюсь к синему цвету со скоростью - r / 100f
            }).AddTo(this);  //отказ от подписки если объект удалён

        //каждый intervalFrameCount, т.е. в данном случае когда он равен 1 будет каждый кадр в Update выводиться значение z
        Observable.IntervalFrame(intervalFrameCount, FrameCountType.Update)
            .Subscribe(z =>
            {
                //Debug.Log(z);
            }).AddTo(this);
    }
}