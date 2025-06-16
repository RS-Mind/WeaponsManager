using UnityEngine;

namespace WeaponsManager
{
    public class RightLeftScale : MonoBehaviour
    {
        private void Start()
        {
            this.holdable = base.transform.root.GetComponent<Holdable>();
            this.rightScale = base.transform.localScale;
        }

        private void Update()
        {
            if (!this.holdable || !this.holdable.holder)
            {
                return;
            }
            bool flag = base.transform.root.position.x - 0.1f < this.holdable.holder.transform.position.x;
            Vector3 a = flag ? this.leftScale : this.rightScale;
            base.transform.localScale = a;
        }

        // Token: 0x040009B3 RID: 2483
        public Vector3 leftScale;

        // Token: 0x040009B4 RID: 2484
        private Vector3 rightScale;

        // Token: 0x040009BB RID: 2491
        private Holdable holdable;
    }
}