using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using StatisticalAnalysisProduction.Infrastructure.Configuration;
using SqlServerDataAdapter;
namespace StatisticalAnalysisProduction.Service.RunIndicatorsStatisitics
{
    public class RunIndicatorsStatisitics
    {
        private static readonly string _connStr = ConnectionStringFactory.NXJCConnectionString;
        private static readonly ISqlServerDataFactory _dataFactory = new SqlServerDataFactory(_connStr);
        public static string GetEquipmentCommonInfo(string myOrganizationId)
        {
            string m_Sql = @"SELECT distinct A.EquipmentCommonId as EquipmentCommonId
                            ,A.Name as Name
                            ,A.VariableId as VariableId
							,A.DisplayIndex
                        FROM equipment_EquipmentCommonInfo A, equipment_EquipmentDetail B,system_MasterMachineDescription C, system_Organization D, system_Organization E
                        where A.EquipmentCommonId = B.EquipmentCommonId
	                    and B.Enabled = 1
                        and B.EquipmentId = C.Id
	                    {0}
                        and E.LevelCode like D.LevelCode + '%'
                        and C.OrganizationID = E.OrganizationID
                        order by A.DisplayIndex";
            if (myOrganizationId != "")
            {
                string m_Condtion = string.Format("and D.OrganizationID = '{0}' ", myOrganizationId);
                m_Sql = string.Format(m_Sql, m_Condtion);
            }
            else
            {
                m_Sql = string.Format(m_Sql, "");
            }

            try
            {
                DataTable m_EquipmentCommonInfoTable = _dataFactory.Query(m_Sql);
                string m_ReturnString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_EquipmentCommonInfoTable);
                return m_ReturnString;

            }
            catch
            {
                return "{\"rows\":[],\"total\":0}";
            }
        }
        public static string GetSpecificationsInfo(string myEquipmentCommonId)
        {
            string m_Sql = @"SELECT distinct A.Specifications as id, 
                                  A.Specifications as text
                                  FROM equipment_EquipmentDetail A,equipment_EquipmentCommonInfo B
                                  where A.Enabled = 1
                                  and A.Specifications is not null
                                  and A.Specifications <> ''
                                  and A.EquipmentCommonId = B.EquipmentCommonId
                                  and B.EquipmentCommonId = '{0}'";
            m_Sql = string.Format(m_Sql, myEquipmentCommonId);
            try
            {
                DataTable m_SpecificationsInfoTable = _dataFactory.Query(m_Sql);
                string m_ReturnString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_SpecificationsInfoTable);
                return m_ReturnString;

            }
            catch
            {
                return "{\"rows\":[],\"total\":0}";
            }
        }
        public static string GetRunindicatorsInfo(string myOrganizationId, string myEquipmentCommonId, string mySpecifications, string myRunindicatorsOrder, string myStartTime, string myEndTime)
        {
            string m_Specifications = mySpecifications == "All" ? "" : mySpecifications;
            string[] m_RunIndictorsList = new string[] {"运转率","可靠性","故障率","台时产量","运转时间", "计划检修时间" };
            DataTable m_RunIndictorsDetailTableFinally = null;
            DataTable m_OrganizationInfoTable = GetOrganizationInfo(myOrganizationId);
            if (m_OrganizationInfoTable != null)
            {
                for (int i = 0; i < m_OrganizationInfoTable.Rows.Count; i++)
                {
                    string m_FactoryOrganizationId = m_OrganizationInfoTable.Rows[i]["OrganizationId"].ToString();
                    DataTable m_RunIndictorsDetailTable = RunIndicators.EquipmentRunIndicators.GetEquipmentUtilizationByCommonId(m_RunIndictorsList, myEquipmentCommonId, m_Specifications, m_FactoryOrganizationId, myStartTime, myEndTime, _dataFactory);
                    if (m_RunIndictorsDetailTable != null)
                    {
                        m_RunIndictorsDetailTable.Columns.Add("OrganizationId", typeof(string));
                        m_RunIndictorsDetailTable.Columns.Add("OrganizationName", typeof(string));
                        m_RunIndictorsDetailTable.Columns.Add("OrganizationLevelCode", typeof(string));

                        if (m_RunIndictorsDetailTableFinally == null)            //当第一个不为空的表返回时,克隆该表
                        {
                            m_RunIndictorsDetailTableFinally = m_RunIndictorsDetailTable.Clone();
                        }

                        for (int j = 0; j < m_RunIndictorsDetailTable.Rows.Count; j++)
                        {
                            m_RunIndictorsDetailTable.Rows[j]["OrganizationId"] = m_OrganizationInfoTable.Rows[i]["OrganizationId"].ToString();
                            m_RunIndictorsDetailTable.Rows[j]["OrganizationName"] = m_OrganizationInfoTable.Rows[i]["OrganizationName"].ToString();
                            m_RunIndictorsDetailTable.Rows[j]["OrganizationLevelCode"] = m_OrganizationInfoTable.Rows[i]["OrganizationLevelCode"].ToString();
                            m_RunIndictorsDetailTable.Rows[j]["运转率"] = (decimal)m_RunIndictorsDetailTable.Rows[j]["运转率"] * 100;
                            m_RunIndictorsDetailTable.Rows[j]["可靠性"] = (decimal)m_RunIndictorsDetailTable.Rows[j]["可靠性"] * 100;
                            m_RunIndictorsDetailTable.Rows[j]["故障率"] = (decimal)m_RunIndictorsDetailTable.Rows[j]["故障率"] * 100;
                            m_RunIndictorsDetailTableFinally.Rows.Add(m_RunIndictorsDetailTable.Rows[j].ItemArray);
                        }
                    }

                }
            }
            if (m_RunIndictorsDetailTableFinally != null)
            {
                DataView m_RunIndictorsDetailTableFinallyView = m_RunIndictorsDetailTableFinally.DefaultView;
                if (myRunindicatorsOrder == "default")
                {
                    m_RunIndictorsDetailTableFinallyView.Sort = "OrganizationLevelCode, EquipmentName Asc";
                }
                else
                {
                    m_RunIndictorsDetailTableFinallyView.Sort = string.Format("{0} desc",myRunindicatorsOrder);
                }
                string m_ReturnString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_RunIndictorsDetailTableFinallyView.ToTable());
                return m_ReturnString;
            }
            else
            {
                return "{\"rows\":[],\"total\":0}";
            }
        }

        public static string GetMachineHaltInfo(string myOrganizationId, string myEquipmentCommonId, string mySpecifications, string myStartTime, string myEndTime)
        {
            string m_Specifications = mySpecifications == "All" ? "" : mySpecifications;
            DataTable m_MachineHaltInfoFinallyTable = null;
            DataTable m_OrganizationInfoTable = GetOrganizationInfo(myOrganizationId);
            if (m_OrganizationInfoTable != null)
            {
                for (int i = 0; i < m_OrganizationInfoTable.Rows.Count; i++)
                {
                    string m_FactoryOrganizationId = m_OrganizationInfoTable.Rows[i]["OrganizationId"].ToString();
                    DataTable m_RunIndictorsDetailH0Table = RunIndicators.EquipmentHalt.GetEquipmentHaltDetail(myEquipmentCommonId, m_Specifications, m_FactoryOrganizationId, "0", myStartTime, myEndTime, _dataFactory);
                    DataTable m_RunIndictorsDetailH24Table = RunIndicators.EquipmentHalt.GetEquipmentHaltDetail(myEquipmentCommonId, m_Specifications, m_FactoryOrganizationId, "24", myStartTime, myEndTime, _dataFactory);
                    DataTable m_RunIndictorsDetailH72Table = RunIndicators.EquipmentHalt.GetEquipmentHaltDetail(myEquipmentCommonId, m_Specifications, m_FactoryOrganizationId, "72", myStartTime, myEndTime, _dataFactory);
                    if (m_RunIndictorsDetailH0Table != null && m_RunIndictorsDetailH24Table != null && m_RunIndictorsDetailH72Table != null)
                    {
                        int m_RunIndicatorRowsCount = m_RunIndictorsDetailH0Table.Rows.Count;
                        for (int j = 0; j < m_RunIndictorsDetailH24Table.Rows.Count; j++)
                        {
                            m_RunIndictorsDetailH0Table.Rows.Add(m_RunIndictorsDetailH24Table.Rows[j].ItemArray);
                        }
                        for (int j = 0; j < m_RunIndictorsDetailH72Table.Rows.Count; j++)
                        {
                            m_RunIndictorsDetailH0Table.Rows.Add(m_RunIndictorsDetailH72Table.Rows[j].ItemArray);
                        }
                        m_RunIndictorsDetailH0Table.Columns.Add("StatisticsCycle", typeof(string));
                        for (int j = 0; j < m_RunIndicatorRowsCount; j++)
                        {
                            m_RunIndictorsDetailH0Table.Rows[j]["StatisticsCycle"] = "全部";
                        }
                        for (int j = m_RunIndicatorRowsCount; j < m_RunIndicatorRowsCount + m_RunIndictorsDetailH24Table.Rows.Count; j++)
                        {
                            m_RunIndictorsDetailH0Table.Rows[j]["StatisticsCycle"] = "24小时";
                        }
                        for (int j = m_RunIndicatorRowsCount + m_RunIndictorsDetailH24Table.Rows.Count; j < m_RunIndicatorRowsCount + m_RunIndictorsDetailH24Table.Rows.Count + m_RunIndictorsDetailH72Table.Rows.Count; j++)
                        {
                            m_RunIndictorsDetailH0Table.Rows[j]["StatisticsCycle"] = "72小时";
                        }
                        m_RunIndictorsDetailH0Table.Columns.Add("OrganizationId", typeof(string));
                        m_RunIndictorsDetailH0Table.Columns.Add("OrganizationName", typeof(string));
                        m_RunIndictorsDetailH0Table.Columns.Add("OrganizationLevelCode", typeof(string));

                        if (m_MachineHaltInfoFinallyTable == null)           //如果最终的Table为空则克隆第一个不为空的指标表 
                        {
                            m_MachineHaltInfoFinallyTable = m_RunIndictorsDetailH0Table.Clone();
                        }

                        for (int j = 0; j < m_RunIndictorsDetailH0Table.Rows.Count; j++)
                        {
                            m_RunIndictorsDetailH0Table.Rows[j]["OrganizationId"] = m_OrganizationInfoTable.Rows[i]["OrganizationId"].ToString();
                            m_RunIndictorsDetailH0Table.Rows[j]["OrganizationName"] = m_OrganizationInfoTable.Rows[i]["OrganizationName"].ToString();
                            m_RunIndictorsDetailH0Table.Rows[j]["OrganizationLevelCode"] = m_OrganizationInfoTable.Rows[i]["OrganizationLevelCode"].ToString();
                            m_MachineHaltInfoFinallyTable.Rows.Add(m_RunIndictorsDetailH0Table.Rows[j].ItemArray);
                        }
                    }

                }
            }
            DataView m_MachineHaltInfoFinallyView = m_MachineHaltInfoFinallyTable.DefaultView;
            m_MachineHaltInfoFinallyView.Sort = "OrganizationName, StatisticsCycle, EquipmentName asc";  
            string m_ReturnString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_MachineHaltInfoFinallyView.ToTable());
            return m_ReturnString;
        }
        private static DataTable GetOrganizationInfo(string myOrganizationId)
        {
            string m_Sql = @"SELECT B.OrganizationID as OrganizationId
	                              ,C.Name as OrganizationName
	                              ,B.LevelCode as OrganizationLevelCode
                              FROM system_Organization A, system_Organization B
                              left join system_Organization C on charindex(C.LevelCode,B.LevelCode)>0 and C.LevelType = 'Company'
                              where B.Enabled = 1
                              and B.LevelType = 'Factory'
                              {0}
                              order by A.LevelCode";
            if (myOrganizationId != "")
            {
                string m_Condition = string.Format("and A.OrganizationID = '{0}' and B.LevelCode like A.LevelCode + '%'", myOrganizationId);
                m_Sql = string.Format(m_Sql, m_Condition);
            }
            else
            {

                m_Sql = string.Format(m_Sql, "and A.LevelCode = B.LevelCode");
            }
            
            try
            {
                DataTable m_OrganizationInfoTable = _dataFactory.Query(m_Sql);
                return m_OrganizationInfoTable;
            }
            catch
            {
                return null;
            }
        }
    }
}
