using Oc;
using Oc.Dungeon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace CraftopiaDiscord
{
    /// <summary>
    /// Wrapper to access info about the current game session
    /// </summary>
    class NetWrapper
    {
        private static Player CACHED_localPlayer;

        public static int MaxPlayers => OcNetMng.MAX_PLAYER_NUM;
        public static int ConnectedPlayersCount => SingletonMonoBehaviour<OcNetMng>.Inst.ConnectPlayerNum;
        public static bool IsMultiplayer => SingletonMonoBehaviour<OcNetMng>.Inst.isMutilPlay;

        /// <summary>
        /// Get your current player instance
        /// </summary>
        public static Player LocalPlayer
        {
            get
            {
                if (CACHED_localPlayer == null || !CACHED_localPlayer.IsValid)
                {
                    CACHED_localPlayer = new Player();
                }
                return CACHED_localPlayer;
            }
        }
    }


    /// <summary>
    /// Wrapper to access local player character data
    /// </summary>
    public class Player
    {
        OcPlMaster internalPlayer;
        public Stats Stats;

        private int lastDungeonId = -1;

        public Player() : this(OcPlMaster.Inst){}

        public Player(OcPlMaster internalPlayer)
        {
            this.internalPlayer = internalPlayer;
            this.Stats = new Stats(this);

            this.RegisterEnterExitDungeonCallback((int index) => {
                lastDungeonId = index;
            });
        }

        /// <summary>
        /// Check if the local player object is available
        /// (if you're currently ingame or not)
        /// </summary>
        public bool IsValid => internalPlayer != null;

        /// <summary>
        /// Access some internal player data, use with caution
        /// </summary>
        public OcPlMaster InternalPlayer  => internalPlayer;

        /// <summary>
        /// Access dungeons manager, use with caution
        /// </summary>
        public OcDungeonManager InternalDungeonManager => SingletonMonoBehaviour<OcDungeonManager>.Inst;

        /// <summary>
        /// Check if player is currently inside a dungeon
        /// </summary>
        public bool IsInsideADungeon => CurrentDungeon != null;

        /// <summary>
        /// Get current dungeon internal object. If null means that you're not in a dungeon.
        /// </summary>
        public OcDungeonSo CurrentDungeon
        {
            get
            {
                if (!IsValid || InternalPlayer.CurrentDungeonId == -1)
                    return null;

                return GetDungeonData(InternalPlayer.CurrentDungeonId);
            }
        }

        /// <summary>
        /// Get current dungeon internal object by its index. If null means that you're not in a dungeon.
        /// </summary>
        public OcDungeonSo GetDungeonData(int index)
        {
            return InternalDungeonManager.GetDungeonData(index);
        }

        public bool RegisterEnterExitDungeonCallback(Action<int> action)
        {
            if (!IsValid)
                return false;
            SingletonMonoBehaviour<OcDungeonManager>.Inst.onEnterDungeon += action;
            return true;
        }
    }

    /// <summary>
    /// Wrapper to access character exp and level
    /// </summary>
    public class Stats
    {
        Player player;

        public Stats(Player pl)
        {
            this.player = pl;
        }

        public byte Level => player.InternalPlayer.PlLevelCtrl.Level.Value;
        public long PlayerExp => player.InternalPlayer.PlLevelCtrl.Exp.Value;
        public long PlayerNextLevelExp
        {
            get
            {
                if (player.IsValid)
                    return ((LongReactiveProperty)typeof(OcPlLevelCtrl).GetField("requirNextLevelUpExp", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(player.InternalPlayer.PlLevelCtrl)).Value;
                return -1;
            }
        }
        public bool RegisterAddExpCallback(Action<long> action)
        {
            if (player.IsValid)
                return false;
            OcPlLevelCtrl levelCtrl = player.InternalPlayer.PlLevelCtrl;
            levelCtrl.AddExpEvent += action;
            return true;
        }
    }

}
