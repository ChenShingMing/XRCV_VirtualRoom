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

    private void OnEnable()
    {
        teacher.SetActive(false);
        student.SetActive(false);
        monitor.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.InRoom)
        {
            switch (ClassroomManager.ins.joinType)
            {
                case ClassroomManager.JoinType.Member:

                    if (ClassroomManager.ins.GetIdentityInfo() == Player.Identity.Teacher.ToString())
                    {
                        type = InRoomPanel.Type.Teacher;
                    }
                    else
                    {
                        type = InRoomPanel.Type.Student;
                    }

                    break;

                case ClassroomManager.JoinType.Monitor:

                    type = InRoomPanel.Type.Monitor;

                    break;
            }

            PanelHandle();
        }
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
