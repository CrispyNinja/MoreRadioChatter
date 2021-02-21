using System;
using System.Threading;
using System.IO;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using System.Windows.Media;
using System.Collections.Generic;
using NAudio.Wave;

namespace MoreRadioChatter
{
    public class AudioManager
    {
        string baseUrl = "lspdfr/audio/scanner/MoreRadioChatter Audio/";

        InitializationFile initFile;
        int volume;
        
        public AudioManager()
        {
            initFile = iniFile();

            volume = int.Parse(initFile.ReadString("Settings", "Volume", "15"));
        }

        private InitializationFile iniFile()
        {
            InitializationFile init = new InitializationFile("Plugins/LSPDFR/MoreRadioChatter.ini");
            init.Create();
            return init;
        }

        private string getLineUrl(string line)
        {
            return baseUrl + line + ".wav";
        }

        private string getLineDir(string line)
        {
            return baseUrl + line;
        }

        public void PlayLine(string line)
        {
            // Split the line to get every file from string
            String[] lines = line.Split(' ');

            GameFiber.Yield(); // idk

            foreach (string file in lines)
            {
                string dirUrl = getLineDir(file);

                try
                {
                    PlaybackEngine eng = new PlaybackEngine(44100, 2);

                    if (!Directory.Exists(dirUrl)) // Is it a file?
                    {
                        string url = getLineUrl(file);

                        Game.LogTrivial("[MoreRadioChatter] Playing chatter '" + file + "'");
                        eng.PlaySound(url, volume);
                    }
                    else // It is a folder
                    {
                        // Get every file in folder
                        DirectoryInfo dir = new DirectoryInfo(dirUrl);
                        FileInfo[] files = dir.GetFiles();
                        // Get one random file from folder
                        FileInfo rndFile = files[new Random().Next(0, files.Length - 1)];

                        Game.LogTrivial("[MoreRadioChatter] Playing random file from folder, chatter '" + file + "'");
                        eng.PlaySound(rndFile.FullName, volume);
                    }

                    while (eng.outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        GameFiber.Sleep(100);
                    }
                    Game.LogTrivial("[MoreRadioChatter] Play done");
                }
                catch(Exception e)
                {
                    Game.LogTrivial("[MoreRadioChatter] Something went wrong for file '" + file + "', see error below:");
                    Game.LogTrivial("[MoreRadioChatter] ERROR: " + e.Message);
                }
            }
        }

    }
}
