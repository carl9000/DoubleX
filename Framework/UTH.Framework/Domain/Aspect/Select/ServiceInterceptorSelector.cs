﻿namespace UTH.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Reflection;
    using Castle.DynamicProxy;
    using FluentValidation;
    using UTH.Infrastructure.Resource;
    using UTH.Infrastructure.Resource.Culture;
    using UTH.Infrastructure.Utility;

    /// <summary>
    /// 业务拦截选择（判断方法需调用什么拦截器(Interceptor)? ）
    /// IInterceptorSelector接口决定某个方法该运用哪些拦截器，它在每次调用被拦截的方法时执行
    /// </summary>
    public class ServiceInterceptorSelector : IInterceptorSelector
    {
        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            if (!interceptors.IsEmpty())
            {
                return interceptors.Where(i => i is IServiceInterceptor).ToArray();
            }
            return interceptors;
        }
    }
}