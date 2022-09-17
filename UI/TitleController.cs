using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleController : MonoBehaviour {
    public GameObject mainMenu;
    public GameObject VRDesignMenu;
    void Start() {
        mainMenu.SetActive(true);
        VRDesignMenu.SetActive(false);
    }
    public void NewVRMissionCallback() {
        mainMenu.SetActive(false);
        VRDesignMenu.SetActive(true);
    }
    public void CancelVRMissionCallback() {
        mainMenu.SetActive(true);
        VRDesignMenu.SetActive(false);
    }
}
