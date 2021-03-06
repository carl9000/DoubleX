﻿namespace UTH.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UTH.Infrastructure.Resource;
    using UTH.Infrastructure.Resource.Culture;
    using UTH.Infrastructure.Utility;

    /// <summary>
    /// 领域配置管理器
    /// </summary>
    public class DomainProfileManager : IEnumerable<IDomainProfile>
    {
        public DomainProfileManager()
        {
        }

        private List<IDomainProfile> items = new List<IDomainProfile>();

        /// <summary>
        /// 组件集合
        /// </summary>
        public List<IDomainProfile> List { get { return items; } }

        /// <summary>
        /// 添加组件
        /// </summary>
        public void Add(IDomainProfile item)
        {
            items.Add(item);
        }

        /// <summary>
        /// 添加组件列表
        /// </summary>
        public void AddRange(IEnumerable<IDomainProfile> collection)
        {
            items.AddRange(collection);
        }

        /// <summary>
        /// 插入组件
        /// </summary>
        public void Insert(int index, IDomainProfile item)
        {
            items.Insert(index, item);
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        public void Remove(IDomainProfile item)
        {
            items.Remove(item);
        }

        /// <summary>
        /// 实现接口中的方法  
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IDomainProfile> GetEnumerator()
        {
            foreach (IDomainProfile item in items)
                yield return item;   //这里使用yield可以非常简化我们工作量不用我们自己去写一个继承IEnumerator 的类，这样编译器帮我做了很多事情  
        }

        //接口中的方法  
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
