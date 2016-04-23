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
                        FROM equipment_EquipmentCommonInfo A, equipment_EquipmentDetail B,system_MasterMachineDescription C, system_Organization D, system_Organization E
                        where A.EquipmentCommonId = B.EquipmentCommonId
	                    and B.Enabled = 1
                        and B.EquipmentId = C.Id
	                    and D.OrganizationID = '{0}'
                        and E.LevelCode like D.LevelCode + '%'
                        and C.OrganizationID = E.OrganizationID";
            m_Sql = string.Format(m_Sql, myOrganizationId);
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
        public static string GetRunindicatorsInfo(string myEquipmentCommonId, string myFactoryOrganizationId, string myStartTime, string myEndTime)
        {
            string[] m_RunIndictorsList = new string[] {"运转率","可靠性","故障率","台时产量","运转时间", "计划检修时间" };
            DataTable m_RunIndictorsDetailTable = RunIndicators.EquipmentRunIndicators.GetEquipmentUtilizationByCommonId(m_RunIndictorsList, myEquipmentCommonId, myFactoryOrganizationId, myStartTime, myEndTime, _dataFactory);
            string m_ReturnString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_RunIndictorsDetailTable);
            return m_ReturnString;
        }
        public static string GetMachineHaltInfo(string myEquipmentCommonId, string myFactoryOrganizationId, string myStartTime, string myEndTime)
        {
            DataTable m_RunIndictorsDetailH0Table = RunIndicators.EquipmentHalt.GetEquipmentHaltDetail(myEquipmentCommonId, myFactoryOrganizationId, "0", myStartTime, myEndTime, _dataFactory);
            DataTable m_RunIndictorsDetailH24Table = RunIndicators.EquipmentHalt.GetEquipmentHaltDetail(myEquipmentCommonId, myFactoryOrganizationId, "24", myStartTime, myEndTime, _dataFactory);
            DataTable m_RunIndictorsDetailH72Table = RunIndicators.EquipmentHalt.GetEquipmentHaltDetail(myEquipmentCommonId, myFactoryOrganizationId, "72", myStartTime, myEndTime, _dataFactory);

            if (m_RunIndictorsDetailH0Table != null && m_RunIndictorsDetailH24Table != null && m_RunIndictorsDetailH72Table != null)
            {
                int m_RunIndicatorRowsCount = m_RunIndictorsDetailH0Table.Rows.Count;
                for (int i = 0; i < m_RunIndictorsDetailH24Table.Rows.Count; i++)
                {
                    m_RunIndictorsDetailH0Table.Rows.Add(m_RunIndictorsDetailH24Table.Rows[i].ItemArray);
                }
                for (int i = 0; i < m_RunIndictorsDetailH72Table.Rows.Count; i++)
                {
                    m_RunIndictorsDetailH0Table.Rows.Add(m_RunIndictorsDetailH72Table.Rows[i].ItemArray);
                }
                m_RunIndictorsDetailH0Table.Columns.Add("StatisticsCycle", typeof(string));
                for (int i = 0; i < m_RunIndicatorRowsCount; i++)
                {
                    m_RunIndictorsDetailH0Table.Rows[i]["StatisticsCycle"] = "全部";
                }
                for (int i = m_RunIndicatorRowsCount; i < m_RunIndicatorRowsCount + m_RunIndictorsDetailH24Table.Rows.Count; i++)
                {
                    m_RunIndictorsDetailH0Table.Rows[i]["StatisticsCycle"] = "24小时";
                }
                for (int i = m_RunIndicatorRowsCount + m_RunIndictorsDetailH24Table.Rows.Count; i < m_RunIndicatorRowsCount + m_RunIndictorsDetailH24Table.Rows.Count + m_RunIndictorsDetailH72Table.Rows.Count; i++)
                {
                    m_RunIndictorsDetailH0Table.Rows[i]["StatisticsCycle"] = "72小时";
                }
            }

            string m_ReturnString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_RunIndictorsDetailH0Table);
            return m_ReturnString;
        }
    }
}
