using System;
using System.Collections.Generic;
using MyFamework.Runtime.Base;
using RSJWYFamework.Runtime.Data.Base;

namespace RSJWYFamework.Runtime.Data
{
    /// <summary>
    /// 数据管理类
    /// </summary>
    public interface IDataManager:ModleInterface
    {
        /// <summary>
        /// 添加数据集至数据集仓库
        /// </summary>
        /// <param name="dataSet">数据集</param>
        void AddDataSet(DataBaseSB dataSet);
        /// <summary>
        /// 添加数据集至数据集仓库
        /// </summary>
        /// <param name="dataSet">数据集</param>
        void AddDataSet(DataBase dataSet);
        /// <summary>
        /// 从数据集仓库中移除数据集
        /// </summary>
        /// <param name="dataSet">数据集</param>
        void RemoveDataSet(DataBaseSB dataSet);
        
        /// <summary>
        /// 从数据集仓库中移除数据集
        /// </summary>
        /// <param name="dataSet">数据集</param>
        void RemoveDataSet(DataBase dataSet);
        
        /// <summary>
        /// 获取某一类型的所有数据集
        /// </summary>
        /// <param name="type">数据集类型</param>
        /// <returns>数据集列表</returns>
        List<DataBaseSB> GetAllDataSetsSB(Type type);
        
        /// <summary>
        /// 获取某一类型的所有数据集
        /// </summary>
        /// <param name="type">数据集类型</param>
        /// <returns>数据集列表</returns>
        List<DataBase> GetAllDataSets(Type type);
        /// <summary>
        /// 获取某一类型的满足匹配条件的所有数据集
        /// </summary>
        /// <param name="type">数据集类型</param>
        /// <param name="match">匹配条件</param>
        /// <returns>数据集列表</returns>
        List<DataBase> GetAllDataSets(Type type, Predicate<DataBase> match);
        
        /// <summary>
        /// 获取某一类型的满足匹配条件的所有数据集
        /// </summary>
        /// <param name="type">数据集类型</param>
        /// <param name="match">匹配条件</param>
        /// <returns>数据集列表</returns>
        List<DataBaseSB> GetAllDataSets(Type type, Predicate<DataBaseSB> match);

        /// <summary>
        /// 获取某一类型的满足匹配条件的第一条数据集
        /// </summary>
        /// <param name="type">数据集类型</param>
        /// <param name="match">匹配条件</param>
        /// <param name="isCut">是否同时在数据集仓库中移除该数据集</param>
        /// <returns>数据集</returns>
        DataBase GetDataSet(Type type, Predicate<DataBase> match, bool isCut = false);
        
        /// <summary>
        /// 获取某一类型的满足匹配条件的第一条数据集
        /// </summary>
        /// <param name="type">数据集类型</param>
        /// <param name="match">匹配条件</param>
        /// <param name="isCut">是否同时在数据集仓库中移除该数据集</param>
        /// <returns>数据集</returns>
        DataBaseSB GetDataSet(Type type, Predicate<DataBaseSB> match, bool isCut = false);
        
        /// <summary>
        /// 根据先后顺序获取某一类型的第一条数据集
        /// </summary>
        /// <param name="type">数据集类型</param>
        /// <param name="isCut">是否同时在数据集仓库中移除该数据集</param>
        /// <returns>数据集</returns>
        DataBase GetDataSet(Type type, bool isCut = false);
        
        
        /// <summary>
        /// 根据先后顺序获取某一类型的第一条数据集
        /// </summary>
        /// <param name="isCut">是否同时在数据集仓库中移除该数据集</param>
        /// <typeparam name="TDataBase">类型，继承自DataBaseSB</typeparam>
        /// <returns></returns>
        TDataBase GetDataSetSB<TDataBase>(bool isCut = false)where TDataBase :DataBaseSB;
        
        /// <summary>
        /// 根据索引获取某一类型的一条数据集
        /// </summary>
        /// <param name="type">数据集类型</param>
        /// <param name="index">索引</param>
        /// <param name="isCut">是否同时在数据集仓库中移除该数据集</param>
        /// <returns>数据集</returns>
        DataBase GetDataSet(Type type, int index, bool isCut = false);
        
        
        /// <summary>
        /// 根据索引获取某一类型的一条数据集
        /// </summary>
        /// <param name="type">数据集类型</param>
        /// <param name="index">索引</param>
        /// <param name="isCut">是否同时在数据集仓库中移除该数据集</param>
        /// <returns>数据集</returns>
        DataBaseSB GetDataSetSB(Type type, int index, bool isCut = false);

        /// <summary>
        /// 获取数据集仓库中某一类型的数据集数量
        /// </summary>
        /// <param name="type">数据集类型</param>
        /// <returns>数据集数量</returns>
        int GetCount(Type type);
        
        /// <summary>
        /// 清空某一类型的数据集仓库
        /// </summary>
        /// <param name="type">数据集类型</param>
        void ClearDataSet(Type type);
    }
}