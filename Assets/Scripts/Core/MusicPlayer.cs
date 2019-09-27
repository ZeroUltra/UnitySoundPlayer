using UnityEngine;
using UnityEngine.UI;
using Un4seen.Bass;
using System;
using ExpansionMusic;
public class MusicPlayer : MonoBehaviour
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


    private bool isInit = false;
    private int currentMusicHandle;
    private bool sliderIsDrag = false;
    private float intervetTime = 0.2f;
    private float timer = 0f;

    void Awake()
    {
        InitXMPlayer();

        #region Slider
        progressSlider.value = 0f;
        EventTriggerListener.SetEventTrigger(progressSlider.gameObject).OnMouseDown.AddListener((data) => sliderIsDrag = true);
        EventTriggerListener.SetEventTrigger(progressSlider.gameObject).OnMouseUp.AddListener((data) =>
        {
            Seek(progressSlider.value);
            sliderIsDrag = false;
        });

        //--volume 
        //一开始就设置音量调
        volumeSlider.maxValue = 1f;
        volumeSlider.value = GetVolume();
        //需要一直监听
        volumeSlider.onValueChanged.AddListener((value) =>
        {
            SetVolume(value);
        });

        
        #endregion

        btnPlay.onClick.AddListener(Play);
        btnPause.onClick.AddListener(Pause);

        if (btnStop != null)
            btnStop.onClick.AddListener(Stop);

    }

    public void InitXMPlayer()
    {
        isInit = true;
        //-1 默认设备 0=无声音 
        try
        {
            if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, new IntPtr()))
            {
                BASS_INFO info = new BASS_INFO();
                Bass.BASS_GetInfo(info);
                Debug.Log("Init Success:" + info.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    #region Ctrl
    public void LoadMusic(string musicPath, bool isAutoPlay = true, bool isFreeLastMuisc = true)
    {
        //释放之前的
        if (isFreeLastMuisc)
        {
            Stop();
            Bass.BASS_StreamFree(currentMusicHandle);
        }
        switch (MusicHelper.GetMusicType(musicPath))
        {
            case MusicHelper.MusicType.MOD:
                //MOD音乐获取长度要求在BASS_MusicLoad（String，Int64，Int32，BASSFlag，Int32）调用中使用BASS_MUSIC_PRESCAN标志
                //BASS_MusicLoad 用来加载MOD音乐文件-MO3 / IT / XM / S3M / MTM / MOD / UMX格式。 
                //方法说明:文件路径   偏移   数据长度 0使用所有数据 
                currentMusicHandle = Bass.BASS_MusicLoad(musicPath, 0L, 0, BASSFlag.BASS_MUSIC_PRESCAN, 0);
                break;
            case MusicHelper.MusicType.Stream:
                //BASS_StreamCreateFile 用来加载MP3，MP2，MP1，OGG，WAV，AIFF或插件支持的文件创建示例流
                currentMusicHandle = Bass.BASS_StreamCreateFile(musicPath, 0L, 0, BASSFlag.BASS_DEFAULT);
                break;
            case MusicHelper.MusicType.None:
                Debug.LogError("check file");
                return;
        }

        titleTxt.text = Bass.BASS_ChannelGetMusicName(currentMusicHandle);
        if (isAutoPlay)
            Play();
        BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(currentMusicHandle);
        BASSError bASSError = Bass.BASS_ErrorGetCode();
        Debug.Log($"Load Music Success!!! \ncurrentMusicHandle:{currentMusicHandle}\nTotletime(s):{GetMusicTotleTime()}\nInfo:{info.ToString()}\nerror:{bASSError}\n\n");
    }
    public void LoadMusic(string musicPath)
    {
        //释放之前的
        Stop();
        Bass.BASS_StreamFree(currentMusicHandle);

        switch (MusicHelper.GetMusicType(musicPath))
        {
            case MusicHelper.MusicType.MOD:
                //MOD音乐获取长度要求在BASS_MusicLoad（String，Int64，Int32，BASSFlag，Int32）调用中使用BASS_MUSIC_PRESCAN标志
                //BASS_MusicLoad 用来加载MOD音乐文件-MO3 / IT / XM / S3M / MTM / MOD / UMX格式。 
                //方法说明:文件路径   偏移   数据长度 0使用所有数据 
                currentMusicHandle = Bass.BASS_MusicLoad(musicPath, 0L, 0, BASSFlag.BASS_MUSIC_PRESCAN, 0);
                break;
            case MusicHelper.MusicType.Stream:
                //BASS_StreamCreateFile 用来加载MP3，MP2，MP1，OGG，WAV，AIFF或插件支持的文件创建示例流
                currentMusicHandle = Bass.BASS_StreamCreateFile(musicPath, 0L, 0, BASSFlag.BASS_DEFAULT);
                break;
            case MusicHelper.MusicType.None:
                Debug.LogError("check file");
                return;
        }
        titleTxt.text = Bass.BASS_ChannelGetMusicName(currentMusicHandle);
        Play();
        BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(currentMusicHandle);
        BASSError bASSError = Bass.BASS_ErrorGetCode();
        Debug.Log($"Load Music Success!!! \ncurrentMusicHandle:{currentMusicHandle}\nTotletime(s):{GetMusicTotleTime()}\nInfo:{info.ToString()}\nerror:{bASSError}\n\n");
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="isFreeLastMuisc">是否关闭上一个音乐</param>
    public void Play()
    {
        btnPause.gameObject.SetActive(true);
        btnPlay.gameObject.SetActive(false);
        if (currentMusicHandle == 0)
        {
            Debug.LogError("当前没有音乐加载");
            return;
        }
        BASSActive bASSActive = Bass.BASS_ChannelIsActive(currentMusicHandle);
        if (bASSActive == BASSActive.BASS_ACTIVE_PAUSED) //如果暂停了,重新播放
        {
            Bass.BASS_ChannelPlay(currentMusicHandle, false);
        }
        else  //否则重头开始播放
        {
            //预缓冲
            Bass.BASS_ChannelUpdate(currentMusicHandle, 0);
            //播放 是否重新开始
            Bass.BASS_ChannelPlay(currentMusicHandle, true);
        }
    }
    public void Pause()
    {
        btnPlay.gameObject.SetActive(true);
        btnPause.gameObject.SetActive(false);
        if (isInit && currentMusicHandle != 0)
            Bass.BASS_ChannelPause(currentMusicHandle);
    }
    public void Stop()
    {
        if (isInit && currentMusicHandle != 0)
            Bass.BASS_ChannelStop(currentMusicHandle);
    }
    public void Close()
    {
        Bass.BASS_Stop();
        Bass.BASS_Free();
    }
    /// <summary>
    /// 设置到目标点
    /// </summary>
    /// <param name="second"></param>
    public void Seek(float second)
    {
       // Debug.Log(currentMusicHandle);
        if (currentMusicHandle == 0)
        {
            Debug.LogError("no music file running");
            return;
        }
        if (second > GetMusicTotleTime())
        {
            Debug.LogError("Time is greater than the total time");
            return;
        }
        Bass.BASS_ChannelSetPosition(currentMusicHandle, (double)second);
    }
    /***************  Volume  ************************/
    public void SetVolume(float value)
    {
        value = Mathf.Clamp01(value);
        Bass.BASS_SetVolume(value);
    }
    public void Mute()
    {
       // Bass.BASS_SetVolume(0);
        volumeSlider.value = 0;
    }
    public void MaxVolume()
    {
        //Bass.BASS_SetVolume(1);
        volumeSlider.value = 1;
    }
    public float GetVolume()
    {
        return Bass.BASS_GetVolume();
    }
    /***************  Volume  ************************/

    public void FreeItem()
    {
        if (currentMusicHandle == 0)
        {
            Debug.LogError("当前没有音乐加载");
            return;
        }
        Bass.BASS_StreamFree(currentMusicHandle);
    }
    public void FreeAll()
    {
        Bass.BASS_Free();
    }
    #endregion

    #region Music Data
    /// <summary>
    /// 获取总时间
    /// </summary>
    /// <returns>以s为单位</returns>
    public double GetMusicTotleTime()
    {
        if (currentMusicHandle == 0)
        {
            Debug.LogError("没有音乐加载");
            return -1d;
        }
        //**********Total time***********
        //并不一定准确
        long len = Bass.BASS_ChannelGetLength(currentMusicHandle);
        double totaltime = Bass.BASS_ChannelBytes2Seconds(currentMusicHandle, len); // 返回以秒为单位的时间 the total time length
        if (progressSlider != null) progressSlider.maxValue = (float)totaltime;
        if (totalTxt != null) totalTxt.text = Utils.FixTimespan(totaltime, "MMSS");
        return totaltime;
    }
    /// <summary>
    /// 获取当前时间
    /// </summary>
    /// <returns>以s为单位</returns>
    public double GetMusicCurrentTime()
    {
        if (currentMusicHandle == 0)
        {
            Debug.LogError("没有音乐加载");
            return -1d;
        }
        long pos = Bass.BASS_ChannelGetPosition(currentMusicHandle); // position in bytes
        double currentTime = Bass.BASS_ChannelBytes2Seconds(currentMusicHandle, pos); // 根据通道的格式将字节位置转换为时间（秒）
        return currentTime;
    }
    #endregion

    #region MonoBehaviour
    private void Update()
    {
        if (currentMusicHandle == 0) return;
        timer += Time.deltaTime;
        if (timer >= intervetTime)
        {
            timer = 0;
            if (!sliderIsDrag)
            {
                float currentTime = (float)GetMusicCurrentTime();
                updateTxt.text = Utils.FixTimespan(GetMusicCurrentTime(), "MMSS");
                progressSlider.value = (float)currentTime;
            }
            //BASSActive bASSActive = Bass.BASS_ChannelIsActive(currentMusicHandle);
            //CPU使用率
            //this.text2.text = String.Format("Bass-CPU: {0:0.00}% (not including Waves & Spectrum!)", Bass.BASS_GetCPU());
        }
    }
    private void OnApplicationQuit()
    {
        Close();
    }
    #endregion


}
