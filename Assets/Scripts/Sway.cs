using UnityEngine;

namespace Com.Kawaiisun.SimpleHostile
{
    public class Sway : MonoBehaviour
    {
        #region Variable

        public float intensity;
        public float smooth;
        public bool isOurs;
        private Quaternion origin_Rotation;

        #endregion Variable

        #region MonoBehaviour

        private void Start()
        {
            origin_Rotation = transform.localRotation;
        }

        private void Update()
        {
            
            UpdateSway();
        }

        #endregion MonoBehaviour

        #region Private Methods

        private void UpdateSway()
        {
            //controls
            float t_x_Mouse = Input.GetAxis("Mouse X");
            float t_y_Mouse = Input.GetAxis("Mouse Y");

            if (!isOurs)
            {
                t_x_Mouse = 0;
                t_y_Mouse = 0;
            }

            //Calculate target rotation
            Quaternion t_x_adj = Quaternion.AngleAxis(-intensity * t_x_Mouse, Vector3.up);
            Quaternion t_y_adj = Quaternion.AngleAxis(intensity * t_y_Mouse, Vector3.right);
            Quaternion target_Rotation = origin_Rotation * t_x_adj * t_y_adj;

            //rotate towards target or original position
            transform.localRotation = Quaternion.Lerp(transform.localRotation, target_Rotation, Time.deltaTime * smooth);
        }

        #endregion Private Methods
    }
}