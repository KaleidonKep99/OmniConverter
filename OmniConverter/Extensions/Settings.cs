using FFMpegCore;
using FFMpegCore.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace OmniConverter
{
    public enum AudioCodecType
    {
        PCM,
        FLAC,
        LAME,
        Vorbis
    }

    public static class AudioCodecTypeExtensions
    {
        public static string ToExtension(this AudioCodecType codec)
        {
            return codec switch
            {
                AudioCodecType.PCM => ".wav",
                AudioCodecType.FLAC => ".flac",
                AudioCodecType.LAME => ".mp3",
                AudioCodecType.Vorbis => ".ogg",
                _ => ""
            };
        }

        public static Codec? ToFFMpegCodec(this AudioCodecType codec)
        {
            return codec switch
            {
                AudioCodecType.PCM => null, // We don't need to convert PCM
                AudioCodecType.FLAC => FFMpeg.GetCodec("flac"),
                AudioCodecType.LAME => FFMpeg.GetCodec("libmp3lame"),
                AudioCodecType.Vorbis => FFMpeg.GetCodec("libvorbis"),
                _ => null
            };
        }
    }

    public class Settings
    {
        [JsonProperty]
        public EngineID Renderer = EngineID.BASS;

        [JsonProperty]
        public float Volume = 100.0f;
        [JsonProperty]
        public int SampleRate = 48000;
        [JsonProperty]
        public AudioCodecType AudioCodec = AudioCodecType.PCM;
        [JsonProperty]
        public int AudioBitrate = 256; // kbps

        [JsonProperty]
        public int MaxVoices = 1000;
        [JsonProperty]
        public short ReverbVal = 64;
        [JsonProperty]
        public short ChorusVal = 64;
        [JsonProperty]
        public bool DisableEffects = true;
        [JsonProperty]
        public bool OverrideEffects = false;
        [JsonProperty]
        public bool AudioLimiter = false;

        // Threads count is set to ProcessorCount - 1,
        // to avoid having the converter hog up all the resources
        [JsonProperty]
        public bool MultiThreadedMode = true;
        [JsonProperty]
        public int ThreadsCount = Environment.ProcessorCount - 1;
        [JsonProperty]
        public bool PerTrackMode = false;
        [JsonProperty]
        public bool PerTrackFile = false;
        [JsonProperty]
        public bool PerTrackStorage = false;

        [JsonProperty]
        public bool NoteOff1 = false;
        [JsonProperty]
        public bool SincInter = true;

        [JsonProperty]
        public bool RichPresence = true;

        [JsonProperty]
        public bool RTSMode = false;
        [JsonProperty]
        public double RTSFPS = 90.0;
        [JsonProperty]
        public double RTSFluct = 75.0;

        [JsonProperty]
        public bool AutoUpdateCheck = false;
        [JsonProperty]
        public UpdateSystem.Branch UpdateBranch = UpdateSystem.Branch.None;

        [JsonProperty]
        public bool AutoExportToFolder = false;
        [JsonProperty]
        public string AutoExportFolderPath = string.Empty;
        [JsonProperty]
        public int AfterRenderAction = -1;

        [JsonProperty]
        public string LastMIDIFolder = string.Empty;
        [JsonProperty]
        public string LastSoundFontFolder = string.Empty;
        [JsonProperty]
        public string LastExportFolder = string.Empty;

        [JsonProperty("SoundFonts")]
        public ObservableCollection<SoundFont> SoundFontsList = new();
    }
}
