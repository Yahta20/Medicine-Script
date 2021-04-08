using System.Collections;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

public class TwoClickButton : Basic
{
    public Button button;

    void Start()
    {
        button.onClick.AsObservable().Skip(1)  //после первого нажатия на кнопку ничего не происходит, после второго двигаю объект
            .Subscribe(o => Move(2.5f, 0, 0));
    }

}