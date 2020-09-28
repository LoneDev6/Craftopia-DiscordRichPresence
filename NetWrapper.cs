using Oc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UniRx;

namespace CraftopiaDiscord
{
    class NetWrapper
    {
        private static FieldInfo CACHED_requirNextLevelUpExp;

        public static int MaxPlayers => OcNetMng.MAX_PLAYER_NUM;
        public static int ConnectedPlayersCount => SingletonMonoBehaviour<OcNetMng>.Inst.ConnectPlayerNum;
        public static byte PlayerLevel => OcPlMaster.Inst.PlLevelCtrl.Level.Value;
        public static bool IsMultiplayer => SingletonMonoBehaviour<OcNetMng>.Inst.isMutilPlay;
        public static long PlayerExp => OcPlMaster.Inst.PlLevelCtrl.Exp.Value;
        public static long PlayerNextLevelExp
        {
            get
            {
                if (CACHED_requirNextLevelUpExp == null)
                    CACHED_requirNextLevelUpExp = typeof(OcPlLevelCtrl).GetField("requirNextLevelUpExp", BindingFlags.NonPublic | BindingFlags.Instance);

                OcPl player = SingletonMonoBehaviour<OcNetMng>.FindObjectOfType<OcPl>();
                if (player == null)
                    return -1;
                OcPlLevelCtrl levelCtrl = player.PlLevelCtrl;
                if (player == null)
                    return -1;
                return ((LongReactiveProperty)CACHED_requirNextLevelUpExp.GetValue(levelCtrl)).Value;
            }
        }
    }
}
