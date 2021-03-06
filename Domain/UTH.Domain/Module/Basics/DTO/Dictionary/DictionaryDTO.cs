﻿namespace UTH.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.ComponentModel;
    using FluentValidation;
    using UTH.Infrastructure.Resource.Culture;
    using UTH.Infrastructure.Utility;
    using UTH.Framework;

    /// <summary>
    /// 数据字典基本信息(DTO)
    /// </summary>
    [Serializable]
    public class DictionaryDTO : IKeys, IOutput
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Ids
        /// </summary>
        public List<Guid> Ids { get; set; }

        /// <summary>
        /// 所属分类
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 父值
        /// </summary>
        public string Parent { get; set; }

        /// <summary>
        /// 关联值
        /// </summary>
        public string Relation { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Sort { get; set; }

    }
}
