using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSystem : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    // Movement
    [SerializeField] private bool edgeScrolling = true;
    [SerializeField] private bool dragAndMove = true;
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float dragAndMoveSpeed = 2f;
    private bool _dragAndMoveActive;
    private Vector2 _lastMousePos;

    // Rotation
    [SerializeField] private float rotateSpeed = 100f;

    // Zoom
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float zoomSensitivity = 3f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 30f;
    private Vector3 _followOffset;

    private void Awake()
    {
        _followOffset = virtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
    }

    // Update is called once per frame
    private void Update()
    {
        HandleCameraMovement();

        if (edgeScrolling)
        {
            HandleCameraEdgeScrolling();
        }

        if (dragAndMove)
        {
            HandleCameraDragAndMove();
        }

        HandleCameraRotation();
        HandleCameraZoom();
    }

    private void HandleCameraMovement()
    {
        var inputDir = new Vector3();
        if (Keyboard.current.wKey.isPressed)
        {
            inputDir.z = 1;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            inputDir.z = -1;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            inputDir.x = -1;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            inputDir.x = 1;
        }

        var moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;
        transform.position += moveDir * (moveSpeed * Time.deltaTime);
    }

    private void HandleCameraEdgeScrolling()
    {
        var inputDir = new Vector3();
        const int edgeScrollSize = 20;
        var mousePos = Mouse.current.position.ReadValue();

        if (mousePos.x < edgeScrollSize)
        {
            inputDir.x = -1;
        }

        if (mousePos.y < edgeScrollSize)
        {
            inputDir.z = -1;
        }

        if (mousePos.x > Screen.width - edgeScrollSize)
        {
            inputDir.x = 1;
        }

        if (mousePos.y > Screen.height - edgeScrollSize)
        {
            inputDir.z = 1;
        }

        var moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;
        transform.position += moveDir * (moveSpeed * Time.deltaTime);
    }

    private void HandleCameraDragAndMove()
    {
        var inputDir = new Vector3();
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            _dragAndMoveActive = true;
            _lastMousePos = Mouse.current.position.ReadValue();
        }
        else if (_dragAndMoveActive && Mouse.current.rightButton.wasReleasedThisFrame)
        {
            _dragAndMoveActive = false;
        }

        if (_dragAndMoveActive)
        {
            var mouseMovementDelta = Mouse.current.position.ReadValue() - _lastMousePos;

            inputDir.x = mouseMovementDelta.x * dragAndMoveSpeed;
            inputDir.z = mouseMovementDelta.y * dragAndMoveSpeed;

            _lastMousePos = Mouse.current.position.ReadValue();
        }

        var moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;
        transform.position += moveDir * (moveSpeed * Time.deltaTime);
    }

    private void HandleCameraRotation()
    {
        var rotateDir = 0f;
        if (Keyboard.current.qKey.isPressed)
        {
            rotateDir = -1;
        }

        if (Keyboard.current.eKey.isPressed)
        {
            rotateDir = 1;
        }

        transform.eulerAngles += new Vector3(0, rotateDir * rotateSpeed * Time.deltaTime, 0);
    }

    private void HandleCameraZoom()
    {
        var zoomDir = _followOffset.normalized;
        var scrollDeltaY = Mouse.current.scroll.ReadValue().y;
        switch (scrollDeltaY)
        {
            case > 0:
                _followOffset -= zoomDir * zoomSensitivity;
                break;
            case < 0:
                _followOffset += zoomDir * zoomSensitivity;
                break;
        }

        if (_followOffset.magnitude < minZoom)
        {
            _followOffset = zoomDir * minZoom;
        }
        else if (_followOffset.magnitude > maxZoom)
        {
            _followOffset = zoomDir * maxZoom;
        }

        virtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset =
            Vector3.Lerp(virtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset, _followOffset,
                Time.deltaTime * zoomSpeed);
    }
}