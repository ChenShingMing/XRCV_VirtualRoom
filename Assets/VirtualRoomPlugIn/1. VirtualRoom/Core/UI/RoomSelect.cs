using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomSelect : MonoBehaviour
{
    public Dropdown dropdown;

    public void SetRoomName(int index)
    {
        PunNetworkManager.ins.SetRoomName(dropdown.options[index].text);
    }

    public void ClearRoomName()
    {
        PunNetworkManager.ins.SetRoomName(string.Empty);
    }

    public void ReflashServerList()
    {
        foreach(KeyValuePair<string, RoomInfo> item in PunNetworkManager.ins.cachedRoomList)
        {
            bool needToAdd = true;

            for (int i = 0; i < dropdown.options.Count; i++)
            {
                if (dropdown.options[i].text == item.Value.Name)
                {
                    needToAdd = false;
                    break;
                }
            }

            if (needToAdd)
            {
                dropdown.options.Add(new Dropdown.OptionData(item.Value.Name));
            }
        }

        for(int i = 0; i < dropdown.options.Count; i++)
        {
            if (PunNetworkManager.ins.cachedRoomList.ContainsKey(dropdown.options[i].text) == false)
            {
                dropdown.options.RemoveAt(i);
                i--;
            }
        }

        dropdown.RefreshShownValue();

        if (dropdown.options.Count != 0)
        {
            if (dropdown.onValueChanged != null)
            {
                dropdown.onValueChanged.Invoke(dropdown.value);
            }
        }
        else
        {
            ClearRoomName();
        }
    }
}
