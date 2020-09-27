using BepInEx;
using Oc;
using System;
using UnityEngine.SceneManagement;

namespace CraftopiaDiscord
{
    [BepInPlugin("dev.lone.craftopia.discordrichpresence", "Craftopia DiscordRichPresence", "1.0.0.0")]
    [BepInProcess("Craftopia.exe")]
    public class RichPresence : BaseUnityPlugin
    {
        long CLIENT_ID = 759792773087363094;

        Discord.Discord discord;

        long startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        int playersCount = 0;

        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);

            discord = new Discord.Discord(CLIENT_ID, (UInt64)Discord.CreateFlags.NoRequireDiscord);

            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            UnityEngine.Debug.Log("------ Changed scene: " + SceneManager.GetActiveScene().name);

            playersCount = FindObjectsOfType<OcCharacter>().Length;

            var activityManager = discord.GetActivityManager();
            var activity = new Discord.Activity
            {
                Details = GetDetailsBySceneName(arg1.name),
                State = playersCount > 1 ? "Multiplayer" : "Singleplayer",
                Assets = new Discord.ActivityAssets
                {
                    LargeImage = "main"
                },
                Timestamps = new Discord.ActivityTimestamps
                {
                    Start = startTime
                }
            };
            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res != Discord.Result.Ok)
                {
                    UnityEngine.Debug.LogError("Error setting Discord Presence: " + res.ToString());
                }
            });
        }

        // Update is called once per frame
        void Update()
        {
            discord.RunCallbacks();
        }

        string GetDetailsBySceneName(string scene)
        {
            switch(scene)
            {
                case "OcScene_Home":
                    return "Main menu";
                case "OcScene_Setting":
                    return "Loading...";

                case "OcScene_DevTest_yohei_Tutorial_0728_MS":
                    return "Ingame";

                default:
                    return "Ingame";
            }
        }
    }
}
