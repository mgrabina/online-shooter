﻿using System;
 using System.Collections;
using System.Collections.Generic;
 using System.Linq;
 using custom;
 using UnityEngine;
 using UnityEngine.UI;

 public class menu : MonoBehaviour
 {
     public InputField InputField;
     public Button clientBtn;
     public Button serverBtn;
     public GameObject shadowClientGO;
     public GameObject shadowServerGO;
     
     void Start()
    {
        clientBtn.onClick.AddListener(setClient);
        serverBtn.onClick.AddListener(setServer);
    }

    void Update()
    {
    }

    private void setClient()
    {
        string address = "127.0.0.1";
        if (InputField.text != null && ValidateIPv4(InputField.text))
        {
            address = InputField.text;
        }
        MasterBehavior.MasterData.setClient(address, shadowClientGO);
    }

    private void setServer()
    {
        MasterBehavior.MasterData.setServer(shadowServerGO);
    }

    public bool ValidateIPv4(string ipString)
    {
        if (String.IsNullOrWhiteSpace(ipString))
        {
            return false;
        }

        string[] splitValues = ipString.Split('.');
        if (splitValues.Length != 4)
        {
            return false;
        }

        byte tempForParsing;

        return splitValues.All(r => byte.TryParse(r, out tempForParsing));
    }
}
