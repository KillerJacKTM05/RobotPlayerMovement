using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SafeZone
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [SerializeField]private List<Vector3> cameraAnglesOffsets = new List<Vector3>(2);
        [SerializeField] private Camera gameCamera;
        private bool isIdleState = true;
        private bool isInSmallSpace = false;
        private bool isSwitchRoutineActive = false;

        private Coroutine switchRoutine;
        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }

        }

        public Camera GetGameCamera()
        {
            return gameCamera;
        }
        public List<Vector3> GetCamPosOffsets()
        {
            return cameraAnglesOffsets;
        }
        public void SetStatusBool(bool state)
        {
            isIdleState = state;
        }
        public bool GetStatusBool()
        {
            return isIdleState;
        }
        public void SetSmallSpace(bool state)
        {
            isInSmallSpace = state;
        }
        public bool GetInSmallSpace()
        {
            return isInSmallSpace;
        }
        public bool GetCamSwitchRoutineActive()
        {
            return isSwitchRoutineActive;
        }
        public void ResetCameraAngle()
        {
            gameCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);
            gameCamera.transform.localPosition = cameraAnglesOffsets[0];
        }
        public void SwitchCameraAngle(int from, int to, float duration = 1)
        {
            if(from > cameraAnglesOffsets.Count || to > cameraAnglesOffsets.Count)
            {
                Debug.LogError("Chosen camera angle couldn't found. Check the list from manager.");
            }
            else
            {
                isSwitchRoutineActive = true;
                if(duration != 1)
                {
                    switchRoutine = StartCoroutine(SwitchAngleRoutine(cameraAnglesOffsets[from - 1], cameraAnglesOffsets[to - 1], duration));
                }
                else
                {
                    switchRoutine = StartCoroutine(SwitchAngleRoutine(cameraAnglesOffsets[from - 1], cameraAnglesOffsets[to - 1], GameManager.Instance.GetGameSettings().cameraSwitchingDuration));
                }              
            }
        }

        private IEnumerator SwitchAngleRoutine(Vector3 from, Vector3 to, float duration)
        {
            float fl = 0;
            float timer = (1 * duration) / 60f;
            while(fl < 1f)
            {
                GetGameCamera().transform.localPosition = Vector3.Lerp(from, to, fl);
                fl += timer;
                yield return new WaitForSeconds(timer);
            }
            isSwitchRoutineActive = false;
        }
    }
}

