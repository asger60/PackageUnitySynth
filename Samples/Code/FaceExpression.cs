using UnityEngine;

namespace UnitySynth.Samples.Code
{
    public class FaceExpression : MonoBehaviour
    {
        public Animator animator;
        private Quaternion _rotationTarget;
        public void SetExpression(int expressionIndex)
        {
            for (int i = 0; i < 5; i++)
            {
                animator.ResetTrigger(i.ToString());
            }

            animator.SetTrigger(expressionIndex.ToString());
        }

        public void SetNote(int note)
        {
            float t = Mathf.InverseLerp(50, 70, note);
            _rotationTarget = Quaternion.Lerp(Quaternion.Euler(-6f, 0, 0), Quaternion.Euler(22f, 0, 0), t);
        }

        private void Update()
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, _rotationTarget, Time.deltaTime * 10);

        }
    }
}