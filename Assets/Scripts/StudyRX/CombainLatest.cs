using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;

public class CombainLatest : MonoBehaviour
{
    public InputField inputField1, inputField2;

    void Start()
    {
        //CombineLatest помещает последнее значение каждого потока вместе в массив при отправке любого потока
        Observable.CombineLatest<string>(inputField1.OnValueChangedAsObservable(), inputField2.OnValueChangedAsObservable())
            .Subscribe(inlist =>
            {
                try
                {
                    GetComponent<Text>().text = inlist.Sum(s => double.Parse(s)).ToString();
                }
                catch { }
            });
    }

}