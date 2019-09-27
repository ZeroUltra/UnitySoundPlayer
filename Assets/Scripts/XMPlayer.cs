using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using System;
using System.Runtime.InteropServices;

public class XMPlayer : MonoBehaviour
{
    public Button btnPlay, btnStop, btnPause, btnUnpause, btnTest;
    public Slider progressSlider;
    public Text text1, text2;

    public string musicName;
    public double second;
    public long pos;
    private bool isInit = false;
    private int currentXMHandle;


    void Start()
    {
        InitXMPlayer();
        btnPlay.onClick.AddListener(() => Play($"{Application.streamingAssetsPath}/{musicName}"));
        btnStop.onClick.AddListener(Stop);
        btnPause.onClick.AddListener(Pause);
        btnUnpause.onClick.AddListener(UnPause);
        btnTest.onClick.AddListener(() =>
        {
            //bool isok = Bass.BASS_ChannelBytes2Seconds(currentXMHandle, pos);
            //Debug.Log(isok);
            //double elapsedtime = Bass.BASS_ChannelBytes2Seconds(currentXMHandle, pos);
            //Debug.Log(elapsedtime);
            bool isok = Bass.BASS_ChannelSetPosition(currentXMHandle, pos);
            Debug.Log(isok);
        });
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
    public void Play(string xmPath)
    {
        //一开始进来应该释放之前的
        if (currentXMHandle != 0)
        {
            Stop();
            Bass.BASS_StreamFree(currentXMHandle);
        }
        //文件路径   偏移   数据长度 0使用所有数据 
        currentXMHandle = Bass.BASS_MusicLoad(xmPath, 0L, 0, BASSFlag.BASS_DEFAULT, 0);        //加载MOD音乐文件-MO3 / IT / XM / S3M / MTM / MOD / UMX格式。        
                                                                                               // currentXMHandle = Bass.BASS_StreamCreateFile(xmPath, 0L, 0, BASSFlag.BASS_DEFAULT);  // 从MP3，MP2，MP1，OGG，WAV，AIFF或插件支持的文件创建示例流

        //播放 是否重新开始
        Bass.BASS_ChannelPlay(currentXMHandle, true);
        //Info
        BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(currentXMHandle);
        Debug.Log($"currentXMHandle:{currentXMHandle} Info:{info.ToString()}");
        //--------------
        // 要转换的位置（以秒为单位，例如0.03 = 30ms）

        //经过测试 获取xm音乐的长度 Bass.BASS_ChannelGetLength 失败
        //设置位置失败 Bass.BASS_ChannelSetPosition 只能设置到0
        //Mp3其他文件可以
    }
    private void Update()
    {
        if (currentXMHandle != 0)
        {
            // Bug MOD音乐文件获取不到长度
            //long len = Bass.BASS_ChannelGetLength(currentXMHandle,BASSMode.); // length in bytes
            //double totaltime = Bass.BASS_ChannelBytes2Seconds(currentXMHandle, len); // 返回以秒为单位的时间 the total time length
            //text1.text = "TotlaTime:" + (float)totaltime;

            long pos = Bass.BASS_ChannelGetPosition(currentXMHandle); // position in bytes
                                                                      //  Debug.Log(pos);
            double elapsedtime = Bass.BASS_ChannelBytes2Seconds(currentXMHandle, pos); // the elapsed time length
            this.text2.text = String.Format("Elapsed: {0:#0.00}", Utils.FixTimespan(elapsedtime, "MMSS"));
            //CPU使用率
            // this.text2.text = String.Format("Bass-CPU: {0:0.00}% (not including Waves & Spectrum!)", Bass.BASS_GetCPU());
        }
    }
    public void Pause()
    {
        if (isInit && currentXMHandle != 0)
            Bass.BASS_ChannelPause(currentXMHandle);
    }
    public void UnPause()
    {
        if (isInit && currentXMHandle != 0)
            Bass.BASS_ChannelPlay(currentXMHandle, false);
    }
    public void Stop()
    {
        if (isInit && currentXMHandle != 0)
            Bass.BASS_ChannelStop(currentXMHandle);
    }
    private void OnApplicationQuit()
    {
        Bass.BASS_Stop();
        Bass.BASS_Free();
    }
}
