using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Linq;

namespace OmniConverter;

public partial class SettingsWindow : Window
{
    private bool ForceLimitAudio = false;
    private bool NoFFMPEGFound = false;

    public SettingsWindow()
    {
        InitializeComponent();

        Loaded += CheckSettings;
    }

    private void CheckSettings(object? sender, RoutedEventArgs e)
    {
        // Jank
        for (int i = 0; i < SampleRate.Items.Count; i++)
        {
            object? item = SampleRate.Items[i];

            if (item != null)
            {
                int freq = Convert.ToInt32(((ComboBoxItem)item).Content);

                if (freq == Program.Settings.SampleRate)
                {
                    SampleRate.SelectedIndex = i;
                    break;
                }
            }
        }

        var maxCodec = Program.FFmpegAvailable ? AudioCodecType.Max : AudioCodecType.PCM;
        if (maxCodec == AudioCodecType.PCM)
        {
            var items = AudioCodec.Items.Where(item => !((ComboBoxItem)item).Name.Contains("WAV"));

            foreach (var item in items.ToList())
                AudioCodec.Items.Remove(item);

            NoFFMPEGFound = true;
            AudioCodec.IsEnabled = false;
        }

        SelectedRenderer.SelectedIndex = ((int)Program.Settings.Renderer).LimitToRange((int)EngineID.BASS, (int)EngineID.MAX);
        AudioRendererChanged(sender, new SelectionChangedEventArgs(e.RoutedEvent, null, null));

        AudioCodec.SelectedIndex = ((int)Program.Settings.AudioCodec).LimitToRange((int)AudioCodecType.PCM, (int)maxCodec);
        AudioBitrate.Value = Program.Settings.AudioBitrate.LimitToRange(1, (int)AudioBitrate.Maximum);

        SincInterSelection.SelectedIndex = ((int)Program.Settings.SincInter).LimitToRange((int)SincInterType.Linear, (int)SincInterType.Max);

        DisableFX.IsChecked = Program.Settings.DisableEffects;
        NoteOff1.IsChecked = Program.Settings.NoteOff1;
        KilledNoteFading.IsChecked = Program.Settings.KilledNoteFading;
        AudioLimiter.IsChecked = Program.Settings.AudioLimiter;
        AudioCodecChanged(sender, new SelectionChangedEventArgs(e.RoutedEvent, null, null));

        RTSMode.IsChecked = Program.Settings.RTSMode;
        RTSFPS.Value = (decimal)Program.Settings.RTSFPS;
        RTSFluct.Value = (decimal)Program.Settings.RTSFluct;
        RTSModeCheck(sender, e);

        FilterVelocity.IsChecked = Program.Settings.FilterVelocity;
        VelocityLowValue.Value = Program.Settings.VelocityLow;
        VelocityHighValue.Value = Program.Settings.VelocityHigh;
        FilterVelocityCheck(sender, e);

        FilterKey.IsChecked = Program.Settings.FilterKey;
        KeyLowValue.Value = Program.Settings.KeyLow;
        KeyHighValue.Value = Program.Settings.KeyHigh;
        FilterKeyCheck(sender, e);

        OverrideEffects.IsChecked = Program.Settings.OverrideEffects;
        ReverbValue.Value = Program.Settings.ReverbVal;
        ChorusValue.Value = Program.Settings.ChorusVal;
        OverrideEffectsCheck(sender, e);

        IgnoreProgramChanges.IsChecked = Program.Settings.IgnoreProgramChanges;

        MTMode.IsChecked = Program.Settings.MultiThreadedMode;
        PerTrackMode.IsChecked = Program.Settings.PerTrackMode;
        PerTrackFile.IsChecked = Program.Settings.PerTrackFile;
        PerTrackStorage.IsChecked = Program.Settings.PerTrackStorage;

        NoLimitThreadsOnCPU.IsChecked = Program.Settings.ThreadsCount > Environment.ProcessorCount;   
        NoLimitThreadsOnCPUCheck(sender, e);
        MaxThreads.Value = Program.Settings.ThreadsCount.LimitToRange(1, (int)MaxThreads.Maximum);

        if ((bool)NoLimitThreadsOnCPU.IsChecked)
            LimitThreads.IsChecked = true;
        else
            LimitThreads.IsChecked = MaxThreads.Value < Environment.ProcessorCount;

        MaxThreadsPanel.IsEnabled = (bool)LimitThreads.IsChecked;

        AutoExportToFolder.IsChecked = Program.Settings.AutoExportToFolder;
        AutoExportFolderPath.Text = Program.Settings.AutoExportFolderPath;
        AutoExportFolderCheck(sender, e);

        AfterRenderAction.IsChecked = Program.Settings.AfterRenderAction >= 0;
        AfterRenderSelectedAction.SelectedIndex = Program.Settings.AfterRenderAction.LimitToRange(0, AfterRenderSelectedAction.Items.Count);
        AfterRenderActionCheck(sender, e);

        NoFFMPEG.IsVisible = NoFFMPEGFound;
        AudioEvents.IsChecked = Program.Settings.AudioEvents;
        OldKMCScheme.IsChecked = Program.Settings.OldKMCScheme;
        AudioEventsCheck(sender, e);
    }

