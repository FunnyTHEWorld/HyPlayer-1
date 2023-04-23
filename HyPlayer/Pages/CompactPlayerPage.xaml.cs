﻿using HyPlayer.Classes;
using HyPlayer.HyPlayControl;
using Microsoft.Toolkit.Uwp.UI.Media;
using System;
using System.IO;
using Windows.Storage.FileProperties;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace HyPlayer.Pages;

/// <summary>
///     可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class CompactPlayerPage : Page, IDisposable
{
    public static readonly DependencyProperty NowProgressProperty = DependencyProperty.Register(
        "NowProgress", typeof(double), typeof(CompactPlayerPage), new PropertyMetadata(default(double)));

    public static readonly DependencyProperty TotalProgressProperty = DependencyProperty.Register(
        "TotalProgress", typeof(double), typeof(CompactPlayerPage), new PropertyMetadata(default(double)));

    public static readonly DependencyProperty AlbumCoverProperty = DependencyProperty.Register(
        "AlbumCover", typeof(Brush), typeof(CompactPlayerPage), new PropertyMetadata(default(Brush)));

    public static readonly DependencyProperty ControlHoverProperty = DependencyProperty.Register(
        "ControlHover", typeof(Brush), typeof(CompactPlayerPage),
        new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

    public static readonly DependencyProperty LyricTextProperty =
        DependencyProperty.Register("LyricText", typeof(string), typeof(CompactPlayerPage),
            new PropertyMetadata("双击此处回正常窗口"));

    public static readonly DependencyProperty LyricTranslationProperty =
        DependencyProperty.Register("LyricTranslation", typeof(string), typeof(CompactPlayerPage),
            new PropertyMetadata("将鼠标移到以查看更多功能"));

    public static readonly DependencyProperty NowPlayingNameProperty =
        DependencyProperty.Register("NowPlayingName", typeof(string), typeof(CompactPlayerPage),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty NowPlayingArtistsProperty =
        DependencyProperty.Register("NowPlayingArtists", typeof(string), typeof(CompactPlayerPage),
            new PropertyMetadata(string.Empty));


    private readonly SolidColorBrush TransparentBrush = new SolidColorBrush(Colors.Transparent);

    public CompactPlayerPage()
    {
        InitializeComponent();
        HyPlayList.OnPlayPositionChange +=
            position => _ = Common.Invoke(() => NowProgress = position.TotalMilliseconds);
        HyPlayList.OnPlayItemChange += OnChangePlayItem;
        HyPlayList.OnPlay += () => _ = Common.Invoke(() => PlayStateIcon.Glyph = "\uEDB4");
        HyPlayList.OnPause += () => _ = Common.Invoke(() => PlayStateIcon.Glyph = "\uEDB5");
        HyPlayList.OnLyricChange += OnLyricChanged;
        HyPlayList.OnSongLikeStatusChange += HyPlayList_OnSongLikeStatusChange;
        //CompactPlayerAni.Begin();
    }

    private void HyPlayList_OnSongLikeStatusChange(bool isLiked)
    {
        _ = Common.Invoke(() =>
        {
            IconLiked.Foreground = isLiked
                ? new SolidColorBrush(Colors.Red)
                : Application.Current.Resources["TextFillColorPrimaryBrush"] as Brush;
            IconLiked.Glyph = isLiked
                ? "\uE00B"
                : "\uE006";
        });
    }

    public double NowProgress
    {
        get => (double)GetValue(NowProgressProperty);
        set => SetValue(NowProgressProperty, value);
    }

    public double TotalProgress
    {
        get => (double)GetValue(TotalProgressProperty);
        set => SetValue(TotalProgressProperty, value);
    }

    public Brush AlbumCover
    {
        get => (Brush)GetValue(AlbumCoverProperty);
        set => SetValue(AlbumCoverProperty, value);
    }

    public Brush ControlHover
    {
        get => (Brush)GetValue(ControlHoverProperty);
        set => SetValue(ControlHoverProperty, value);
    }


    public string LyricText
    {
        get => (string)GetValue(LyricTextProperty);
        set => SetValue(LyricTextProperty, value);
    }


    public string LyricTranslation
    {
        get => (string)GetValue(LyricTranslationProperty);
        set => SetValue(LyricTranslationProperty, value);
    }


    public string NowPlayingName
    {
        get => (string)GetValue(NowPlayingNameProperty);
        set => SetValue(NowPlayingNameProperty, value);
    }


    public string NowPlayingArtists
    {
        get => (string)GetValue(NowPlayingArtistsProperty);
        set => SetValue(NowPlayingArtistsProperty, value);
    }

    private void OnLyricChanged()
    {
        if (HyPlayList.LyricPos == -1) return;
        if (HyPlayList.Lyrics.Count <= HyPlayList.LyricPos) return;
        _ = Common.Invoke(() =>
        {
            LyricText = HyPlayList.Lyrics[HyPlayList.LyricPos].LyricLine.CurrentLyric;
            LyricTranslation = HyPlayList.Lyrics[HyPlayList.LyricPos].Translation;
        });
    }

    public void Dispose()
    {
        HyPlayList.OnPlayPositionChange -=
            position => _ = Common.Invoke(() => NowProgress = position.TotalMilliseconds);
        HyPlayList.OnPlayItemChange -= OnChangePlayItem;
        HyPlayList.OnPlay -= () => _ = Common.Invoke(() => PlayStateIcon.Glyph = "\uEDB4");
        HyPlayList.OnPause -= () => _ = Common.Invoke(() => PlayStateIcon.Glyph = "\uEDB5");
        HyPlayList.OnLyricChange -= OnLyricChanged;
        HyPlayList.OnSongLikeStatusChange -= HyPlayList_OnSongLikeStatusChange;
        //CompactPlayerAni.Begin();
    }

    private void OnChangePlayItem(HyPlayItem item)
    {
        _ = Common.Invoke(async () =>
        {
            NowPlayingName = item?.PlayItem?.Name;
            NowPlayingArtists = item?.PlayItem?.ArtistString;
            BitmapImage img = null;
            if (item != null)
                if (!Common.Setting.noImage)
                    if (item.ItemType is HyPlayItemType.Local or HyPlayItemType.LocalProgressive)
                    {
                        img = new BitmapImage();
                        if (!Common.Setting.useTaglibPicture || item.PlayItem?.LocalFileTag is null || item.PlayItem.LocalFileTag.Pictures.Length == 0)
                        {
                            await img.SetSourceAsync(
                                await HyPlayList.NowPlayingStorageFile?.GetThumbnailAsync(ThumbnailMode.MusicView, 9999));
                        }
                        else
                        {
                            await img.SetSourceAsync(new MemoryStream(item.PlayItem.LocalFileTag.Pictures[0].Data.Data).AsRandomAccessStream());
                        }
                    }
                    else
                    {
                        img = new BitmapImage(new Uri(HyPlayList.NowPlayingItem.PlayItem.Album.cover));
                    }

            TotalProgress = item?.PlayItem?.LengthInMilliseconds ?? 0;
            AlbumCover = new ImageBrush { ImageSource = img, Stretch = Stretch.UniformToFill };
        });
        if (item.ItemType is not HyPlayItemType.Local or HyPlayItemType.LocalProgressive)
        {
            var isLiked = Common.LikedSongs.Contains(HyPlayList.NowPlayingItem.PlayItem.Id);
            _ = Common.Invoke(() =>
            {
                IconLiked.Foreground = isLiked
                    ? new SolidColorBrush(Colors.Red)
                    : Application.Current.Resources["TextFillColorPrimaryBrush"] as Brush;
                IconLiked.Glyph = isLiked
                    ? "\uE00B"
                    : "\uE006";
            });
        }
    }

    private void MovePrevious(object sender, RoutedEventArgs e)
    {
        HyPlayList.SongMovePrevious();
    }

    private void MoveNext(object sender, RoutedEventArgs e)
    {
        HyPlayList.SongMoveNext();
    }

    private void ChangePlayState(object sender, RoutedEventArgs e)
    {
        if (HyPlayList.IsPlaying) HyPlayList.Player.Pause();
        else HyPlayList.Player.Play();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        OnChangePlayItem(HyPlayList.NowPlayingItem);
        PlayStateIcon.Glyph = HyPlayList.IsPlaying ? "\uEDB4" : "\uEDB5";
        Common.BarPlayBar.Visibility = Visibility.Collapsed;
        Window.Current.SetTitleBar(MainGrid);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        Dispose();
        Common.BarPlayBar.Visibility = Visibility.Visible;
    }

    private void ExitCompactMode(object sender, DoubleTappedRoutedEventArgs e)
    {
        _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
        Common.PageMain.ExpandedPlayer.Navigate(typeof(ExpandedPlayer));
    }

    private void CompactPlayerPage_OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ControlHover = new BackdropBlurBrush { Amount = 10.0 };
        PlayProgress.Visibility = Visibility.Visible;
        
    }

    private void CompactPlayerPage_OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (!Common.Setting.CompactPlayerPageBlurStatus)
            ControlHover = TransparentBrush;
        PlayProgress.Visibility = Visibility.Collapsed;

    }

    private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        Common.Setting.CompactPlayerPageBlurStatus = !Common.Setting.CompactPlayerPageBlurStatus;
    }

    private void LikeButton_Click(object sender, RoutedEventArgs e)
    {
        HyPlayList.LikeSong();
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        _ = ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
        Common.PageMain.ExpandedPlayer.Navigate(typeof(ExpandedPlayer));
    }
}