﻿namespace UTH.Module.User
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
    /// 账户领域服务
    /// </summary>
    public class AccountDomainService : DomainDefaultService<IAccountRepository, AccountEntity>, IAccountDomainService
    {
        public AccountDomainService(IAccountRepository repository, IApplicationSession session, ICachingService caching) :
            base(repository, session, caching)
        {
        }

        #region override

        protected override void DeleteBefore(List<Guid> ids)
        {
            //操作管理员不允许删除
            if (ids.Contains(GuidHelper.Get("79e775ec-c1f2-4865-883f-82d8ee777468")))
            {
                throw new DbxException(EnumCode.提示消息, Lang.userChaoJiGuanLiYuanBuYunXuCaoZuo);
            }
            base.DeleteBefore(ids);
        }

        #endregion

        /// <summary>
        /// 获取账户
        /// </summary>
        /// <param name="account"></param>
        /// <param name="mobile"></param>
        /// <param name="email"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public AccountEntity Get(string account = null, string mobile = null, string email = null, string userName = null, string no = null)
        {
            if (userName.IsEmpty() && (account + email + mobile).IsEmpty())
            {
                return null;
            }

            AccountEntity entity = null;

            if (entity.IsNull() && (!userName.IsEmpty() || !account.IsEmpty()))
            {
                var value = (!account.IsEmpty() ? account : userName).ToUpper();
                entity = Get(x => x.NormalizedAccount == value);
            }

            if (entity.IsNull() && (!userName.IsEmpty() || !mobile.IsEmpty()))
            {
                var value = (!mobile.IsEmpty() ? mobile : userName).ToUpper();
                entity = Get(x => x.Mobile == value && x.MobileAuth);
            }

            if (entity.IsNull() && (!userName.IsEmpty() || !email.IsEmpty()))
            {
                var value = (!email.IsEmpty() ? email : userName).ToUpper();
                entity = Get(x => x.NormalizedEmail == value && x.EmailAuth);
            }

            if (entity.IsNull() && !no.IsEmpty())
            {
                entity = Get(x => x.No == no);
            }

            return entity;
        }

        /// <summary>
        /// 创建账户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="account"></param>
        /// <param name="mobile"></param>
        /// <param name="email"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="organize"></param>
        /// <param name="employe"></param>
        /// <returns></returns>
        public AccountEntity Create(Guid id, string account, string mobile, string email, string userName, string password, string organize, string employe, bool isAdmin)
        {
            return Create(new AccountEntity()
            {
                Id = id,
                Account = account,
                Mobile = mobile,
                Email = email,
                Password = password,
                OrganizeCode = organize,
                EmployeCode = employe,
                Type = isAdmin ? EnumAccountType.管理员 : EnumAccountType.Default
            });
        }

        /// <summary>
        /// 创建账户
        /// </summary>
        /// <param name="input"></param>
        /// <param name="organize"></param>
        /// <returns></returns>
        public AccountEntity Create(AccountEntity input)
        {
            input.CheckNull();

            input.Id = !input.Id.IsEmpty() ? input.Id : Guid.NewGuid();

            input.Type = input.Type == EnumAccountType.Default ? EnumAccountType.个人用户 : input.Type;

            if (!input.OrganizeCode.IsEmpty())
            {
                input.Type = input.EmployeCode.IsEmpty() ? EnumAccountType.组织用户 : EnumAccountType.组织成员;
            }

            if (input.Type == EnumAccountType.组织用户)
            {
                input.OrganizeCode.CheckEmpty();
                input.Account = !input.Account.IsEmpty() ? input.Account : input.OrganizeCode;
            }

            if (input.Type == EnumAccountType.组织成员)
            {
                input.OrganizeCode.CheckEmpty();
                input.EmployeCode.CheckEmpty();
                input.Account = !input.Account.IsEmpty() ? input.Account : $"{input.EmployeCode}@{input.OrganizeCode}";
            }

            if (!Get(input.Account, input.Mobile, input.Email).IsNull())
            {
                throw new DbxException(EnumCode.提示消息, Lang.userZhangHuYiCunZai);
            }


            input.Account = !input.Account.IsEmpty() ? input.Account : $"AUTO_{GuidHelper.GetToString(input.Id, removeSplit: true, isCaseUpper: true)}";

            input.Account.CheckEmpty();
            input.Password.CheckEmpty();

            input.No = GetMaxNo(input.Type);
            input.Mobile = input.Mobile;
            input.Email = input.Email;

            input.Password = HashSecurityHelper.GetSecurity(input.Password);

            input.MobileAuth = false;
            input.EmailAuth = false;
            input.CertificateAuth = false;

            //DO:使用手机号码，邮箱地址注册，需进行校验码验证，才能进入该方法，考虑使用AOP(CODE+接收者+类型)对Input输入继承ICodeCheckService 进行校验
            if (!input.Mobile.IsEmpty())
            {
                input.MobileAuth = true;
            }
            if (!input.Email.IsEmpty())
            {
                input.EmailAuth = true;
            }

            input.NormalizedAccount = !input.Account.IsEmpty() ? input.Account.ToUpper() : "";
            input.NormalizedEmail = !input.Email.IsEmpty() ? input.Email.ToUpper() : "";

            input.IsTwoFactorEnabled = false;
            input.IsLockoutEnabled = false;
            input.AccessFailedCount = 0;
            input.InviterId = Guid.Empty;

            input.LoginCount = 0;
            input.LoginLastIp = "127.0.0.1";
            input.LoginLastDt = DateTimeHelper.DefaultDateTime;

            input.Status = input.Status == EnumAccountStatus.Default ? EnumAccountStatus.正常 : input.Status;
            if (Insert(input).IsNull())
            {
                throw new DbxException(EnumCode.提示消息, Lang.userZhuCeShiBai);
            }

            return input;
        }

        /// <summary>
        /// 账号登录
        /// </summary>
        /// <param name="password"></param>
        /// <param name="account"></param>
        /// <param name="mobile"></param>
        /// <param name="email"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public AccountEntity Login(string account, string mobile, string email, string userName, string password, string organize)
        {
            var entity = Get(account, mobile, email, userName);
            if (entity.IsNull())
            {
                throw new DbxException(EnumCode.提示消息, Lang.userZhangHaoCuoWu);
            }

            if (!HashSecurityHelper.VerifySecurity(entity.Password, password))
            {
                throw new DbxException(EnumCode.提示消息, Lang.userMiMaCuoWu);
            }

            entity.LoginCount++;
            entity.LoginLastDt = DateTime.Now;
            entity.LoginLastIp = Session.Accessor.ClientIp;
            Update(entity);

            return entity;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newPassword"></param>
        /// <param name="oldPassword"></param>
        /// <param name="key"></param>
        /// <param name="mobile"></param>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public AccountEntity EditPwd(Guid id, string newPassword, string oldPassword, string key, string mobile, string email, string code)
        {
            if (oldPassword.IsEmpty() && (mobile + email + code).IsEmpty())
            {
                throw new DbxException(EnumCode.提示消息, Lang.sysJiaoYanShiBai);
            }

            var account = Get(id);
            account.CheckNull();

            //修改认证方式1：旧密码
            if (!oldPassword.IsEmpty())
            {
                if (!HashSecurityHelper.VerifySecurity(account.Password, oldPassword))
                {
                    throw new DbxException(EnumCode.提示消息, Lang.userQingShuRuYouXiaoDeYuanMiMa);
                }
                account.Password = HashSecurityHelper.GetSecurity(newPassword);
                return Update(account);
            }

            //修改认证方式2：手机/邮箱认证码
            if (!(mobile + email + code).IsEmpty())
            {
                var input = new CaptchaInput();
                input.Category = EnumCaptchaCategory.FindPwd;
                input.Mode = EnumCaptchaMode.Sms;
                input.Key = key;
                input.Code = code;
                input.Receiver = mobile;

                if (!email.IsEmpty())
                {
                    input.Mode = EnumCaptchaMode.Email;
                    input.Receiver = email;
                }
            }

            throw new DbxException(EnumCode.提示消息, Lang.sysJiaoYanShiBai);
        }

        /// <summary>
        /// 获取编号
        /// 账户编号根据类型自加
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetMaxNo(EnumAccountType type)
        {
            var maxNum = Max<string>(field: x => x.No, where: x => x.Type == type);
            if (maxNum.IsEmpty())
            {
                return $"{type.GetValue()}00000000";
            }
            else
            {
                return StringHelper.Get(IntHelper.Get(maxNum) + 1);
            }
        }
    }
}
