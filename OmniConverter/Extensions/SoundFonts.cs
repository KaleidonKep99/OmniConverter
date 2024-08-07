﻿using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

namespace OmniConverter
{
    public class SoundFont
    {
        [JsonProperty("Path")]
        public string SoundFontPath { get; private set; }
        [JsonProperty]
        public short SourcePreset { get; private set; }
        [JsonProperty]
        public short SourceBank { get; private set; }
        [JsonProperty]
        public short DestinationPreset { get; private set; }
        [JsonProperty]
        public short DestinationBank { get; private set; }
        [JsonProperty]
        public short DestinationBankLSB { get; private set; }
        [JsonProperty]
        public bool Enabled { get; private set; }
        [JsonProperty]
        public bool XGMode { get; private set; }

        public SoundFont()
        {
            SoundFontPath = string.Empty;
            SourcePreset = -1;
            SourceBank = -1;
            DestinationPreset = -1;
            DestinationBank = 0;
            DestinationBankLSB = 0;
            Enabled = false;
            XGMode = false;
        }

        public SoundFont(string SFP, short SP, short SB, short DP, short DB, short DBLSB, bool E, bool XGM)
        {
            SoundFontPath = SFP;
            SetNewValues(SP, SB, DP, DB, DBLSB, E, XGM);
        }

        public void ChangePath(string SFP)
        {
            SoundFontPath = SFP;
            SetNewValues(SourcePreset, SourceBank, DestinationPreset, DestinationBank, DestinationBankLSB, Enabled, XGMode);
        }

        public void SetNewValues(short SP, short SB, short DP, short DB, short DBLSB, bool E, bool XGM)
        {
            SourcePreset = IsSFZ() && SP < 0 ? (short)0: SP;
            SourceBank = IsSFZ() && SB < 0 ? (short)0 : SB;
            DestinationPreset = IsSFZ() && DP < 0 ? (short)0 : DP;
            DestinationBank = DB;
            DestinationBankLSB = DBLSB;
            Enabled = E;
            XGMode = XGM;
        }

        public bool IsSFZ()
        {
            return Path.GetExtension(SoundFontPath).Equals(".sfz");
        }

        public static FilePickerFileType SoundFontAll { get; } = new("SoundFonts")
        {
            Patterns = new[] { "*.sf2", "*.sf3", "*.sfz" },
            AppleUniformTypeIdentifiers = new[] { "soundfont" },
            MimeTypes = new[] { "audio/x-soundfont" }
        };
    }

    public class SoundFonts
    {
        private ObservableCollection<SoundFont>? _sfList;

        public SoundFonts()
        {
            _sfList = new ObservableCollection<SoundFont>();
        }

        public SoundFonts(ObservableCollection<SoundFont> sfList)
        {
            _sfList = sfList;
        }

        public ObservableCollection<SoundFont> GetSoundFontList() 
        {
            if (_sfList != null)
                return _sfList;

            return [];
        }

        public int GetSoundFontsCount()
        {
            if (_sfList != null)
                return _sfList.Count;

            return 0;
        }

        public void Add(SoundFont? sf)
        {
            if (_sfList != null && sf != null)
                _sfList.Add(sf);
        }

        public void Remove(SoundFont? sf)
        {
            if (_sfList != null && sf != null)
                _sfList.Remove(sf);
        }

        public void Move(SoundFont? sf, MoveDirection direction)
        {
            if (_sfList != null && sf != null)
            {
                int oldIndex = _sfList.IndexOf(sf);
                _sfList.Move(oldIndex, direction);
            }
        }
    }

}
