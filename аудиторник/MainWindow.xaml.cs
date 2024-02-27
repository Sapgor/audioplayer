using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace AudioPlayer
{
    public partial class MainWindow : Window
    {
        private List<string> playlist;
        private int currentTrackIndex;
        private bool isPlaying;
        private bool isShuffle;
        private bool isRepeat;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            playlist = new List<string>();
            currentTrackIndex = 0;
            isPlaying = false;
            isShuffle = false;
            isRepeat = false;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private List<string> history = new List<string>();
        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Audio Files (*.mp3;*.wav)|*.mp3;*.wav";

            if (openFileDialog.ShowDialog() == true)
            {
                string[] files = openFileDialog.FileNames;

                playlist.Clear();
                playlist.AddRange(files);

                currentTrackIndex = 0;
                history.Add(files[currentTrackIndex]);
                PlayTrack();
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.Count == 0)
            {
                MessageBox.Show("История прослушивания пуста");
                return;
            }

            HistoryListBox.Items.Clear();
            foreach (string track in playlist)
            {
                HistoryListBox.Items.Add(System.IO.Path.GetFileName(track));
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                PauseTrack();
            }
            else
            {
                ContinueTrack();
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrackIndex > 0)
            {
                currentTrackIndex--;
                PlayTrack();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentTrackIndex < playlist.Count - 1)
            {
                currentTrackIndex++;
                PlayTrack();
            }
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            isShuffle = !isShuffle;

            if (isShuffle)
            {
                ShufflePlaylist();
            }
            else
            {
                currentTrackIndex = 0;
            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string trackPath = playlist[currentTrackIndex];
                mediaElement.Source = new Uri(trackPath);
                mediaElement.Play();
                isPlaying = true;
                timer.Start();
            }
            catch (System.ArgumentOutOfRangeException)
            { }
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeSpan newPosition = TimeSpan.FromSeconds(PositionSlider.Value);
                mediaElement.Position = newPosition;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = VolumeSlider.Value;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeSpan currentPosition = mediaElement.Position;
                TimeSpan totalDuration = mediaElement.NaturalDuration.TimeSpan;

                PositionSlider.Minimum = 0;
                PositionSlider.Maximum = totalDuration.TotalSeconds;
                PositionSlider.Value = currentPosition.TotalSeconds;

                RemainingTimeLabel.Content = "-" + (totalDuration - currentPosition).ToString(@"mm\:ss");
                CurrentTimeLabel.Content = currentPosition.ToString(@"mm\:ss");
            }
        }

        private void PlayTrack()
        {
            string trackPath = playlist[currentTrackIndex];
            mediaElement.Source = new Uri(trackPath);
            mediaElement.Play();
            isPlaying = true;
            PlayPauseButton.Content = "Пауза";
            timer.Start();
        }

        private void PauseTrack()
        {
            mediaElement.Pause();
            isPlaying = false;
            PlayPauseButton.Content = "Продолжить";
            timer.Stop();
        }

        private void ContinueTrack()
        {
            mediaElement.Play();
            isPlaying = true;
            PlayPauseButton.Content = "Пауза";
            timer.Start();
        }

        private void ShufflePlaylist()
        {
            Random random = new Random();

            for (int i = 0; i < playlist.Count; i++)
            {
                int randomIndex = random.Next(i, playlist.Count);
                string temp = playlist[i];
                playlist[i] = playlist[randomIndex];
                playlist[randomIndex] = temp;
            }

            currentTrackIndex = 0;
        }
    }
}