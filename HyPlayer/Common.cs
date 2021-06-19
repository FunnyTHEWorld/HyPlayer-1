﻿using HyPlayer.Classes;
using HyPlayer.Controls;
using HyPlayer.Pages;
using NeteaseCloudMusicApi;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Kawazu;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;

namespace HyPlayer
{
    internal class Common
    {
        public static NeteaseCloudMusicApi.CloudMusicApi ncapi = new CloudMusicApi();
        public static bool Logined = false;
        public static NCUser LoginedUser;
        public static ExpandedPlayer PageExpandedPlayer;
        public static MainPage PageMain;
        public static PlayBar BarPlayBar;
        public static Frame BaseFrame;
        public static BasePage PageBase;
        public static Setting Setting; 
        public static bool ShowLyricSound = true;
        public static bool ShowLyricTrans = true;
        public static Dictionary<string, object> GLOBAL = new Dictionary<string, object>();
        public static List<string> LikedSongs = new List<string>();
        public static KawazuConverter KawazuConv = null;
        public static List<NCPlayList> MySongLists = new List<NCPlayList>();
        public static List<NCSong> ListedSongs = new List<NCSong>();

        public static async void Invoke(Action action, Windows.UI.Core.CoreDispatcherPriority Priority = Windows.UI.Core.CoreDispatcherPriority.Normal)
        {
            try
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Priority,
                    () => { action(); });
            }
            catch (Exception e)
            {
#if RELEASE
                Crashes.TrackError(e);
#endif
                /*
                Invoke((async () =>
                {
                    await new ContentDialog
                    {
                        Title = "发生错误",
                        Content = "Error: " + e.Message + "\r\n" + e.StackTrace,
                        CloseButtonText = "关闭",
                        DefaultButton = ContentDialogButton.Close
                    }.ShowAsync();
                }));
                */
            }

        }
    }

    internal struct Setting
    {
        public string audioRate
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("audioRate"))
                    return ApplicationData.Current.LocalSettings.Values["audioRate"].ToString();
                return "999000";
            }
            set => ApplicationData.Current.LocalSettings.Values["audioRate"] = value;
        }

        public int Volume
        {
            get
            {
                try
                {
                    if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Volume"))
                        return int.Parse(ApplicationData.Current.LocalSettings.Values["Volume"].ToString());
                }
                catch
                {
                    return 50;
                }
                return 50;
            }

            set => ApplicationData.Current.LocalSettings.Values["Volume"] = value;
        }

        public string downloadDir
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("downloadDir"))
                    return ApplicationData.Current.LocalSettings.Values["downloadDir"].ToString();
                return ApplicationData.Current.LocalCacheFolder.Path;
            }
            set => ApplicationData.Current.LocalSettings.Values["downloadDir"] = value;
        }

        public bool toastLyric
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("toastLyric"))
                    return ApplicationData.Current.LocalSettings.Values["toastLyric"].ToString() == "true";
                return false;
            }
            set => ApplicationData.Current.LocalSettings.Values["toastLyric"] = value ? "true" : "false";
        }

        public bool expandAnimation
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("expandAnimation"))
                    return ApplicationData.Current.LocalSettings.Values["expandAnimation"].ToString() != "false";
                return true;
            }
            set => ApplicationData.Current.LocalSettings.Values["expandAnimation"] = value ? "true" : "false";
        }
    }
    internal class HistoryManagement
    {

        public static void AddNCSongHistory(NCSong song)
        {
            var list = new List<NCSong>();
            if (ApplicationData.Current.LocalSettings.Values["songHistory"] == null)
            {
                ApplicationData.Current.LocalSettings.Values["songHistory"] = JsonConvert.SerializeObject(list);
            }
            else
            {
                list = JsonConvert.DeserializeObject<List<NCSong>>(ApplicationData.Current.LocalSettings.Values["songHistory"].ToString());
            }
            for(int i=0;i<list.Count;i++)
            {
                NCSong inSong = list[i];
                if (inSong.sid == song.sid)
                    list.Remove(inSong);
            }
            if (!list.Contains(song))
                list.Insert(0,song);
            if (list.Count >= 9)
                list.RemoveRange(9, list.Count - 9);
            ApplicationData.Current.LocalSettings.Values["songHistory"] = JsonConvert.SerializeObject(list);
        }
        public static void AddSearchHistory(String Text)
        {
            var list = new List<string>();
            if (ApplicationData.Current.LocalSettings.Values["searchHistory"] == null)
            {
                ApplicationData.Current.LocalSettings.Values["searchHistory"] = JsonConvert.SerializeObject(list);
            }
            else
            {
                list = JsonConvert.DeserializeObject<List<string>>(ApplicationData.Current.LocalSettings.Values["searchHistory"].ToString());
            }

            list.Add(Text);
            ApplicationData.Current.LocalSettings.Values["searchHistory"] = JsonConvert.SerializeObject(list);
        }
        public static void AddSonglistHistory(NCPlayList playList)
        {
            var list = new List<NCPlayList>();
            if (ApplicationData.Current.LocalSettings.Values["songlistHistory"] == null)
            {
                ApplicationData.Current.LocalSettings.Values["songlistHistory"] = JsonConvert.SerializeObject(list);
            }
            else
            {
                list = JsonConvert.DeserializeObject<List<NCPlayList>>(ApplicationData.Current.LocalSettings.Values["songlistHistory"].ToString());
            }
            for (int i = 0; i < list.Count; i++)
            {
                NCPlayList inlist = list[i];
                if (inlist.plid == playList.plid)
                    list.Remove(inlist);
            }

            if (!list.Contains(playList))
                list.Insert(0,playList);
            if (list.Count >= 10)
                list.RemoveRange(10, list.Count - 10);
            ApplicationData.Current.LocalSettings.Values["songlistHistory"] = JsonConvert.SerializeObject(list);
        }
        public static void ClearHistory()
        {
            ApplicationData.Current.LocalSettings.Values["songlistHistory"] = null;
            ApplicationData.Current.LocalSettings.Values["songHistory"] = null;
            ApplicationData.Current.LocalSettings.Values["searchHistory"] = null;
        }
        public static List<NCSong> GetNCSongHistory()
        {
            if (ApplicationData.Current.LocalSettings.Values["songHistory"] != null)
                return JsonConvert.DeserializeObject<List<NCSong>>(ApplicationData.Current.LocalSettings.Values["songHistory"].ToString());
            else return new List<NCSong> { };
        }
        public static List<NCPlayList> GetSonglistHistory()
        {
            if (ApplicationData.Current.LocalSettings.Values["songlistHistory"] != null)
                return JsonConvert.DeserializeObject<List<NCPlayList>>(ApplicationData.Current.LocalSettings.Values["songlistHistory"].ToString());
            else return new List<NCPlayList> { };
        }
        public static List<String> GetSearchHistory()
        {
            if (ApplicationData.Current.LocalSettings.Values["searchHistory"] != null)
                return JsonConvert.DeserializeObject<List<string>>(ApplicationData.Current.LocalSettings.Values["searchHistory"].ToString());
            else return new List<String> { };
        }
    }
    internal static class Extensions
    {
        public static byte[] ToByteArrayUtf8(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static string ToHexStringLower(this byte[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte t in value)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }

        public static string ToHexStringUpper(this byte[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte t in value)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        public static string ToBase64String(this byte[] value)
        {
            return Convert.ToBase64String(value);
        }

        public static byte[] ComputeMd5(this byte[] value)
        {
            MD5 md5 = MD5.Create();
            return md5.ComputeHash(value);
        }

        public static byte[] RandomBytes(this Random random, int length)
        {
            byte[] buffer = new byte[length];
            random.NextBytes(buffer);
            return buffer;
        }

        public static string Get(this CookieCollection cookies, string name, string defaultValue)
        {
            return cookies[name]?.Value ?? defaultValue;
        }
    }
}
