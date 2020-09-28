using BepInEx;
using Oc;
using System;
using UnityEngine.SceneManagement;
using UniRx;

namespace CraftopiaDiscord
{
    [BepInPlugin("dev.lone.craftopia.discordrichpresence", "Craftopia DiscordRichPresence", "1.0.0.0")]
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

            OcPl player = SingletonMonoBehaviour<OcNetMng>.FindObjectOfType<OcPl>();
            if (player != null)
            {
                UnityEngine.Debug.Log("------ Registered AddExpEvent");
                OcPlLevelCtrl levelCtrl = player.PlLevelCtrl;
                levelCtrl.AddExpEvent += LevelCtrl_AddExpEvent;
            }
        }

        private void LevelCtrl_AddExpEvent(long exp)
        {
            UnityEngine.Debug.Log($"AddExpEvent +{exp}");
            RefreshAndSendActivity();
        }


        private Discord.Activity BuildActivity()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            return new Discord.Activity
            {
                Details = GetGameDetailsBySceneName(currentSceneName), //top
                State = GetGameStateBySceneName(currentSceneName), //bottom
                Assets = new Discord.ActivityAssets
                {
                    LargeImage = "main"
                },
                Timestamps = new Discord.ActivityTimestamps
                {
                    Start = startTime
                }
            };
        }

        private void RefreshAndSendActivity()
        {
            discord.GetActivityManager().UpdateActivity(BuildActivity(), (res) => {
                if (res != Discord.Result.Ok)
                    UnityEngine.Debug.LogError("Error setting Discord Presence: " + res.ToString());
            });
        }

        string GetGameDetailsBySceneName(string scene)
        {
            switch(scene)
            {
                case "OcScene_Home":
                    return "Main menu";
                case "OcScene_Setting":
                    return "Loading...";

                //case "OcScene_DevTest_yohei_Tutorial_0728_MS": //first island scene
                //    return null;

                default:
                    return $"Level {NetWrapper.PlayerLevel} ({NetWrapper.PlayerExp}/{NetWrapper.PlayerNextLevelExp}xp)";
            }
        }

        private string GetGameStateBySceneName(string scene)
        {
            switch (scene)
            {
                case "OcScene_Home":
                case "OcScene_Setting":
                    return "";

                //case "OcScene_DevTest_yohei_Tutorial_0728_MS": //first island scene
                default:
                    return $"{(NetWrapper.IsMultiplayer ? $"Multiplayer ({NetWrapper.ConnectedPlayersCount} of {NetWrapper.MaxPlayers})" : "Singleplayer")} ";
            }
        }
    }
}
