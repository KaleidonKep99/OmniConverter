﻿using CSCore;
using System;

namespace OmniConverter
{
    public enum EngineID
    {
        Unknown = -1,
        BASS = 0,
        XSynth = 1,
        MAX = XSynth
    }

    public abstract class AudioEngine : IDisposable
    {
        protected bool _disposed = false;
        public bool Initialized { get; protected set; } = false;
        public WaveFormat WaveFormat { get; protected set; } = new(48000, 32, 2);
        public Settings CachedSettings { get; protected set; }

        public AudioEngine(WaveFormat waveFormat, Settings settings, bool defaultInit = true) 
        {
            CachedSettings = settings;
            WaveFormat = waveFormat; 
            Initialized = defaultInit;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}
