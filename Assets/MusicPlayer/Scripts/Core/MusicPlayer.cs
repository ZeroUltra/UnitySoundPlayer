using UnityEngine;
using Un4seen.Bass;
using System;

namespace Music.Core
{
    public class MusicPlayer
    {
        private int currentMusicHandle;
        public int CurrentMusicHandle
        {
            get { return currentMusicHandle; }
            private set { currentMusicHandle = value; }
        }

        public MusicPlayer()
        {
            InitXMPlayer();
            if (Bass.BASS_GetDevice() == -1)
                Debug.LogError("No Device");
        }

        private void InitXMPlayer()
        {
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
        /***************  Load  ************************/
        public void LoadMusic(string musicPath, bool isAutoPlay = true, bool isFreeLastMuisc = true)
        {
            //释放之前的
            if (isFreeLastMuisc)
            {
                Stop();
                Bass.BASS_StreamFree(CurrentMusicHandle);
            }
            switch (MusicHelper.GetMusicType(musicPath))
            {
                case MusicHelper.MusicType.MOD:
                    //MOD音乐获取长度要求在BASS_MusicLoad（String，Int64，Int32，BASSFlag，Int32）调用中使用BASS_MUSIC_PRESCAN标志
                    //BASS_MusicLoad 用来加载MOD音乐文件-MO3 / IT / XM / S3M / MTM / MOD / UMX格式。 
                    //方法说明:文件路径   偏移   数据长度 0使用所有数据 
                    CurrentMusicHandle = Bass.BASS_MusicLoad(musicPath, 0L, 0, BASSFlag.BASS_MUSIC_PRESCAN, 0);
                    break;
                case MusicHelper.MusicType.Stream:
                    //BASS_StreamCreateFile 用来加载MP3，MP2，MP1，OGG，WAV，AIFF或插件支持的文件创建示例流
                    CurrentMusicHandle = Bass.BASS_StreamCreateFile(musicPath, 0L, 0, BASSFlag.BASS_DEFAULT);
                    break;
                case MusicHelper.MusicType.None:
                    Debug.LogError("check file");
                    return;
            }
            if (isAutoPlay)
                Play();
            BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(CurrentMusicHandle);
            BASSError bASSError = Bass.BASS_ErrorGetCode();
            Debug.Log($"Load Music Success!!! \ncurrentMusicHandle:{CurrentMusicHandle}\nTotletime(s):{GetMusicTotleTime()}\nInfo:{info.ToString()}\nerror:{bASSError}\n\n");
        }
        public void LoadMusic(string musicPath)
        {
            //释放之前的
            Stop();
            Bass.BASS_StreamFree(CurrentMusicHandle);

            switch (MusicHelper.GetMusicType(musicPath))
            {
                case MusicHelper.MusicType.MOD:
                    //MOD音乐获取长度要求在BASS_MusicLoad（String，Int64，Int32，BASSFlag，Int32）调用中使用BASS_MUSIC_PRESCAN标志
                    //BASS_MusicLoad 用来加载MOD音乐文件-MO3 / IT / XM / S3M / MTM / MOD / UMX格式。 
                    //方法说明:文件路径   偏移   数据长度 0使用所有数据 
                    CurrentMusicHandle = Bass.BASS_MusicLoad(musicPath, 0L, 0, BASSFlag.BASS_MUSIC_PRESCAN, 0);
                    break;
                case MusicHelper.MusicType.Stream:
                    //BASS_StreamCreateFile 用来加载MP3，MP2，MP1，OGG，WAV，AIFF或插件支持的文件创建示例流
                    CurrentMusicHandle = Bass.BASS_StreamCreateFile(musicPath, 0L, 0, BASSFlag.BASS_DEFAULT);
                    break;
                case MusicHelper.MusicType.None:
                    Debug.LogError("check file");
                    return;
            }
            Play();
            BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(CurrentMusicHandle);
            BASSError bASSError = Bass.BASS_ErrorGetCode();
            Debug.Log($"Load Music Success!!! \ncurrentMusicHandle:{CurrentMusicHandle}\nTotletime(s):{GetMusicTotleTime()}\nInfo:{info.ToString()}\nerror:{bASSError}\n\n");
        }
        /***************  Ctrl  ************************/
        public void Play()
        {
            if (CurrentMusicHandle == 0)
            {
                Debug.LogError("No music loading");
                return;
            }
            BASSActive bASSActive = Bass.BASS_ChannelIsActive(CurrentMusicHandle);
            if (bASSActive == BASSActive.BASS_ACTIVE_PAUSED) //如果暂停了,重新播放
            {
                Bass.BASS_ChannelPlay(CurrentMusicHandle, false);
            }
            else  //否则重头开始播放
            {
                //预缓冲
                Bass.BASS_ChannelUpdate(CurrentMusicHandle, 0);
                //播放 是否重新开始
                Bass.BASS_ChannelPlay(CurrentMusicHandle, true);
            }
        }
        public void Pause()
        {
            if (CurrentMusicHandle != 0)
                Bass.BASS_ChannelPause(CurrentMusicHandle);
        }
        public void Stop()
        {
            if (CurrentMusicHandle != 0)
                Bass.BASS_ChannelStop(CurrentMusicHandle);
        }
        public void Close()
        {
            Bass.BASS_Stop();
            Bass.BASS_Free();
        }
        public void Seek(float second)
        {
            // Debug.Log(currentMusicHandle);
            if (CurrentMusicHandle == 0)
            {
                Debug.LogError("no music Playing");
                return;
            }
            if (second > GetMusicTotleTime())
            {
                Debug.LogError("Time is greater than the total time");
                return;
            }
            long tLong = Bass.BASS_ChannelSeconds2Bytes(CurrentMusicHandle, (double)second);
            bool flag = Bass.BASS_ChannelSetPosition(CurrentMusicHandle, tLong, BASSMode.BASS_MUSIC_POSRESET);
            if (flag == false)
                Debug.Log("Seek error  " + Bass.BASS_ErrorGetCode().ToString());

        }
        /***************  Volume  ************************/
        public void SetVolume(float value)
        {
            value = Mathf.Clamp01(value);
            Bass.BASS_SetVolume(value);
        }
        public void Mute()
        {
            Bass.BASS_SetVolume(0);
        }
        public void MaxVolume()
        {
            Bass.BASS_SetVolume(1);
        }
        public float GetVolume()
        {
            return Bass.BASS_GetVolume();
        }
        /***************  Free  ************************/
        public void FreeItem()
        {
            if (CurrentMusicHandle == 0)
            {
                Debug.LogError("No music loading");
                return;
            }
            Bass.BASS_StreamFree(CurrentMusicHandle);
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
            if (CurrentMusicHandle == 0)
            {
                Debug.LogError("No music loading");
                return -1d;
            }
            //**********Total time***********
            //并不一定准确
            long len = Bass.BASS_ChannelGetLength(CurrentMusicHandle);
            double totaltime = Bass.BASS_ChannelBytes2Seconds(CurrentMusicHandle, len); // 返回以秒为单位的时间 the total time length
            return totaltime;
        }
        /// <summary>
        /// 获取当前时间
        /// </summary>
        /// <returns>以s为单位</returns>
        public double GetMusicCurrentTime()
        {
            if (CurrentMusicHandle == 0)
            {
                Debug.LogError("No music loading");
                return -1d;
            }
            long pos = Bass.BASS_ChannelGetPosition(CurrentMusicHandle); // position in bytes
            double currentTime = Bass.BASS_ChannelBytes2Seconds(CurrentMusicHandle, pos); // 根据通道的格式将字节位置转换为时间（秒）
            return currentTime;
        }
        public string GetMusicName()
        {
            if (CurrentMusicHandle == 0)
            {
                Debug.LogError("No music loading");
                return null;
            }
            return Bass.BASS_ChannelGetMusicName(CurrentMusicHandle);
        }
        public bool GetInitMusic()
        {
            return CurrentMusicHandle != 0;
        }
        public string GetCPU()
        {
            return string.Format("Bass-CPU: {0:0.00}%", Bass.BASS_GetCPU());
        }
        #endregion
    }
}
