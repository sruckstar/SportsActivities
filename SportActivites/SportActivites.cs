using GTA;
using GTA.Native;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace SportActivites
{
    public class SportActivites : Script
    {

        Ped player = Game.Player.Character;

        ScriptSettings config;
        int KeyActiveA;
        int KeyActiveB;
        int KeySpace;
        int PushUp = 0; // 0 - mode disabled, 1 - enter mode, 2 - in mode, 3 - exit mode
        int stonks = 0;
        float altPercantage = 1.0f;
        int warning1 = 0;
        int warning2 = 0;
        int lang = 1;

        public SportActivites()
        {

            Setup();

            Tick += OnTick;
        }

        void Setup()
        {
            config = ScriptSettings.Load("Scripts\\SportsActivites.ini");
            KeyActiveA = config.GetValue<int>("MAIN", "Key1", 205);
            KeyActiveB = config.GetValue<int>("MAIN", "Key2", 206);
            KeySpace = config.GetValue<int>("MAIN", "Key3", 203);
            lang = config.GetValue<int>("MAIN", "lang", 1);
        }

        void CheckPushUps()
        {
            if (PushUp == 1) // Enter mode
            {
                GTA.Native.Function.Call(GTA.Native.Hash.ENABLE_CLOWN_BLOOD_VFX, true);
                GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS_IMMEDIATELY, player);
                LoadAndPlayEnterAnim();
                PushUp = 2;
                Wait(3000);
                PlayStayAnim();
                ShowMessengeStart();
            }
            else
            {
                if (PushUp == 3) // Exit mode
                {
                    LoadAndPlayExitAnim();
                    GTA.Native.Function.Call(GTA.Native.Hash.ENABLE_CLOWN_BLOOD_VFX, false);
                    Wait(3000);
                    PushUp = 0;
                }
                else
                {
                    if (PushUp == 4) // The player did a push-up after pressing the key (normal)
                    {
                        PlayPushUpAnim();
                        Wait(1100);

                        PushUp = 2;
                        PlayStayAnim();
                    }
                    else
                    {
                        if (PushUp == 5) // The player did a push-up after pressing the key (tired)
                        {
                            PlayPushUpAnimFatigue();
                            Wait(1100);
                            PushUp = 2;
                            PlayStayAnimFatigue();
                        }
                        else
                        {
                            if(Game.Player.Character.IsDead)
                            {
                                PushUp = 0;
                                GTA.Native.Function.Call(GTA.Native.Hash.ENABLE_CLOWN_BLOOD_VFX, false);

                            }
                        }
                    }
                }
            }
        }

        void LoadAndPlayEnterAnim()
        {
            Game.Player.Character.Task.PlayAnimation("amb@world_human_push_ups@male@enter", "enter");
        }

        void LoadAndPlayExitAnim()
        {
            Game.Player.Character.Task.PlayAnimation("amb@world_human_push_ups@male@exit", "exit", 8.0f, -1, GTA.AnimationFlags.None);
        }

        void PlayStayAnim()
        {
            Game.Player.Character.Task.PlayAnimation("amb@world_human_push_ups@male@idle_a", "idle_a", 8.0f, -1, GTA.AnimationFlags.Loop);
        }

        void PlayStayAnimFatigue()
        {
            Game.Player.Character.Task.PlayAnimation("amb@world_human_push_ups@male@idle_a", "idle_e", 8.0f, -1, GTA.AnimationFlags.Loop);
        }

        void PlayPushUpAnim()
        {
            Game.Player.Character.Task.PlayAnimation("amb@world_human_push_ups@male@idle_a", "idle_d");
        }

        void PlayPushUpAnimFatigue()
        {
            Game.Player.Character.Task.PlayAnimation("amb@world_human_push_ups@male@base", "base");
        }

        void ShowMessengeStart()
        {
            if (lang == 1)
            {
                GTA.UI.Screen.ShowHelpText("Press ~INPUT_FRONTEND_X~ to do push-ups.");
            }
            else
            {
                GTA.UI.Screen.ShowHelpText("Нажимайте ~INPUT_FRONTEND_X~, чтобы отжиматься.");
            }
        }


        void OnTick(object sender, EventArgs e)
        {
            OnKeyDownAlt();
            CheckPushUps();
            RestoreFatigue();
            Warnings();
        }

        void OnKeyDownAlt()
        {
            if (Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, KeyActiveA) && Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, KeyActiveB) && PushUp == 0 && altPercantage >= 1.0)
            {
                PushUp = 1;
            }
            else
            {
                if (Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, KeyActiveA) && Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, KeyActiveB) && PushUp == 2)
                {
                    PushUp = 3;
                }
                else
                {
                    if (Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, KeySpace) && PushUp == 2 && altPercantage > 0.0)
                    {
                        PushUp = 4;
                        altPercantage -= 0.01f;
                        SetStonks();
                    }
                    else
                    {
                        if (Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, KeySpace) && PushUp == 2 && 0.0 >= altPercantage)
                        {
                            PushUp = 5;                          
                            Game.Player.Character.Health -= 15;
                        }
                        else
                        {
                            if (Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, KeyActiveA) && Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, KeyActiveB) && PushUp == 0 && 1.0 > altPercantage)
                            {
                                ShowCancelled();
                            }
                        }
                    }
                }
            }
        }

        void Warnings()
        {
            if(warning1 == 0 && 0.3 >= altPercantage)
            {
                ShowWarning1();
                warning1 = 1;
            }
            else
            {
                if (warning2 == 0 && 0.0 >= altPercantage)
                {
                    ShowWarning2();
                    warning2 = 1;
                }
            }
        }

        void SetStaminaForPlayer()
        {
            if (Game.Player.Character.Model.Hash == new Model("player_zero").Hash)
            {
                StatAddInt("SP0_STRENGTH", 1);
            }
            else
            {
                if (Game.Player.Character.Model.Hash == new Model("player_one").Hash)
                {
                    StatAddInt("SP1_STRENGTH", 1);
                }
                else
                {
                    if (Game.Player.Character.Model.Hash == new Model("player_two").Hash)
                    {
                        StatAddInt("SP2_STRENGTH", 1);
                    }
                }
            }
        }

        void RestoreFatigue()
        {
            if (altPercantage != 1.0 && PushUp != 4)
            {
                altPercantage += 0.0001f;
            }
        }

        void ShowCancelled()
        {
            if (lang == 1)
            {
                GTA.UI.Screen.ShowHelpText("The character does not want to do push-ups yet.");
            }
            else
            {
                GTA.UI.Screen.ShowHelpText("Персонаж не хочет заниматься в данный момент.");
            }
        }

        void ShowWarning1()
        {
            if (lang == 1)
            {
                GTA.UI.Screen.ShowHelpText("The character is tired. Take a break.");
            }
            else
            {
                GTA.UI.Screen.ShowHelpText("Персонаж начинает уставать. Сделайте перерыв.");
            }
        }

        void ShowWarning2()
        {
            if (lang == 1)
            {
                GTA.UI.Screen.ShowHelpText("The character is very tired and loses health.");
            }
            else
            {
                GTA.UI.Screen.ShowHelpText("Персонаж очень устал и начал терять здоровье.");
            }
        }

        void SetStonks()
        {
            stonks += 1;
            if (stonks >= 20)
            {
                SetStaminaForPlayer();
                stonks = 0;
            }
        }

        void StatAddInt(string statName, int addValue)
        {
            float i = 0.0f;
            int b = 0;

            Function.Call(Hash.STAT_GET_FLOAT, Game.GenerateHash(statName), i, true);
            i += 1.0f;
            b = (int)i;
            Function.Call(Hash.STAT_SET_INT, Game.GenerateHash(statName), b, true);
        }
    }
}