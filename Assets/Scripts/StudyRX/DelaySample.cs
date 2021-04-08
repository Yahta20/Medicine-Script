using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using System;

public class DelaySample : Basic
{
    public Button button;

    void Start()
    {
        button.onClick.AsObservable()  //обрабатываю нажатие кнопки
            .Delay(TimeSpan.FromSeconds(5))  //задержка на 5 сек
            .Subscribe(p => Move(2, 0, 0));  //подписываюсь на метод, который передвигает объект
    }

}