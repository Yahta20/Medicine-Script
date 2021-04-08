using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;

public class ClickButton : Basic
{
    public Button button;

    void Start()
    {
        button.onClick.AsObservable().First()  //после нажатие на кнопку первый раз, двигаю объект
            .Subscribe(l => Move(2f, 0, 0));
    }

}