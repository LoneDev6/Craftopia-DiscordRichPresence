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
        public static int MaxPlayers => OcNetMng.MAX_PLAYER_NUM;

        public static int ConnectedPlayersCount => SingletonMonoBehaviour<OcNetMng>.Inst.ConnectPlayerNum;

        public static byte PlayerLevel => SingletonMonoBehaviour<OcNetMng>.FindObjectOfType<OcPl>().PlLevelCtrl.Level.Value;

        public static bool IsMultiplayer => SingletonMonoBehaviour<OcNetMng>.Inst.isMutilPlay;

        public static long PlayerExp => SingletonMonoBehaviour<OcNetMng>.FindObjectOfType<OcPl>().PlLevelCtrl.Exp.Value;
        
        public static long PlayerNextLevelExp
        {
            get
            {
                OcPlLevelCtrl levelCtrl = SingletonMonoBehaviour<OcNetMng>.FindObjectOfType<OcPl>().PlLevelCtrl;
                return ((LongReactiveProperty)typeof(OcPlLevelCtrl).GetField("requirNextLevelUpExp", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(levelCtrl)).Value;
            }
        }
    }
}
