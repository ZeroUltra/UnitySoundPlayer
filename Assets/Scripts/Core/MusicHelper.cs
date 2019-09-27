using System.Collections.Generic;
using UnityEngine;
namespace ExpansionMusic
{
    public class MusicHelper 
    {
        private static HashSet<string> modHashSet = new HashSet<string> { "MO3", "IT", "XM", "S3M", "MTM", "MOD", "UMX格式" };
        private static  HashSet<string> streamHashSet = new HashSet<string> { "MP3", "MP2", "MP1", "OGG", "WAV", "AIFF"};
        public enum MusicType
        {
            /// <summary>
            /// MOD音乐文件-MO3 / IT / XM / S3M / MTM / MOD / UMX格式
            /// </summary>
            MOD,
            /// <summary>
            ///   MP3，MP2，MP1，OGG，WAV，AIFF或插件支持的文件创建示例流
            /// </summary>
            Stream,

            None
        }

        public static MusicType GetMusicType(string musicPath)
        {
            musicPath = musicPath.Remove(0, musicPath.LastIndexOf('.')+1);
            musicPath = musicPath.ToUpper();
            if (modHashSet.Contains(musicPath))
                return MusicType.MOD;
            else if (streamHashSet.Contains(musicPath))
                return MusicType.Stream;
            else return MusicType.None;
        }
    }

}
