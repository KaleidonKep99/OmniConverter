using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OmniConverter;

public partial class SettingsWindow : Window
{
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

        KhangMod.IsChecked = Program.Settings.MaxVoices > 100000;
        MaxVoices.Value = Program.Settings.MaxVoices;
        AudioCodec.SelectedIndex = (int)Program.Settings.AudioCodec;
        AudioBitrate.Value = Program.Settings.AudioBitrate;

        SincInter.IsChecked = Program.Settings.SincInter;
        DisableFX.IsChecked = Program.Settings.DisableEffects;
        NoteOff1.IsChecked = Program.Settings.NoteOff1;
        AudioLimiter.IsChecked = Program.Settings.AudioLimiter;

        OverrideEffects.IsChecked = Program.Settings.OverrideEffects;
        ReverbValue.Value = Program.Settings.ReverbVal;
        ChorusValue.Value = Program.Settings.ChorusVal;
        OverrideEffectsCheck(sender, e);

        RTSMode.IsChecked = Program.Settings.RTSMode;
        RTSFPS.Value = (decimal)Program.Settings.RTSFPS;
        RTSFluct.Value = (decimal)Program.Settings.RTSFluct;
        RTSModeCheck(sender, e);

        MTMode.IsChecked = Program.Settings.MultiThreadedMode;
        PerTrackMode.IsChecked = Program.Settings.PerTrackMode;
        PerTrackFile.IsChecked = Program.Settings.PerTrackFile;
        PerTrackStorage.IsChecked = Program.Settings.PerTrackStorage;

        MaxThreads.Maximum = Environment.ProcessorCount;
        MaxThreads.Value = Program.Settings.ThreadsCount.LimitToRange(1, (int)MaxThreads.Maximum);

        LimitThreads.IsChecked = MaxThreads.Value < Environment.ProcessorCount;
        MaxThreadsPanel.IsEnabled = (bool)LimitThreads.IsChecked;

        AutoExportToFolder.IsChecked = Program.Settings.AutoExportToFolder;
        AutoExportFolderPath.Text = Program.Settings.AutoExportFolderPath;
        AutoExportFolderCheck(sender, e);

        AfterRenderAction.IsChecked = Program.Settings.AfterRenderAction >= 0;
        AfterRenderSelectedAction.SelectedIndex = Program.Settings.AfterRenderAction.LimitToRange(0, AfterRenderSelectedAction.Items.Count);
        AfterRenderActionCheck(sender, e);
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

    private void KhangModCheck(object? sender, RoutedEventArgs e)
    {
        if (KhangMod.IsChecked != null)
        {
            MaxVoices.Maximum = (bool)KhangMod.IsChecked ? int.MaxValue : 100000;
            MaxVoicesChanged(null, new NumericUpDownValueChangedEventArgs(e.RoutedEvent, null, null));
        }
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
            if (AudioCodec.SelectedIndex <= 1)
            {
                AudioBitrate.IsEnabled = false;
            }
            else
            {
                AudioBitrate.IsEnabled = true;
            }
        }
    }

    private void OverrideEffectsCheck(object? sender, RoutedEventArgs e)
    {
        if (OverrideEffects.IsChecked != null)
        {
            ReverbValPanel.IsEnabled = (bool)OverrideEffects.IsChecked;
            ChorusValPanel.IsEnabled= (bool)OverrideEffects.IsChecked;
        }
    }

    private void MTModeCheck(object? sender, RoutedEventArgs e)
    {
        if (MTMode.IsChecked != null)
            LimitThreadsPanel.IsEnabled = (bool)MTMode.IsChecked;
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
            Program.Settings.MaxVoices = (int)MaxVoices.Value;
        Program.Settings.AudioCodec = (AudioCodecType)AudioCodec.SelectedIndex;
        if (AudioBitrate.Value != null)
            Program.Settings.AudioBitrate = (int)AudioBitrate.Value;

        if (SincInter.IsChecked != null) 
            Program.Settings.SincInter = (bool)SincInter.IsChecked;
        if (DisableFX.IsChecked != null)
            Program.Settings.DisableEffects = (bool)DisableFX.IsChecked;
        if (NoteOff1.IsChecked != null)
            Program.Settings.NoteOff1 = (bool)NoteOff1.IsChecked;

        if (AudioLimiter.IsChecked != null)
            Program.Settings.AudioLimiter = (bool)AudioLimiter.IsChecked;

        if (OverrideEffects.IsChecked != null)
            Program.Settings.OverrideEffects = (bool)OverrideEffects.IsChecked;
        if (ReverbValue.Value != null)
            Program.Settings.ReverbVal = (short)ReverbValue.Value;
        if (ChorusValue.Value != null)
            Program.Settings.ChorusVal = (short)ChorusValue.Value;

        if (RTSMode.IsChecked != null)
            Program.Settings.RTSMode = (bool)RTSMode.IsChecked;
        if (RTSFPS.Value != null)
            Program.Settings.RTSFPS = (double)RTSFPS.Value;
        if (RTSFluct.Value != null)
            Program.Settings.RTSFluct = (double)RTSFluct.Value;

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

        var newExportPath = AutoExportFolderPath.Text;
        if (newExportPath != null && !newExportPath.Equals(Program.Settings.AutoExportFolderPath))
        {
            if (!Directory.Exists(newExportPath))
            {
                MessageBox.Show($"\"{newExportPath}\" does not exist.\n\nPlease pick a valid export path.", "OmniConverter - Error", this, MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
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
                    MessageBox.Show($"You do not have enough permissions to write to \"{newExportPath}\".\n\nPlease pick a path you have write access to.", "OmniConverter - Error", this, MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    success = false;
                }
                catch (IOException)
                {
                    MessageBox.Show($"An I/O error has occurred while testing the path \"{newExportPath}\" for write permissions.\n\nPlease pick another path.", "OmniConverter - Error", this, MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
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