using System.Collections;
using UnityEngine;

namespace Console
{
    public class WireframeWidget : MonoBehaviour
    {
        void OnPreRender()
        {
            GL.wireframe = true;
        }

        void OnPostRender()
        {
            GL.wireframe = false;
        }
    }
}