    private async void AutoExportFolderSelection(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);

        if (topLevel != null)
        {
            var startPath = await StorageProvider.TryGetFolderFromPathAsync(Program.Settings.LastExportFolder);
            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                SuggestedStartLocation = startPath,
                Title = "Selected export folder"
            });

            if (folder.Count == 1)
            {
                string? path = folder[0].TryGetLocalPath();

                if (path != null && AutoExportFolderPath.Text != null)
                    AutoExportFolderPath.Text = path;
            }
        }
    }

    private void AudioEventsCheck(object? sender, RoutedEventArgs e)
    {
        if (AudioEvents.IsChecked != null)
            OldKMCScheme.IsEnabled = (bool)AudioEvents.IsChecked;
    }

    private void MaxVoicesChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (MaxVoices.Value > MaxVoices.Maximum)
            MaxVoices.Value = MaxVoices.Maximum;
    }

    private void AudioCodecChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (AudioCodec != null)
        {
            AudioBitrate.IsEnabled = AudioCodec.SelectedIndex > 1;

            switch ((AudioCodecType)AudioCodec.SelectedIndex)
            {
                case AudioCodecType.LAME:
                    ForceLimitAudio = true;
                    AudioLimiter.IsChecked = ForceLimitAudio;
                    break;

                default:
                    ForceLimitAudio = false;
                    AudioLimiter.IsChecked = Program.Settings.AudioLimiter;
                    break;
            }

            AudioLimiter.IsEnabled = !ForceLimitAudio;
        }
    }

    private void KhangModCheck(object? sender, RoutedEventArgs e)
    {
        if (NoVoiceLimit.IsChecked != null)
        {
            switch ((EngineID)SelectedRenderer.SelectedIndex)
            {
                case EngineID.XSynth:
                    NoVoiceLimit.Content = "Unlimited";
                    MaxVoices.IsEnabled = !(bool)NoVoiceLimit.IsChecked;
                    MaxVoices.Value = (bool)NoVoiceLimit.IsChecked ? 0 : Program.Settings.MaxLayers;
                    break;

                default:
                    NoVoiceLimit.Content = "Uncap limit";
                    MaxVoices.IsEnabled = true;
                    MaxVoices.Maximum = (bool)NoVoiceLimit.IsChecked ? int.MaxValue : 100000;
                    break;
            }

            MaxVoicesChanged(null, new NumericUpDownValueChangedEventArgs(e.RoutedEvent, null, null));
        }
    }

    private void AudioRendererChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SelectedRenderer != null)
        {
            switch ((EngineID)SelectedRenderer.SelectedIndex)
            {
                case EngineID.XSynth:
                    NotDesignedForThis.IsVisible = true;
                    MaxVoicesLabel.Content = "Layer count";
                    NoVoiceLimit.IsChecked = Program.Settings.MaxLayers == 0;
                    MaxVoices.Minimum = 0;
                    MaxVoices.Value = Program.Settings.MaxLayers;
                    KhangModCheck(sender, new RoutedEventArgs(e.RoutedEvent));
                    break;

                default:
                    NotDesignedForThis.IsVisible = false;
                    MaxVoicesLabel.Content = "Voice limit";
                    NoVoiceLimit.IsChecked = Program.Settings.MaxVoices > 100000;
                    MaxVoices.Minimum = 1;
                    MaxVoices.Value = Program.Settings.MaxVoices;
                    break;
            }
        }
    }

    private void NoFFMPEGWarning(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        MessageBox.Show(this, "To use additional formats, you need ffmpeg.\n\n" +
            "Please install it on your system, or move the ffmpeg binary to the same folder as the converter.",
            "OmniConverter - Warning", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
    }

    private void NotDesignedForThisWarning(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        MessageBox.Show(this, "This converter is primarily designed around BASSMIDI.\n\n" +
            "Some features might not be available with other audio engines.",
            "OmniConverter - Warning", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
    }

    private void MTModeCheck(object? sender, RoutedEventArgs e)
    {
        if (MTMode.IsChecked != null)
            LimitThreadsPanel.IsEnabled = (bool)MTMode.IsChecked;
    }

    private void OverrideEffectsCheck(object? sender, RoutedEventArgs e)
    {
        if (OverrideEffects.IsChecked != null)
        {
            ReverbValue.IsEnabled = (bool)OverrideEffects.IsChecked;
            ChorusValue.IsEnabled = (bool)OverrideEffects.IsChecked;
        }
    }

    private void FilterVelocityCheck(object? sender, RoutedEventArgs e)
    {
        if (FilterVelocity.IsChecked != null)
        {
            VelocityLowValue.IsEnabled = (bool)FilterVelocity.IsChecked;
            VelocityHighValue.IsEnabled = (bool)FilterVelocity.IsChecked;
        }
    }

    private void FilterKeyCheck(object? sender, RoutedEventArgs e)
    {
        if (FilterKey.IsChecked != null)
        {
            KeyLowValue.IsEnabled = (bool)FilterKey.IsChecked;
            KeyHighValue.IsEnabled = (bool)FilterKey.IsChecked;
        }
    }

    private void PerTrackModeCheck(object? sender, RoutedEventArgs e)
    {
        if (PerTrackMode.IsEnabled)
        {
            if (PerTrackMode.IsChecked != null)
            {
                PerTrackFile.IsEnabled = (bool)PerTrackMode.IsChecked;
                PerTrackStorage.IsEnabled = (bool)PerTrackMode.IsChecked;
            }

            PerTrackFileCheck(sender, e);
        }
    }

    private void PerTrackFileCheck(object? sender, RoutedEventArgs e)
    {
        if (PerTrackFile.IsEnabled)
        {
            if (PerTrackFile.IsChecked != null)
                PerTrackStorage.IsEnabled = (bool)PerTrackFile.IsChecked;
        }
    }

    private void LimitThreadsCheck(object? sender, RoutedEventArgs e)
    {
        if (LimitThreads.IsChecked != null)
            MaxThreadsPanel.IsEnabled = (bool)LimitThreads.IsChecked;
    }

    private void NoLimitThreadsOnCPUCheck(object? sender, RoutedEventArgs e)
    {
        if (NoLimitThreadsOnCPU.IsChecked != null)
        {
            bool val = (bool)NoLimitThreadsOnCPU.IsChecked;

            MaxThreads.Maximum = val ? 65536 : Environment.ProcessorCount;

            if (val && MaxThreads.Value > Environment.ProcessorCount)
                MaxThreads.Value = Environment.ProcessorCount;
        }
    }

    private void RTSModeCheck(object? sender, RoutedEventArgs e)
    {
        if (RTSMode.IsChecked != null)
            RTSPanel.IsEnabled = (bool)RTSMode.IsChecked;
    }

    private void AutoExportFolderCheck(object? sender, RoutedEventArgs e)
    {
        if (AutoExportToFolder.IsChecked != null)
            AutoExportFolderPanel.IsEnabled = (bool)AutoExportToFolder.IsChecked;
    }

    private void AfterRenderActionCheck(object? sender, RoutedEventArgs e)
    {
        if (AfterRenderAction.IsChecked != null)
            AfterRenderSelectedAction.IsEnabled = (bool)AfterRenderAction.IsChecked;
    }

    private void ApplySettings(object? sender, RoutedEventArgs e)
    {
        bool success = true;

        object? item = SampleRate.Items[SampleRate.SelectedIndex];
        if (item != null)
            Program.Settings.SampleRate = Convert.ToInt32(((ComboBoxItem)item).Content);

        if (MaxVoices.Value != null)
        {
            if (SelectedRenderer != null)
            {
                switch ((EngineID)SelectedRenderer.SelectedIndex)
                {
                    case EngineID.XSynth:
                        Program.Settings.MaxLayers = (ulong)MaxVoices.Value;
                        break;

                    default:
                        Program.Settings.MaxVoices = (int)MaxVoices.Value;
                        break;
                }
            }
        }

        Program.Settings.Renderer = (EngineID)SelectedRenderer.SelectedIndex;
        Program.Settings.SincInter = (SincInterType)SincInterSelection.SelectedIndex;

        Program.Settings.AudioCodec = (AudioCodecType)AudioCodec.SelectedIndex;
        if (AudioBitrate.Value != null)
            Program.Settings.AudioBitrate = (int)AudioBitrate.Value;

        if (DisableFX.IsChecked != null)
            Program.Settings.DisableEffects = (bool)DisableFX.IsChecked;
        if (NoteOff1.IsChecked != null)
            Program.Settings.NoteOff1 = (bool)NoteOff1.IsChecked;
        if (KilledNoteFading.IsChecked != null)
            Program.Settings.KilledNoteFading = (bool)KilledNoteFading.IsChecked;

        if (AudioLimiter.IsChecked != null && !ForceLimitAudio)
            Program.Settings.AudioLimiter = (bool)AudioLimiter.IsChecked;

        if (RTSMode.IsChecked != null)
            Program.Settings.RTSMode = (bool)RTSMode.IsChecked;
        if (RTSFPS.Value != null)
            Program.Settings.RTSFPS = (double)RTSFPS.Value;
        if (RTSFluct.Value != null)
            Program.Settings.RTSFluct = (double)RTSFluct.Value;

        if (FilterVelocity.IsChecked != null)
            Program.Settings.FilterVelocity = (bool)FilterVelocity.IsChecked;
        if (VelocityLowValue.Value != null)
            Program.Settings.VelocityLow = (int)VelocityLowValue.Value;
        if (VelocityHighValue.Value != null)
            Program.Settings.VelocityHigh = (int)VelocityHighValue.Value;
        
        if (FilterKey.IsChecked != null)
            Program.Settings.FilterKey = (bool)FilterKey.IsChecked;
        if (KeyLowValue.Value != null)
            Program.Settings.KeyLow = (int)KeyLowValue.Value;
        if (KeyHighValue.Value != null)
            Program.Settings.KeyHigh = (int)KeyHighValue.Value;

        if (OverrideEffects.IsChecked != null)
            Program.Settings.OverrideEffects = (bool)OverrideEffects.IsChecked;
        if (ReverbValue.Value != null)
            Program.Settings.ReverbVal = (short)ReverbValue.Value;
        if (ChorusValue.Value != null)
            Program.Settings.ChorusVal = (short)ChorusValue.Value;

        if (IgnoreProgramChanges.IsChecked != null)
            Program.Settings.IgnoreProgramChanges = (bool)IgnoreProgramChanges.IsChecked;

        if (MTMode.IsChecked != null)
            Program.Settings.MultiThreadedMode = (bool)MTMode.IsChecked;
        if (PerTrackMode.IsChecked != null)
            Program.Settings.PerTrackMode = (bool)PerTrackMode.IsChecked;
        if (PerTrackFile.IsChecked != null)
            Program.Settings.PerTrackFile = (bool)PerTrackFile.IsChecked;
        if (PerTrackStorage.IsChecked != null)
            Program.Settings.PerTrackStorage = (bool)PerTrackStorage.IsChecked;

        if (LimitThreads.IsChecked != null)
        {
            if ((bool)LimitThreads.IsChecked && MaxThreads.Value != null)
                Program.Settings.ThreadsCount = (int)MaxThreads.Value;
            else Program.Settings.ThreadsCount = Environment.ProcessorCount;
        }

        if (AutoExportToFolder.IsChecked != null)
            Program.Settings.AutoExportToFolder = (bool)AutoExportToFolder.IsChecked;

        if (AfterRenderAction.IsChecked != null && (bool)AfterRenderAction.IsChecked)
            Program.Settings.AfterRenderAction = AfterRenderSelectedAction.SelectedIndex;
        else Program.Settings.AfterRenderAction = -1;

        if (AudioEvents.IsChecked != null)
            Program.Settings.AudioEvents = (bool)AudioEvents.IsChecked;

        if (OldKMCScheme.IsChecked != null)
            Program.Settings.OldKMCScheme = (bool)OldKMCScheme.IsChecked;

        var newExportPath = AutoExportFolderPath.Text;
        if (newExportPath != null && !newExportPath.Equals(Program.Settings.AutoExportFolderPath))
        {
            if (!Directory.Exists(newExportPath))
            {
                MessageBox.Show(this, $"\"{newExportPath}\" does not exist.\n\nPlease pick a valid export path.", "OmniConverter - Error", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                success = false;
            }

            // Test the directory for permissions
            if (success)
            {
                FileStream? testStream = null;
                string testFile = $"{newExportPath}/testFile.123";
                try
                {
                    testStream = File.Create(testFile);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(this, $"You do not have enough permissions to write to \"{newExportPath}\".\n\nPlease pick a path you have write access to.", "OmniConverter - Error", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    success = false;
                }
                catch (IOException)
                {
                    MessageBox.Show(this, $"An I/O error has occurred while testing the path \"{newExportPath}\" for write permissions.\n\nPlease pick another path.", "OmniConverter - Error", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    success = false;
                }
                finally
                {
                    if (testStream != null)
                    {
                        testStream.Dispose();
                        File.Delete(testFile);
                    }

                    if (success) Program.Settings.AutoExportFolderPath = newExportPath;
                }
            }
        }

        if (success)
        {
            Program.SaveConfig();
            Close();
            return;
        }

        Program.LoadConfig();
        CheckSettings(sender, e);
    }
}