using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProgressSceneLoader : MonoBehaviour
{
    [SerializeField]
    private Text progressText;
    [SerializeField]
    private Text progressInfoText;
    [SerializeField]
	private Slider slider;
    [SerializeField]
    public GameObject camera;

    private Canvas canvas;
    
    public const string START_LOADING = "Generierung startet ...";
    public const string LOAD_RND = "Zufallszahlen werden berechnet ...";
    public const string LOAD_TERRAIN = "Das Terrain wird initialisiert ...";
    public const string LOAD_BIOMES = "Biome werden generiert ...";
    public const string LOAD_TEXTURES = "Texturen werden verteilt ...";
    public const string LOAD_BORDERS = "Grenzen werden berechnet ...";
    public const string LOAD_MOUNTAINS = "Berge werden generiert ...";
    public const string LOAD_HOUSES = "Häuser werden verteilt ...";
    public const string LOAD_GAMEOBJECTS = "Weitere Spielobjekte werden verteilt ...";
    public const string LOAD_PLAYER = "Der Spieler wird erstellt ...";

    public void UpdateBar(string code)
    {

        Debug.Log(code + " " + DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

        float progressValue = 0;
        switch (code)
        {
            case START_LOADING:
                progressValue = 0.0f;
                break;
            case LOAD_RND:
                progressValue = 0.016f;
                break;
            case LOAD_TERRAIN:
                progressValue = 0.081f;
                break;
            case LOAD_BIOMES:
                progressValue = 0.167f;
                break;
            case LOAD_TEXTURES:
                progressValue = 0.224f;
                break;
            case LOAD_BORDERS:
                progressValue = 0.382f;
                break;
            case LOAD_MOUNTAINS:
                progressValue = 0.569f;
                break;
            case LOAD_HOUSES:
                progressValue = 0.701f;
                break;
            case LOAD_GAMEOBJECTS:
                progressValue = 0.843f;
                break;
            case LOAD_PLAYER:
                progressValue = 0.990f;
                break;
            default:
                progressValue = 1.0f;
                break;
        }
        UpdateProgressUI(progressValue,code);
    }


	private void UpdateProgressUI(float progress, string code)
	{
	    progressInfoText.text = code;
        slider.value = progress;
		progressText.text = (int)(progress * 100f) + "%";
	}
}