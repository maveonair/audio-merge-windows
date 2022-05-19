using AudioMerge.App.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace AudioMerge.App.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<AudioFile> AudioFiles { get; }

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(MergeCommand))]
        public bool hasFiles = false;

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(AddAudioFilesCommand))]
        [AlsoNotifyChangeFor(nameof(RemoveSelectedAudioFilesCommand))]
        [AlsoNotifyChangeFor(nameof(ClearAudioFilesCommand))]
        [AlsoNotifyChangeFor(nameof(MergeCommand))]
        public bool isMerging = false;

        [ObservableProperty]
        public TimeSpan? totalDuration;

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(RemoveSelectedAudioFilesCommand))]
        public AudioFile selectedAudioFile;

        public IRelayCommand AddAudioFilesCommand { get; }

        public IRelayCommand RemoveSelectedAudioFilesCommand { get; }

        public IRelayCommand ClearAudioFilesCommand { get; }

        public IAsyncRelayCommand MergeCommand { get; }

        public MainViewModel()
        {
            AudioFiles = new ObservableCollection<AudioFile>();
            AddAudioFilesCommand = new RelayCommand(AddAudioFilesAysnc, CanAddAudioFiles);
            RemoveSelectedAudioFilesCommand = new RelayCommand(RemoveSelectedAudioFile, CanRemoveSelectedAudioFile);
            ClearAudioFilesCommand = new RelayCommand(ClearAudioFiles, CanClearAudioFiles);
            MergeCommand = new AsyncRelayCommand(StartMergeAsync, CanMergeAudioFiles);
        }


        private async void AddAudioFilesAysnc()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".m4a");
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wma");

            // Fixes: https://github.com/microsoft/WindowsAppSDK/issues/1188#issuecomment-1066261574
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var files = await picker.PickMultipleFilesAsync();

            if (files.Count > 0)
            {
                foreach (var file in files)
                {
                    AudioFiles.Add(new AudioFile(file.Path));
                }
            }

            HasFiles = AudioFiles.Any();
            TotalDuration = AudioFiles.Aggregate(TimeSpan.Zero,
                (sum, next) => sum + next.Duration);
        }

        private bool CanAddAudioFiles() => !IsMerging;

        public void RemoveSelectedAudioFile()
        {
            AudioFiles.Remove(SelectedAudioFile);
        }

        public bool CanRemoveSelectedAudioFile() => SelectedAudioFile != null && !IsMerging;

        private void ClearAudioFiles()
        {
            AudioFiles.Clear();
            HasFiles = false;
            TotalDuration = null;
        }

        private bool CanClearAudioFiles() => HasFiles && !IsMerging;

        private async Task StartMergeAsync()
        {
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("M4a", new List<string>() { ".m4a" });
            picker.SuggestedFileName = "output.m4a";
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;

            // Fixes: https://github.com/microsoft/WindowsAppSDK/issues/1188#issuecomment-1066261574
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                return;
            }

            try
            {
                var audioFileReaders = AudioFiles.Select(a => new AudioFileReader(a.FilePath));
                var playlist = new ConcatenatingSampleProvider(audioFileReaders);

                IsMerging = true;

                await Task.Run(() =>
                {
                    MediaFoundationEncoder.EncodeToAac(playlist.ToWaveProvider(), file.Path);
                }).ContinueWith(_ =>
                {
                    IsMerging = false;
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                // TODO: Proper Error Handling if something goes south :O...
                Console.WriteLine(e);
            }
        }

        private bool CanMergeAudioFiles() => HasFiles && !IsMerging;
    }
}
