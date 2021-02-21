using System;
using System.Collections.Generic;
using System.Threading;
using Rage;
using Rage.Native;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Engine;
using LSPD_First_Response.Mod.Callouts;

[assembly: Rage.Attributes.Plugin("MoreRadioChatter", Description = "Want a more realistic experience while playing LSPDFR?", Author = "AkASlow")]
namespace MoreRadioChatter
{
    public class EntryPoint : Plugin
    {
        private bool isAsleep;
        private int lastIndex;

        int sleepMin;
        int sleepMax;
        int currentLines;
        bool onlyInVehicle;
        bool onlyInPoliceVehicle;
        bool playInPursuit;
        InitializationFile initFile;
        List<string> chatterLines;

        private AudioManager audioManager;

        private InitializationFile iniFile()
        {
            InitializationFile init = new InitializationFile("Plugins/LSPDFR/MoreRadioChatter.ini");
            init.Create();
            return init;
        }

        private List<String> chatterLinesFromIni()
        {
            List<string> list = new List<string>();

            for (int i = 1; i < currentLines; i++)
            {
                list.Add(initFile.ReadString("Lines", "Chatter_" + i, "null"));
            }

            return list;
        }

        private void playRandomAudio()
        {
            if(isAsleep)
            {
                return;
            }

            // First sleep
            isAsleep = true;
            int sleepRnd = new Random().Next(sleepMin, sleepMax);
            Game.LogTrivial("[MoreRadioChatter] Sleeping for " + sleepRnd + " seconds!");
            GameFiber.Sleep(sleepRnd * 1000);

            // Select random audio
            int randomIndex = new Random().Next(1, chatterLines.Count);
            if(chatterLines.Count > 1)
            {
                if(lastIndex != randomIndex)
                {
                    this.lastIndex = randomIndex;
                }
                else
                {
                    // Last played is same...
                    if(randomIndex > 2 && chatterLines.Count > 2)
                    {
                        randomIndex--;
                    }
                    else if(randomIndex < 2 && chatterLines.Count > 2)
                    {
                        randomIndex++;
                    }
                    else
                    {
                        Game.LogTrivial("[MoreRadioChatter] Could not play chatter: '" + randomIndex + "'");
                        isAsleep = false;
                        return;
                    }
                }
            }

            // Get the player
            Ped localPed = Game.LocalPlayer.Character;

            // If the player is in a pursuit, don't play audio
            if(!playInPursuit && Functions.IsPedInPursuit(localPed))
            {
                Game.LogTrivial("[MoreRadioChatter] Should have played, but player is in pursuit");
                isAsleep = false;
                return;
            }

            // Player must be in vehicle if we can play the chatter
            if(onlyInVehicle)
            {
                if (localPed.CurrentVehicle != null)
                {
                    if(onlyInPoliceVehicle)
                    {
                        // Get the current vehicle to verify it is a police vehicle
                        if (localPed.IsInAnyPoliceVehicle)
                        {
                            // Only in vehicle (must be police), play chatter
                            Game.LogTrivial("[MoreRadioChatter] Playing: " + randomIndex);
                            audioManager.PlayLine(chatterLines[randomIndex]);
                        }
                        else
                        {
                            Game.LogTrivial("[MoreRadioChatter] Should have played, but not in police vehicle");
                        }
                    }
                    else
                    {
                        // Not only in police vehicle, play chatter
                        Game.LogTrivial("[MoreRadioChatter] Playing: " + randomIndex);
                        audioManager.PlayLine(chatterLines[randomIndex]);
                    }
                }
                else
                {
                    Game.LogTrivial("[MoreRadioChatter] Should have played, but not in vehicle");
                }
            }
            else
            {
                // Not only in vehicle, play chatter on foot as well
                Game.LogTrivial("[MoreRadioChatter] Playing: " + randomIndex);
                audioManager.PlayLine(chatterLines[randomIndex]);
            }

            isAsleep = false;
        }

        public EntryPoint()
        {
            GameFiber.StartNew((() =>
                {
                    while(true)
                    {
                        playRandomAudio();
                        GameFiber.Yield();
                    }
                }
            ));
        }

        public override void Initialize()
        {
            initFile = iniFile();

            sleepMin = int.Parse(initFile.ReadString("Settings", "MinWait", "15"));
            sleepMax = int.Parse(initFile.ReadString("Settings", "MaxWait", "45"));
            currentLines = int.Parse(initFile.ReadString("Settings", "CurrentLines", "0"));

            onlyInVehicle = initFile.ReadBoolean("Settings", "OnlyInVehicle", true);
            onlyInPoliceVehicle = initFile.ReadBoolean("Settings", "OnlyInPoliceVehicle", true);
            playInPursuit = initFile.ReadBoolean("Settings", "PlayInPursuit", false);

            audioManager = new AudioManager();
            chatterLines = chatterLinesFromIni();

            Game.LogTrivial("[MoreRadioChatter] Plugin loaded!");
        }

        public override void Finally()
        {

        }
    }
}
