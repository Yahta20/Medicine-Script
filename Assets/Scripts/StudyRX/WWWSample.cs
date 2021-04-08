using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System;

public class WWWSample : MonoBehaviour
{
    void Start()
    {
        ScheduledNotifier<float> progress = new ScheduledNotifier<float>();  // Наблюдаемый, который подталкивает прогресс DL

        var watcher = progress.Subscribe(p => Debug.Log(p));  // Отображение прогресса
        //загружаю:
        var getter = ObservableWWW.Get("https://example.com/", null, progress)
            .Subscribe(body =>
            {
                GetComponent<SpriteRenderer>().color = Color.green;  //по завершению меняю цвет

                Debug.Log(body.Split(new[] { "<title>", "</title>" }, StringSplitOptions.None)[1]);  //вывожу заголовок с сайта
            });

        // Если gameObject удален, удаляем оба
        this.OnDestroyAsObservable()  //this.OnDestroyAsObservable () вызывается при уничтожении.  Это уборка.
            .Subscribe(u =>
            {
                watcher.Dispose();
                getter.Dispose();
            });
    }

}