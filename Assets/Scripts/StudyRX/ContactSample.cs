using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class ContactSample : Basic
{
    void Start()
    {
        //первые 150 кадров которые указаны в Take объект движется по оси У
        var first = this.UpdateAsObservable().Select(l => new Vector3(0, 0.01f, 0));

        //вторые 150 кадров которые указаны в Take и далее без остановки объект движется по оси Х
        var second = this.UpdateAsObservable().Select(p => new Vector3(0.01f, 0, 0));

        //Concat используется для соединения двух коллекций(наборов)
        first.Take(150).Concat(second).Subscribe(i => Move(i.x, i.y, i.z));
    }
}