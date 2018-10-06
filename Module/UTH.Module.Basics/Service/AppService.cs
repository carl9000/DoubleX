﻿namespace UTH.Module.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text;
    using UTH.Infrastructure.Resource;
    using UTH.Infrastructure.Resource.Culture;
    using UTH.Infrastructure.Utility;
    using UTH.Framework;
    using UTH.Domain;
    using UTH.Plug;

    /// <summary>
    /// 应用程序业务
    /// </summary>
    public class AppService : ApplicationDefault<AppEntity, AppEditInput, AppOutput>, IAppService
    {
        #region 构造函数

        public AppService(IRepository<AppEntity> _repository) : base(_repository)
        {
        }

        #endregion

        #region 私有变量

        #endregion

        #region 公共属性

        #endregion

        #region 辅助操作

        #endregion

        #region 回调事件

        public override Action<AppEditInput> InsertBeforeCall => (input) =>
        {
            var isExist = Find(predicate: x => x.Name == input.Name || x.Code == input.Code);
            if (!isExist.IsEmpty())
            {
                throw new DbxException(EnumCode.提示消息, isExist.Where(x => x.Name == input.Name).Count() > 0 ? Lang.sysMingChengYiCunZai : Lang.sysBianMaYiCunZai);
            }
        };

        public override Func<AppEditInput, AppEntity, AppEntity> UpdateBeforeCall => (input, entity) =>
        {
            var isExist = Find(predicate: x => x.Name == input.Name || x.Code == input.Code);
            if (!isExist.IsEmpty())
            {
                if (isExist.Where(x => x.Name == input.Name && x.Id != input.Id).Count() > 0)
                {
                    throw new DbxException(EnumCode.提示消息, Lang.sysMingChengYiCunZai);
                }
                if (isExist.Where(x => x.Code == input.Code && x.Id != input.Id).Count() > 0)
                {
                    throw new DbxException(EnumCode.提示消息, Lang.sysBianMaYiCunZai);
                }
            }

            entity.Name = input.Name;
            entity.AppType = input.AppType;
            entity.Code = input.Code;
            entity.Key = input.Key;
            entity.Secret = input.Secret;
            return entity;
        };

        public override Expression<Func<AppEntity, bool>> FindWhere(QueryInput input)
        {
            if (!input.IsNull() && !input.Query.IsNull())
            {
                var exp = ExpressHelper.Get<AppEntity>();

                var key = input.Query.GetString("key");
                var appType = EnumsHelper.Get<EnumAppType>(input.Query.GetInt("appType"));

                exp = exp.AndIF(!key.IsEmpty(), x => (x.Name).Contains(key));
                exp = exp.AndIF(appType != EnumAppType.Default, x => x.AppType == appType);

                return exp.ToExpression();
            }
            return base.FindWhere(input);

        }

        #endregion

        /// <summary>
        /// 获取应用信息
        /// </summary>
        /// <param name="appCode"></param>
        /// <returns></returns>
        public ApplicationModel GetModel(string appCode)
        {
            var app = Find(predicate: x => x.Code == appCode).FirstOrDefault();
            if (app.IsNull())
            {
                return null;
            }

            var appVersionService = EngineHelper.Resolve<IAppVersionService>();
            var appVersions = appVersionService.Find(where: x => x.AppId == app.Id);
            if (appVersions.IsEmpty())
            {
                return null;
            }

            var currentVersion = appVersions.OrderByDescending(x => x.No).FirstOrDefault();

            var model = EngineHelper.Map<ApplicationModel>(app);
            model.Versions = EngineHelper.Map<ApplicationVersion>(currentVersion);
            model.Versions.No = new Version(currentVersion.No);
            return model;
        }
    }
}
