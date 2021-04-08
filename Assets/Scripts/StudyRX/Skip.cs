using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Skip : Basic
{
    void Start()
    {
        this.UpdateAsObservable()
            .Skip(100)  //пропускает первые 100 кадров
            .Subscribe(t => Move(-.01f, 0, 0));  //перемещаю объект
    }

}