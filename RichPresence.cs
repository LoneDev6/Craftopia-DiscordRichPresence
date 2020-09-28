using BepInEx;
using Oc;
using System;
using UnityEngine.SceneManagement;
using UniRx;
using Oc.Dungeon;

namespace CraftopiaDiscord
{
    [BepInPlugin("dev.lone.craftopia.discordrichpresence", "Craftopia DiscordRichPresence", "1.0.1")]
    [BepInProcess("Craftopia.exe")]
    public class RichPresence : BaseUnityPlugin
    {
        long CLIENT_ID = 759792773087363094;
        Discord.Discord discord;

        long startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);

            discord = new Discord.Discord(CLIENT_ID, (UInt64)Discord.CreateFlags.NoRequireDiscord);

            RefreshAndSendActivity();
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        void Update()
        {
            discord.RunCallbacks();
        }

        private void SceneManager_activeSceneChanged(Scene oldScene, Scene newScene)
        {
            UnityEngine.Debug.Log("--- Changed scene: " + newScene.name);

            RefreshAndSendActivity();

            if (NetWrapper.LocalPlayer.IsValid)
            {
                NetWrapper.LocalPlayer.Stats.RegisterAddExpCallback((long exp) => {
                    UnityEngine.Debug.Log($"AddExpEvent +{exp}");
                    RefreshAndSendActivity();
                });
                UnityEngine.Debug.Log("------ Registered AddExpEvent");

                NetWrapper.LocalPlayer.RegisterEnterExitDungeonCallback((int dungeonIndex) => {

                    if(NetWrapper.LocalPlayer.GetDungeonData(dungeonIndex) == null)
                        UnityEngine.Debug.Log($"ExitDungeon");
                    else
                        UnityEngine.Debug.Log($"EnterDungeon: {NetWrapper.LocalPlayer.GetDungeonData(dungeonIndex).DungeonName}");
                    RefreshAndSendActivity();
                });
                UnityEngine.Debug.Log("------ Registered EnterDungeon");
            }
        }

        private Discord.Activity BuildActivity()
        {
            var activity = new Discord.Activity
            {
                Details = GetGameDetails(), //top
                State = GetGameState(), //bottom
                Assets = new Discord.ActivityAssets
                {
                    LargeImage = GetLargeImage()
                },
                Timestamps = new Discord.ActivityTimestamps
                {
                    Start = startTime
                }
            };

            if(NetWrapper.LocalPlayer.IsInsideADungeon)
            {
                activity.Assets.SmallImage = "main";
                activity.Assets.SmallText = NetWrapper.LocalPlayer.CurrentDungeon.DungeonName;
            }
            else
            {
                activity.Assets.SmallImage = null;
                activity.Assets.SmallText = null;
            }

            return activity;
        }


        private void RefreshAndSendActivity()
        {
            discord.GetActivityManager().UpdateActivity(BuildActivity(), (res) => {
                if (res != Discord.Result.Ok)
                    UnityEngine.Debug.LogError("Error setting Discord Presence: " + res.ToString());
            });
        }

        private string GetLargeImage()
        {
            if(NetWrapper.LocalPlayer.IsInsideADungeon)
            {
                return "dungeon";
            }
            return "main";
        }

        private string GetGameDetails()
        {
            string scene = SceneManager.GetActiveScene().name;
            switch (scene)
            {
                case "OcScene_Home":
                    return "Main menu";
                case "OcScene_Setting":
                    return "Loading...";

                //case "OcScene_DevTest_yohei_Tutorial_0728_MS": //first island scene
                //    return null;

                default:
                    return $"Level {NetWrapper.LocalPlayer.Stats.Level} ({NetWrapper.LocalPlayer.Stats.PlayerExp}/{NetWrapper.LocalPlayer.Stats.PlayerNextLevelExp}xp)";
            }
        }

        private string GetGameState()
        {
            string scene = SceneManager.GetActiveScene().name;
            switch (scene)
            {
                case "OcScene_Home":
                case "OcScene_Setting":
                    return "";

                //case "OcScene_DevTest_yohei_Tutorial_0728_MS": //first island scene
                default:
                    string currentActivity = (NetWrapper.LocalPlayer.IsInsideADungeon ? "In a Dungeon" : (NetWrapper.IsMultiplayer ? "Multiplayer" : "Singleplayer"));
                    return $"{currentActivity} {(NetWrapper.IsMultiplayer ? $"({NetWrapper.ConnectedPlayersCount} of {NetWrapper.MaxPlayers})" : "")}";
            }
        }
    }
}
