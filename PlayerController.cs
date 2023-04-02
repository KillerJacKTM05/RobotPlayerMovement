using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SafeZone;

public class PlayerController : MonoBehaviour
{


    private Transform lastInteracted;
    private Vector3 mouseStartOffset;
    private Vector3 mouseOffset;
    private Vector3 newNeckRotation = Vector3.zero;
    private bool isNeckRotate = false;
    private bool isPlayerStuck = false;
    private bool urgency = false;
    private UnityAction movementAction;
    private UnityAction rayAction;
    private Vector3 beforePos = Vector3.zero;
    private Vector3 currentPos = Vector3.zero;
    private Color arrowNormColor = new Color(0.095f, 1, 0);
    private Color arrowUrgColor = new Color(1, 0, 0.1f);
    private Vector3 arrowInitialScale = new Vector3(0, 0, 0);

    [SerializeField] private Animator robotAnimator;
    [SerializeField] private Transform neck;
    [SerializeField] private Transform arrow;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.SetPlayer(this);
        arrowInitialScale = arrow.GetChild(0).localScale;
        movementAction += OrganizeKeyInputs;
        rayAction += ThrowRayForInteraction;
        rayAction += ThrowRayForCameraAlignment;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GetGameStarted())
        {
            if (Input.GetMouseButton(1) && CameraManager.Instance.GetStatusBool() && !CameraManager.Instance.GetCamSwitchRoutineActive() && !CameraManager.Instance.GetInSmallSpace())
            {
                isNeckRotate = true;
                robotAnimator.SetBool("isWalking", false);
                CameraManager.Instance.GetGameCamera().transform.SetParent(neck.transform);
            }
            else if(CameraManager.Instance.GetStatusBool() && !CameraManager.Instance.GetCamSwitchRoutineActive() && !CameraManager.Instance.GetInSmallSpace())
            {
                isNeckRotate = false;
                CameraManager.Instance.GetGameCamera().transform.SetParent(GameManager.Instance.GetPlayer().transform);
                mouseStartOffset = Input.mousePosition;
                neck.Rotate(new Vector3(neck.rotation.x, 0, neck.rotation.z));
                CameraManager.Instance.ResetCameraAngle();
            }

            if (!isNeckRotate)
            {
                movementAction();
                rayAction();
            }
            else
            {
                mouseOffset = Input.mousePosition - mouseStartOffset;
                float rotationY = mouseOffset.x * GameManager.Instance.GetGameSettings().playerLookSensivity;
                float rotationX = mouseOffset.y * GameManager.Instance.GetGameSettings().playerLookSensivity;
                rotationX = Mathf.Clamp(rotationX, -20, 10);
                rotationY = Mathf.Clamp(rotationY, -40, 40);
                newNeckRotation = new Vector3(rotationX, rotationY, 0);
            }

            ArrowPoint();
            CheckRotation();
        }
    }
    private void LateUpdate()
    {
        if(isNeckRotate)
        TurnNeck();
    }
    private void ArrowPoint()
    {
        if(InterfaceManager.Instance.objectivePanelUI.GetRelatedTaskFromList(0) != null)
        {
            arrow.GetComponentInChildren<MeshRenderer>().enabled = true;
            arrow.LookAt(InterfaceManager.Instance.objectivePanelUI.GetRelatedTaskFromList(0).transform, transform.up);
            if (InterfaceManager.Instance.objectivePanelUI.GetRelatedTaskFromList(0).urgency && !urgency)
            {
                urgency = true;
                arrow.GetComponentInChildren<Renderer>().material.SetColor("_Color", arrowUrgColor);
                StartCoroutine(ArrowScaleUpDown());
            }
            else if(!InterfaceManager.Instance.objectivePanelUI.GetRelatedTaskFromList(0).urgency && urgency)
            {
                urgency = false;
                arrow.GetChild(0).localScale = arrowInitialScale;
                arrow.GetComponentInChildren<Renderer>().material.SetColor("_Color", arrowNormColor);
            }
        }
        else
        {
            arrow.GetComponentInChildren<MeshRenderer>().enabled = false;
        }
    }
    private void CheckRotation()
    {
        float angleX = transform.rotation.eulerAngles.x;
        float angleZ = transform.rotation.eulerAngles.z;
        if (angleX > 180f)
        {
            angleX = angleX - 360;
        }
        if(angleZ > 180f)
        {
            angleZ = angleZ - 360;
        }
        if ((angleX > -100f && angleX < -80f) || (angleX > 80 && angleX < 100))
        {
            if (!isPlayerStuck)
            {
                StartCoroutine(InterfaceManager.Instance.PopUp(GameManager.Instance.GetGameSettings().unstuckCloud, InterfaceManager.Instance.m_inGameGroup.transform, new Vector3(0, 0, 0), 0, 2f));
            }
            isPlayerStuck = true;
        }
        else if((angleX > -20 && angleX < 20) && (angleZ > -100f && angleZ < -80f) || (angleZ > 80 && angleZ < 100))
        {
            if (!isPlayerStuck)
            {
                StartCoroutine(InterfaceManager.Instance.PopUp(GameManager.Instance.GetGameSettings().unstuckCloud, InterfaceManager.Instance.m_inGameGroup.transform, new Vector3(0, 0, 0), 0, 2f));
            }
            isPlayerStuck = true;
        }
    }
    private void TurnNeck()
    {
        neck.localRotation = Quaternion.Euler(newNeckRotation.x,-newNeckRotation.y,neck.rotation.z);
    }
    public Vector3 GetRotationValues()
    {
        var Vector = new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        return Vector;
    }
    public float CalculateAndGetVelocity()
    {
        float diff = (currentPos - beforePos).magnitude;
        //diff = diff / GameManager.Instance.GetGameSettings().realWorldTimeForEachGameHour;
        return diff;
    }
    public void SetBeforePose(Vector3 bef)
    {
        beforePos = bef;
    }
    public void SetCurrentPose(Vector3 cur)
    {
        currentPos = cur;
    }
    public Vector3 GetBeforePos()
    {
        return beforePos;
    }
    public Vector3 GetCurrentPos()
    {
        return currentPos;
    }
    public void OrganizeKeyInputs()
    {
        if (Input.anyKey)
        {
            //Forward backward movement cannot be  at the same time.
            if (Input.GetKey(KeyCode.W))
            {
                MoveIt(KeyCode.W);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                MoveIt(KeyCode.S);
            }

            //Horizontal is also cannot be.
            if (Input.GetKey(KeyCode.D))
            {
                MoveIt(KeyCode.D);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                MoveIt(KeyCode.A);
            }

            if (Input.GetKey(KeyCode.E))
            {
                if(lastInteracted != null)
                {
                    lastInteracted.SendMessage("SingleWingMovement", "right");
                }
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                InterfaceManager.Instance.SetObjective(!InterfaceManager.Instance.GetObjective());
                InterfaceManager.Instance.OpenCloseTab(InterfaceManager.Instance.m_objective, InterfaceManager.Instance.GetObjective());
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                InterfaceManager.Instance.SetArrowState(!InterfaceManager.Instance.GetArrowState());
                InterfaceManager.Instance.OpenCloseTab(arrow, InterfaceManager.Instance.GetArrowState());
            }
            //toggle open-close map with this key
            else if (Input.GetKeyDown(KeyCode.M))
            {
                InterfaceManager.Instance.SetMapState(!InterfaceManager.Instance.GetMapState());
                InterfaceManager.Instance.OpenCloseTab(InterfaceManager.Instance.m_mapGroup.transform, InterfaceManager.Instance.GetMapState());
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                if (isPlayerStuck)
                {
                    UnstuckMe();
                }
            }
            //this two cases for ingame debugging and data collecting. In normal, player data will be inserted at the end of the game.
            else if (Input.GetKeyDown(KeyCode.I))
            {
                DataManager.Instance.WriteToFile();
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                DataManager.Instance.DeletePlayerFile();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if(Time.timeScale >= 0.1f)
                {
                    InterfaceManager.Instance.StopGame(0);
                    InterfaceManager.Instance.SetRoutineBlocker(true);
                }
            }
        }      
        else
        {
            robotAnimator.SetBool("isWalking", false);
        }
    }
    private void MoveIt(KeyCode keyCode)
    {
        robotAnimator.SetBool("isWalking", true);
        switch (keyCode)
        {
            case KeyCode.W:
                transform.position = transform.position + (transform.forward * Time.deltaTime * GameManager.Instance.GetGameSettings().playerMoveSpeed);
                break;
            case KeyCode.S:
                transform.position = transform.position - (transform.forward * Time.deltaTime * GameManager.Instance.GetGameSettings().playerMoveSpeed / 2f);
                break;
            case KeyCode.D:
                RotateAngle(KeyCode.D);
                break;
            case KeyCode.A:
                RotateAngle(KeyCode.A);
                break;
        }
    }
    private void RotateAngle(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.D:
                Vector3 newRotationD = new Vector3(0,(GameManager.Instance.GetGameSettings().playerRotateSensivity), 0);
                transform.Rotate(newRotationD);
                break;
            case KeyCode.A:
                Vector3 newRotationA = new Vector3(0,-(GameManager.Instance.GetGameSettings().playerRotateSensivity), 0);
                transform.Rotate(newRotationA);
                break;
        }

    }
    private void UnstuckMe()
    {
        if (isPlayerStuck)
        {
            transform.SetPositionAndRotation(new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity);
            isPlayerStuck = false;
        }
    }
    private void ThrowRayForInteraction()
    {
        int layer = 6;
        RaycastHit rayhit;
        if(Physics.Raycast(CameraManager.Instance.GetGameCamera().transform.position, CameraManager.Instance.GetGameCamera().transform.forward, out rayhit, 4f, 1 << layer))
        {
            if (rayhit.collider.CompareTag("Interactable"))
            {
                if(lastInteracted == null)
                {
                    InterfaceManager.Instance.SwitchCanvasGroups(InterfaceManager.Instance.m_inGameGroup, InterfaceManager.Instance.m_interactionGroup, 1);
                }
                lastInteracted = rayhit.transform;
            }
        }
        else
        {
            if(lastInteracted != null)
            {
                InterfaceManager.Instance.SwitchCanvasGroups(InterfaceManager.Instance.m_interactionGroup, InterfaceManager.Instance.m_inGameGroup, 1);
                lastInteracted = null;
            }
        }
    }
    private void ThrowRayForCameraAlignment()
    {
        int layer1 = 0;
        int layer2 = 7;
        RaycastHit hit;
        if(Physics.Raycast(neck.transform.position, -neck.transform.forward, out hit, 1.5f, 1 << layer1) || Physics.Raycast(neck.transform.position, -neck.transform.forward, out hit, 1.5f, 1 << layer2))
        {
            if (hit.collider.CompareTag("walls"))
            {
                if(CameraManager.Instance.GetStatusBool() == true && !CameraManager.Instance.GetCamSwitchRoutineActive() && !CameraManager.Instance.GetInSmallSpace())
                {
                    CameraManager.Instance.SetStatusBool(false);
                    CameraManager.Instance.SwitchCameraAngle(1, 3, 0.75f);
                }
            }
        }
        else if (CameraManager.Instance.GetStatusBool() == false && !CameraManager.Instance.GetCamSwitchRoutineActive() && !CameraManager.Instance.GetInSmallSpace())
        {
            CameraManager.Instance.SwitchCameraAngle(3, 1, 0.75f);
            CameraManager.Instance.SetStatusBool(true);
        }
    }
    private IEnumerator ArrowScaleUpDown(float duration = 1f)
    {
        Vector3 upScale = arrowInitialScale * 2; 
        float time = 0f;
        float progress = 0f;
        int counter = 0;
        while (urgency)
        {
            progress = Mathf.PingPong(time, duration) / duration;
            if(counter % 2 == 0)
            {
                arrow.GetChild(0).localScale = Vector3.Lerp(arrowInitialScale, upScale, progress);
            }
            else
            {
                arrow.GetChild(0).localScale = Vector3.Lerp(upScale, arrowInitialScale, progress);
            }
            time += 0.1f;
            if(time >= 1)
            {
                time = time % 1;
                counter += 1;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
