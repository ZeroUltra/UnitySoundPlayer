using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestPlay : MonoBehaviour
{
    public MusicPlayer musicPlayer;
    public string fileName;
    void Start()
    {
        fileName = Application.streamingAssetsPath + "/" + fileName;
        GetComponent<Button>().onClick.AddListener(()=>
        {
            musicPlayer.LoadMusic(fileName);
        });
    }

    
}
