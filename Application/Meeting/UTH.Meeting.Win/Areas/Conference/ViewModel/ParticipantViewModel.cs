namespace UTH.Meeting.Win.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Timers;
    using System.Threading;
    using System.Threading.Tasks;
    using System.ComponentModel;
    using Newtonsoft.Json.Linq;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Threading;
    using GalaSoft.MvvmLight.Messaging;
    using culture = UTH.Infrastructure.Resource.Culture;
    using UTH.Infrastructure.Utility;
    using UTH.Framework;
    using UTH.Framework.Wpf;
    using UTH.Domain;
    using UTH.Plug;
    using UTH.Meeting.Domain;

    /// <summary>
    /// 参与会议
    /// </summary>
    public class ParticipantViewModel : UTHViewModel
    {
        public ParticipantViewModel() : base(culture.Lang.metHuiYiShi, "")
        {
            Initialize();
        }

        #region 公共属性


        #endregion

        #region 私有变量

        #endregion

        #region 辅助操作

        private void Initialize()
        {
        }

        #endregion
    }
}