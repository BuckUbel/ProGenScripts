using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextSceneLoader : MonoBehaviour
{

    [SerializeField] private GameObject startObject;
    [SerializeField] private GameObject startScene;

    // Use this for initialization
    private void Awake () {
		GetComponent<Button>().onClick.AddListener(LoadScene);
	}
	
	// Update is called once per frame
	void LoadScene() {
	    startScene.gameObject.SetActive(false);
	    startObject.gameObject.GetComponent<Terrainer>().activate();
	}
}
