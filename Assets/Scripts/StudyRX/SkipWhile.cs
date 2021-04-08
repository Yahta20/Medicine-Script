using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class SkipWhile : Basic
{
    void Start()
    {
        this.UpdateAsObservable()
            .SkipWhile(l => !Input.GetMouseButtonDown(0))  //пока не нажата ЛКМ объект не будет двигаться
            .Subscribe(o => Move(.01f, 0, 0));
    }

}