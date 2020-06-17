using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    public class LookAtCamera : RichMonoBehaviour
    {
        private Transform target;

        void Start()
        {
            target = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }

        void Update()
        {
            transform.LookAt(new Vector3(target.position.x, target.position.y, target.position.z));
        }
    }

}
