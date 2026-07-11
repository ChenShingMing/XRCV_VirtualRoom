using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRoomPanel : MonoBehaviour
{
    public enum Type
    {
        Teacher,
        Student,
        Monitor
    }

    public Type type;
    public GameObject teacher;
    public GameObject student;
    public GameObject monitor;

    private bool _typeSet;

    private void OnEnable()
    {
        _typeSet = false;
        teacher.SetActive(false);
        student.SetActive(false);
        monitor.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (_typeSet) return;
        if (!PhotonNetwork.InRoom || Player.localPlayer == null) return;

        switch (ClassroomManager.ins.joinType)
        {
            case ClassroomManager.JoinType.Member:
                type = ClassroomManager.ins.GetIdentityInfo() == Player.Identity.Teacher.ToString()
                    ? InRoomPanel.Type.Teacher
                    : InRoomPanel.Type.Student;
                break;
            case ClassroomManager.JoinType.Monitor:
                type = InRoomPanel.Type.Monitor;
                break;
        }

        PanelHandle();
        _typeSet = true;
    }

    void PanelHandle()
    {
        switch (type)
        {
            case Type.Teacher:

                teacher.SetActive(true);
                student.SetActive(false);
                monitor.SetActive(false);

                break;

            case Type.Student:

                teacher.SetActive(false);
                student.SetActive(true);
                monitor.SetActive(false);

                break;

            case Type.Monitor:

                teacher.SetActive(false);
                student.SetActive(false);
                monitor.SetActive(true);

                break;
        }
    }
}
