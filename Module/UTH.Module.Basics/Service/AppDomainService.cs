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

    /// <summary>
    /// 应用程序领域服务
    /// </summary>
    public class AppDomainService : DomainDefaultService<IAppRepository, AppEntity>, IAppDomainService
    {
        public AppDomainService(IAppRepository repository, IApplicationSession session, ICachingService caching) :
            base(repository, session, caching)
        {
        }

        #region override

        protected override List<AppEntity> InsertBefore(List<AppEntity> inputs)
        {
            if (inputs.IsEmpty())
                return inputs;

            var names = inputs.Select(x => x.Name).ToList();
            var codes = inputs.Select(x => x.Code).ToList();

            var isExist = Any(x => names.Contains(x.Name) || codes.Contains(x.Code));
            if (isExist)
            {
                throw new DbxException(EnumCode.提示消息, Lang.sysBianHaoYiCunZai);
            }
            return inputs;
        }

        protected override List<AppEntity> UpdateBefore(List<AppEntity> inputs)
        {
            if (inputs.IsEmpty())
                return inputs;

            var ids = inputs.Select(x => x.Id).ToList();
            var names = inputs.Select(x => x.Name).ToList();
            var codes = inputs.Select(x => x.Code).ToList();

            var entitys = Find(where: x => ids.Contains(x.Id));
            var exists = Find(where: x => names.Contains(x.Name) || codes.Contains(x.Code));

            foreach (var entity in entitys)
            {
                var input = inputs.Where(x => x.Id == entity.Id).FirstOrDefault();
                if (input.IsNull())
                {
                    continue;
                }

                if (exists.Where(x => x.Name == input.Name && x.Id != input.Id).Count() > 0)
                {
                    throw new DbxException(EnumCode.提示消息, Lang.sysMingChengYiCunZai);
                }

                if (exists.Where(x => x.Code == input.Code && x.Id != input.Id).Count() > 0)
                {
                    throw new DbxException(EnumCode.提示消息, Lang.sysBianHaoYiCunZai);
                }

                entity.Name = input.Name;
                entity.AppType = input.AppType;
                entity.Code = input.Code;
                entity.Key = input.Key;
                entity.Secret = input.Secret;
            }

            return entitys;
        }

        #endregion
    }
}
