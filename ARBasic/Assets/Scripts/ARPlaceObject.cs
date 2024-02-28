using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlaceObject : MonoBehaviour
{
    public GameObject pinPrefab;
    public GameObject linePrefab;
    public GameObject placementIndicator;
    public ARRaycastManager raycastManager;
    private Pose placementPose;
    public Camera arCamera;
    public List<ARRaycastHit> touchHits = new List<ARRaycastHit>();

    private GameObject currentLine;
    private Vector3 currentLineFixedPose;
    private List<GameObject> placedPins = new List<GameObject>();
    private List<GameObject> placedLines = new List<GameObject>();

    void Update()
    {
        UpdateMidPointer();
        SetPin();
        ShowLine();
    }

    void UpdateMidPointer()
    {
        var screenCenter = arCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        raycastManager.Raycast(screenCenter, hits, TrackableType.Planes);
        if (hits.Count > 0)
        {
            placementPose = hits[0].pose;
            var cameraForward = Camera.main.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    void SetPin()
    {
        // �� ����
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
            FixPin();
        // ���� ��� Ŀ�ǵ�
        else if (Input.touchCount == 2 && placedPins.Count > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            RemovePin();
    }

    void ShowLine()
    {
        if (currentLine == null)
            return;

        Vector3 direction = (placementPose.position - currentLineFixedPose).normalized;

        // ������Ʈ�� ȸ���� ���� ���Ϳ� �°� ����
        currentLine.transform.rotation = Quaternion.LookRotation(direction);

        // �Ÿ��� ���� ������ ���� (���÷�, ���⼭�� Z�� �������� �����մϴ�)
        float distance = Vector3.Distance(currentLineFixedPose, placementPose.position);
        currentLine.transform.localScale = new Vector3(currentLine.transform.localScale.x, currentLine.transform.localScale.y, distance);

        // ������Ʈ�� ��ġ�� ���� (�������� ���콺 ��ġ�� �߰������� ����)
        currentLine.transform.position = currentLineFixedPose + direction * distance / 2;
    }

    /**
     * �� ����
     */
    void FixPin()
    {
        placedPins.Add(Instantiate(pinPrefab, placementPose.position, Quaternion.identity));
        if (currentLine != null)
            placedLines.Add(currentLine);

        currentLine = Instantiate(linePrefab, placementPose.position, placementPose.rotation);
        currentLineFixedPose = placedPins[^1].transform.position;
    }

    void RemovePin()
    {
        Destroy(placedPins[^1]);
        placedPins.Remove(placedPins[^1]);
        if (placedLines.Count > 0)
        {
            Destroy(placedLines[^1]);
            placedLines.Remove(placedLines[^1]);
            if (placedPins.Count > 0) 
                currentLineFixedPose = placedPins[^1].transform.position;
        }
    }
}
