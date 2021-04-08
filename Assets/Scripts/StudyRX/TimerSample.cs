using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class TimerSample : MonoBehaviour
{
    private int dueTimeFrameCount = 100;

    void Start()
    {
        //this.UpdateAsObservable()
        //    .Subscribe(u =>
        //    {
        //        for (int i = 0; i < 6; i++)
        //        {
        //            i = (int)Time.time;
        //            Debug.Log(i);
        //        }
        //    });

        Observable.Timer(TimeSpan.FromSeconds(5))
            .Subscribe(t =>
            {                
                GetComponent<SpriteRenderer>().color = Color.blue;
            })
            .AddTo(this);

        //через некоторое значение колличества кадров которое хранится в dueTimeFrameCount и каждый Update выведеться значение l
        //Observable.TimerFrame(dueTimeFrameCount, FrameCountType.Update)
        //    .Subscribe(l =>
        //    {
        //        Debug.Log(l);
        //    }).AddTo(this);
    }

}