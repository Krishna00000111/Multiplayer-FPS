using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Kawaiisun.SimpleHostile
{
    public class Look : MonoBehaviourPunCallbacks
    {
        #region Variables

        public static bool cursorLocked;

        public Transform player;
        public Transform normalCam;
        public Transform weaponCam;
        public Transform weapon;

        private SensitivitySlider senseSlider;
        public float xSensitivity;
        public float ySensitivity;
        public float maxAngle;

        private Quaternion camCentre;

        #endregion Variables

        #region MonoBehaviour CallBacks

        private void Start()
        {
            senseSlider = FindObjectOfType<SensitivitySlider>();
            camCentre = normalCam.localRotation; // set rotation origin to cams to camCentre
        }

        private void Update()
        {
            if (!photonView.IsMine) return;
            

            SetY();
            SetX();
            UpdateCursorLock();

            weaponCam.rotation = normalCam.rotation;
        }

        #endregion MonoBehaviour CallBacks

        #region Private Methods

        private void SetY()
        {
            float t_input = Input.GetAxis("Mouse Y") * senseSlider.sensitivity * ySensitivity* Time.deltaTime;
            Quaternion t_adjust = Quaternion.AngleAxis(t_input, -Vector3.right);
            Quaternion t_delta = normalCam.localRotation * t_adjust;

            if (Quaternion.Angle(camCentre, t_delta) < maxAngle)
            {
                normalCam.localRotation = t_delta;
            }

            weapon.rotation = normalCam.rotation;
        }

        private void SetX()
        {
            float t_input = Input.GetAxis("Mouse X") * senseSlider.sensitivity * xSensitivity * Time.deltaTime;
            Quaternion t_adjust = Quaternion.AngleAxis(t_input, Vector3.up);
            Quaternion t_delta = player.localRotation * t_adjust;
            player.localRotation = t_delta;
        }

        private void UpdateCursorLock()
        {
            if (cursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cursorLocked = false;
                }
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cursorLocked = true;
                }
            }
        }

        #endregion Private Methods
    }
}