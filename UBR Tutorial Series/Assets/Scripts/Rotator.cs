using UnityEngine;

namespace PolygonPilgrimage.BattleRoyaleKit
{
    public class Rotator : RichMonoBehaviour
    {
        [SerializeField] private Vector3 rotateSpeed;
        
        // Update is called once per frame
        private void Update()
        {
            transform.Rotate(rotateSpeed * Time.deltaTime);
        }
    }

}
