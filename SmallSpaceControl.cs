using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SafeZone;

public class SmallSpaceControl : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && CameraManager.Instance.GetInSmallSpace() == false)
        {
            CameraManager.Instance.SwitchCameraAngle(1, 2);
            CameraManager.Instance.SetSmallSpace(true);
        }

    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && CameraManager.Instance.GetInSmallSpace() == true)
        {
            CameraManager.Instance.SwitchCameraAngle(2, 1);
            CameraManager.Instance.SetSmallSpace(false);
        }
    }
}
