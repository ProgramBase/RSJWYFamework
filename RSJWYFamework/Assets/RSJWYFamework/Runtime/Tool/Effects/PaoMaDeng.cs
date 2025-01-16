using UnityEngine;
using UnityEngine.UI;

namespace RSJWYFamework.Runtime.Tool.Effects
{
    public class PaoMaDeng : MonoBehaviour
    {
        const string angleProName = "_GradientAngle";//材质角度属性的名字
        [SerializeField] float speed = 7.2f;//每秒数值增量为：50 * speed，7.2为一圈每秒
        private Material rawiamgeMAT;
        // Start is called before the first frame update
        private void Awake()
        {
            rawiamgeMAT = GetComponent<RawImage>().material;
        }

        void FixedUpdate()
        {
            var nowValue = rawiamgeMAT.GetFloat(angleProName);
            nowValue += speed;
            if (nowValue > 180)
            {
                nowValue = -180;
            }
            else if (nowValue<-180)
            {
                nowValue = 180;
            }
            rawiamgeMAT.SetFloat(angleProName, nowValue);
        }
    }
}