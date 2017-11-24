using UnityEngine;

namespace DefaultNamespace
{
    public class MouseHandler
    {
        public static bool clampVerticalRotation = true;
        public static float MinimumX = -65f;
        public static float MaximumX = 65f;
        public static float MinimumXGun = MinimumX + 90f;
        public static float MaximumXGun = MaximumX + 90f;

        public static float XSensitivity = 2f;
        public static float YSensitivity = 2f;

        private Quaternion _characterTargetRot;
        private Quaternion _cameraTargetRot;
        private Quaternion _gunTargetRot;
        private bool _cursorIsLocked = true;

        private float _xRot;
        private float _yRot;

        public void Init(Transform character, Transform camera, Transform gun)
        {
            _characterTargetRot = character.localRotation;
            _cameraTargetRot = camera.localRotation;
            _gunTargetRot = gun.localRotation;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void LookRotation(Transform character, Transform camera, Transform gun)
        {
            _xRot = Input.GetAxis("Mouse X") * XSensitivity;
            _yRot = Input.GetAxis("Mouse Y") * YSensitivity;

            _characterTargetRot *= Quaternion.Euler(0f, _xRot, 0f);
            _cameraTargetRot *= Quaternion.Euler(-_yRot, 0f, 0f);
            _gunTargetRot *= Quaternion.Euler(-_yRot, 0f, 0f);

            if (clampVerticalRotation)
            {
                _cameraTargetRot = ClampRotationAroundXAxis(_cameraTargetRot, MinimumX, MaximumX);
                _gunTargetRot = ClampRotationAroundXAxis(_gunTargetRot, MinimumXGun, MaximumXGun);
            }

            character.localRotation = _characterTargetRot;
            camera.localRotation = _cameraTargetRot;
            gun.localRotation = _gunTargetRot;

            UpdateCursorLockState();
        }

        public void UpdateCursorLockState()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                _cursorIsLocked = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if(Input.GetMouseButtonUp(0))
            {
                _cursorIsLocked = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q, float minimum, float maximum)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, minimum, maximum);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}