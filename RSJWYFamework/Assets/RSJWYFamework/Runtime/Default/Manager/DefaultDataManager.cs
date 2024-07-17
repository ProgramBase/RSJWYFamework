using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RSJWYFamework.Runtime.Data;
using RSJWYFamework.Runtime.Data.Base;
using RSJWYFamework.Runtime.ExceptionLogManager;
using RSJWYFamework.Runtime.Main;
using RSJWYFamework.Runtime.Module;

namespace RSJWYFamework.Runtime.Default.Manager
{
    public class DefaultDataManager:IDataManager, IModule 
    {
        /// <summary>
        /// 数据集，基于ScriptableObject
        /// </summary>
        private ConcurrentDictionary<Type, List<DataBaseSB>> dataSBDic;
        /// <summary>
        /// 数据集，基于DataBase
        /// </summary>
        private ConcurrentDictionary<Type, List<DataBase>> dataDic;
        public void Init()
        {
            dataSBDic = new ConcurrentDictionary<Type, List<DataBaseSB>>();
            dataDic = new ConcurrentDictionary<Type, List<DataBase>>();
        }

        public void Close()
        {
            dataSBDic.Clear();
            dataDic.Clear();
        }

        public void Update(float time, float deltaTime)
        {
            
        }

        public void UpdatePerSecond(float time)
        {
            
        }

        public void AddDataSet(DataBaseSB dataSet)
        {
            if (dataSet == null)
            {
                throw new RSJWYException(RSJWYFameworkEnum.Data, $"数据为空");
            }
               

            Type type = dataSet.GetType();
            if (!dataSBDic.ContainsKey(type))
            {
                dataSBDic.TryAdd(type, new List<DataBaseSB>());
            }
            dataSBDic[type].Add(dataSet);
        }

        public void AddDataSet(DataBase dataSet)
        {
            if (dataSet == null)
                return;

            Type type = dataSet.GetType();
            if (!dataDic.ContainsKey(type))
            {
                dataDic.TryAdd(type, new List<DataBase>());
            }
            if (dataDic[type].Contains(dataSet))
            {
                dataDic[type].Remove(dataSet);
            }
        }

        public void RemoveDataSet(DataBaseSB dataSet)
        {
            if (dataSet == null)
                return;

            Type type = dataSet.GetType();
            if (!dataSBDic.ContainsKey(type))
            {
                dataSBDic.TryAdd(type, new List<DataBaseSB>());
            }
            dataSBDic[type].Add(dataSet);
        }

        public void RemoveDataSet(DataBase dataSet)
        {
            if (dataSet == null)
                return;

            Type type = dataSet.GetType();
            if (!dataDic.ContainsKey(type))
            {
                dataDic.TryAdd(type, new List<DataBase>());
            }
            if (dataDic[type].Contains(dataSet))
            {
                dataDic[type].Remove(dataSet);
            }
        }

        public List<DataBaseSB> GetAllDataSetsSB(Type type)
        {
            if (dataSBDic.ContainsKey(type))
            {
                return dataSBDic[type];
            }
            throw new RSJWYException(RSJWYFameworkEnum.Data, $"获取所有数据集失败：{type.Name} 并不是有效的数据集类型！");
        }

        public List<DataBase> GetAllDataSets(Type type)
        {
            if (dataDic.ContainsKey(type))
            {
                return dataDic[type];
            }
            throw new RSJWYException(RSJWYFameworkEnum.Data, $"获取所有数据集失败：{type.Name} 并不是有效的数据集类型！");
        }

        public List<DataBase> GetAllDataSets(Type type, Predicate<DataBase> match)
        {
            if (dataDic.ContainsKey(type))
            {
                return dataDic[type].FindAll(match);
            }
            throw new RSJWYException(RSJWYFameworkEnum.Data, $"获取所有数据集失败：{type.Name} 并不是有效的数据集类型！");
        }

        public List<DataBaseSB> GetAllDataSets(Type type, Predicate<DataBaseSB> match)
        {
            if (dataSBDic.ContainsKey(type))
            {
                return dataSBDic[type].FindAll(match);
            }
            throw new RSJWYException(RSJWYFameworkEnum.Data, $"获取所有数据集失败：{type.Name} 并不是有效的数据集类型！");
        }

