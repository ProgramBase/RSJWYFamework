using RSJWYFamework.Runtime.Scene;
using UnityEngine;
namespace RSJWYFamework.Runtime.Main
{
    public partial class Main
    {
        void Init()
        {
            var _sceneSwitch= Instantiate(Resources.Load<GameObject>("SceneSwitch"),canvas.transform, false);
            sceneTransition=_sceneSwitch.transform.GetComponent<SceneTransition>();
            sceneTransition.UpdateProgress(0,"");
        }
    }
}