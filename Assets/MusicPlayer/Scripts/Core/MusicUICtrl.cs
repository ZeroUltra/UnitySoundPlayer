using UnityEngine;
using UnityEngine.UI;
using Un4seen.Bass;
using Music.Core;
public class MusicUICtrl : MonoBehaviour
{
    [Header("Button")]
    public Button btnPlay;
    public Button btnPause;
    public Button btnStop;

    [Header("Slider")]
    public Slider progressSlider;
    public Slider volumeSlider;

    [Header("Text")]
    public Text totalTxt;
    public Text updateTxt;
    public Text titleTxt;

    private bool sliderIsDrag = false;
    private float intervetTime = 0.2f;
    private float timer = 0f;

    private MusicPlayer musicPlayer;
    void Start()
    {
        musicPlayer = new MusicPlayer();

        #region Slider
        progressSlider.value = 0f;
        EventTriggerListener.SetEventTrigger(progressSlider.gameObject).OnMouseDown.AddListener((data) => sliderIsDrag = true);
        EventTriggerListener.SetEventTrigger(progressSlider.gameObject).OnMouseUp.AddListener((data) =>
        {
            musicPlayer.Seek(progressSlider.value);
            sliderIsDrag = false;
        });

        //--volume 
        //一开始就设置音量调
        volumeSlider.maxValue = 1f;
        volumeSlider.value = musicPlayer.GetVolume();
        //需要一直监听
        volumeSlider.onValueChanged.AddListener((value) =>
        {
            musicPlayer.SetVolume(value);
        });


        #endregion

        btnPlay.onClick.AddListener(() =>
        {
            musicPlayer.Play();
            btnPlay.gameObject.SetActive(false);
            btnPause.gameObject.SetActive(true);
        });
        btnPause.onClick.AddListener(() =>
        {
            btnPlay.gameObject.SetActive(true);
            btnPause.gameObject.SetActive(false);
            musicPlayer.Pause();
        });

        if (btnStop != null)
            btnStop.onClick.AddListener(musicPlayer.Stop);

    }

    public void LoadMusic(string filename)
    {
        musicPlayer.LoadMusic(filename);
        titleTxt.text = musicPlayer.GetMusicName();
        totalTxt.text = Utils.FixTimespan(musicPlayer.GetMusicTotleTime(), "MMSS");
        progressSlider.maxValue = (float)musicPlayer.GetMusicTotleTime();
    }
    #region MonoBehaviour
    private void Update()
    {
        if (musicPlayer.GetInitMusic() == false) return;
        timer += Time.deltaTime;
        if (timer >= intervetTime)
        {
            timer = 0;
            if (!sliderIsDrag)
            {
                float currentTime = (float)musicPlayer.GetMusicCurrentTime();
                updateTxt.text = Utils.FixTimespan(musicPlayer.GetMusicCurrentTime(), "MMSS");
                progressSlider.value = (float)currentTime;
            }
        }
    }
    private void OnApplicationQuit()
    {
        musicPlayer.Close();
    }
    #endregion
}