        public DataBase GetDataSet(Type type, Predicate<DataBase> match, bool isCut = false)
        {
            if (dataDic.ContainsKey(type))
            {
                var dataset = dataDic[type].Find(match);
                if (isCut && dataset!=null)
                {
                    dataDic[type].Remove(dataset);
                }
                return dataset;
            }
            throw new RSJWYException(RSJWYFameworkEnum.Data, $"获取所有数据集失败：{type.Name} 并不是有效的数据集类型！");
        }

        public DataBaseSB GetDataSet(Type type, Predicate<DataBaseSB> match, bool isCut = false)
        {
            if (dataSBDic.ContainsKey(type))
            {
                var dataset = dataSBDic[type].Find(match);
                if (isCut && dataset)
                {
                    dataSBDic[type].Remove(dataset);
                }
                return dataset;
            }
            throw new RSJWYException(RSJWYFameworkEnum.Data, $"获取所有数据集失败：{type.Name} 并不是有效的数据集类型！");
        }

        public DataBase GetDataSet(Type type, bool isCut = false)
        {
            if (dataDic.ContainsKey(type))
            {
                if (dataDic[type].Count > 0)
                {
                    var dataset = dataDic[type][0];
                    if (isCut)
                    {
                        dataDic[type].RemoveAt(0);
                    }
                    return dataset;
                }
                else
                {
                    return null;
                }
            }
            throw new RSJWYException(RSJWYFameworkEnum.Data, $"获取所有数据集失败：{type.Name} 并不是有效的数据集类型！");
        }

        public TDataBase GetDataSetSB<TDataBase>( bool isCut = false)where TDataBase :DataBaseSB
        {
            var type = typeof(TDataBase);
            if (dataSBDic.ContainsKey(type))
            {
                if (dataSBDic[type].Count > 0)
                {
                    var dataset = dataSBDic[type][0];
                    if (isCut)
                    {
                        dataSBDic[type].RemoveAt(0);
                    }
                    return (TDataBase)dataset;
                }
                else
                {
                    return null;
                }
            }
            throw new RSJWYException(RSJWYFameworkEnum.Data, $"获取所有数据集失败：{type.Name} 并不是有效的数据集类型！");
        }

        public DataBase GetDataSet(Type type, int index, bool isCut = false)
        {
            if (dataDic.ContainsKey(type))
            {
                if (index >= 0 && index < dataDic[type].Count)
                {
                    var dataset = dataDic[type][index];
                    if (isCut)
                    {
                        dataDic[type].RemoveAt(index);
                    }
                    return dataset;
                }
                else
                {
                    return null;
                }
            }
            throw new RSJWYException(RSJWYFameworkEnum.Data, $"获取所有数据集失败：{type.Name} 并不是有效的数据集类型！");
        }

        public DataBaseSB GetDataSetSB(Type type, int index, bool isCut = false)
        {
            if (dataSBDic.ContainsKey(type))
            {
                if (index >= 0 && index < dataSBDic[type].Count)
                {
                    var dataset = dataSBDic[type][index];
                    if (isCut)
                    {
                        dataSBDic[type].RemoveAt(index);
                    }
                    return dataset;
                }
                else
                {
                    return null;
                }
            }
            throw new RSJWYException(RSJWYFameworkEnum.Data, $"获取所有数据集失败：{type.Name} 并不是有效的数据集类型！");
        }

        public int GetCount(Type type)
        {
            /*if (DataSets.ContainsKey(type))
            {
                return DataSets[type].Count;
            }
            else
            {
                throw new HTFrameworkException(HTFrameworkModule.DataSet, $"获取数据集数量失败：{type.Name} 并不是有效的数据集类型！");
            }*/
            return default;
        }

        public void ClearDataSet(Type type)
        {
            /*if (DataSets.ContainsKey(type))
            {
                return DataSets[type].Count;
            }
            else
            {
                throw new HTFrameworkException(HTFrameworkModule.DataSet, $"获取数据集数量失败：{type.Name} 并不是有效的数据集类型！");
            }*/
        }
    }
}