using ItemChanger.Internal;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BingoSync.GameUI
{
    internal class AudioPlayer
    {
        private static readonly string CustomAudioClipsPath = Path.Combine(Path.Combine(Application.persistentDataPath, "BingoSync"), "CustomAudioClips");
        private readonly SoundManager soundManager = new(typeof(BingoSync).Assembly, "BingoSync.Resources.Sounds.");
        private readonly GameObject audioGameObject;
        private readonly AudioSource audioSource;
        private readonly List<string> defaultClipNames = ["Beep 1", "Buzz 1", "Buzz 2", "Click 1", "Ping 1", "Ping 2", "Ping 3"];
        private readonly List<string> customClipNames = [];
        public List<string> ClipNames { get { return [..defaultClipNames, ..customClipNames]; } }
        private readonly List<AudioClip> clips = [];

        public AudioPlayer()
        {
            audioGameObject = new GameObject();
            UnityEngine.Object.DontDestroyOnLoad(audioGameObject);
            audioSource = audioGameObject.AddComponent<AudioSource>();
            foreach (string clipName in defaultClipNames)
            {
                clips.Add(soundManager.GetAudioClip(clipName));
            }
            LoadCustomClips();
        }

        public void LoadCustomClips()
        {
            CreateFolderIfMissing();
            string[] paths = Directory.GetFiles(CustomAudioClipsPath, "*.wav");
            foreach (string path in paths)
            {
                string clipName = Path.GetFileNameWithoutExtension(path);
                customClipNames.Add(clipName);
                using FileStream fs = new(path, FileMode.Open, FileAccess.Read);
                clips.Add(SoundManager.FromStream(fs, clipName));
            }
        }

        private static void CreateFolderIfMissing()
        {
            if (!Directory.Exists(CustomAudioClipsPath))
            {
                Directory.CreateDirectory(CustomAudioClipsPath);
            }
        }

        public void Play(int clip)
        {
            audioSource.PlayOneShot(clips[clip], Controller.GlobalSettings.AudioClipVolume);
        }
    }
}
