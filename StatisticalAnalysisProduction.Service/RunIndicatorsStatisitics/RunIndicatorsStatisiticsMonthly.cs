using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using StatisticalAnalysisProduction.Infrastructure.Configuration;
using SqlServerDataAdapter;

namespace StatisticalAnalysisProduction.Service.RunIndicatorsStatisitics
{
    public class RunIndicatorsStatisiticsMonthly
    {
        private static readonly string _connStr = ConnectionStringFactory.NXJCConnectionString;
        private static readonly ISqlServerDataFactory _dataFactory = new SqlServerDataFactory(_connStr);
        public static string GetEquipmentInfo(string myOrganizationId)
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

        public static string GetRunindicatorsAndHaltInfo(string myEquipmentCommonId, string myOrganizationId, string myStartTime, string myEndTime)
        {
            string[] m_RunIndictorsList = new string[] { "运转率", "可靠性", "故障率", "台时产量", "运转时间", "计划检修时间" };
            string[] m_MasterMachineHaltList = new string[] { "总停机", "24小时停机", "72小时停机" };
            string[] m_MasterMachineHaltListTime = new string[] { "0", "24", "72" };
            
            DataTable m_RunindicatorsAndHaltTable = GetRunindicatorsAndHaltTable();
            GetRunIndictorsValue(m_RunIndictorsList, ref m_RunindicatorsAndHaltTable, myEquipmentCommonId, myOrganizationId, myStartTime, myEndTime);
            GetMachineHaltValue(m_MasterMachineHaltList, m_MasterMachineHaltListTime, ref m_RunindicatorsAndHaltTable, myEquipmentCommonId, myOrganizationId, myStartTime, myEndTime);
            string m_ReturnString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_RunindicatorsAndHaltTable);
            return m_ReturnString;
        }
        private static void GetRunIndictorsValue(string[] myRunIndictorsList, ref DataTable myRunindicatorsAndHaltTable, string myEquipmentCommonId, string myOrganizationId, string myStartTime, string myEndTime)
        {
            if (myRunIndictorsList != null)
            {
                for (int i = 0; i < myRunIndictorsList.Length; i++)
                {
                    DataRow m_RunindicatorRow = myRunindicatorsAndHaltTable.NewRow();
                    m_RunindicatorRow[0] = myRunIndictorsList[i];
                    m_RunindicatorRow[1] = myRunIndictorsList[i];
                    m_RunindicatorRow["Type"] = "RunIndictors";
                    
                    DataTable m_RunIndictorsDetailTable = RunIndicators.EquipmentRunIndicators.GetEquipmentCommonUtilizationPerMonth(myRunIndictorsList[i], myEquipmentCommonId, myOrganizationId, myStartTime, myEndTime, _dataFactory);
                    if (m_RunIndictorsDetailTable != null && m_RunIndictorsDetailTable.Rows.Count > 0)
                    {
                        for (int j = 2; j <= 13; j++)
                        {
                            m_RunindicatorRow[j] = m_RunIndictorsDetailTable.Rows[0][j];
                        }
                    }
                    myRunindicatorsAndHaltTable.Rows.Add(m_RunindicatorRow);
                }
            }
        }
        private static void GetMachineHaltValue(string[] myMasterMachineHaltList, string[] myMasterMachineHaltListTime, ref DataTable myRunindicatorsAndHaltTable, string myEquipmentCommonId, string myOrganizationId, string myStartTime, string myEndTime)
        {
            if (myMasterMachineHaltList != null)
            {
                DateTime m_CurrentStartTime = DateTime.Parse(myStartTime);
                DateTime m_CurrentEndTime = m_CurrentStartTime.AddMonths(1).AddDays(-1);   //本月的最后一天
                for (int i = 0; i < myMasterMachineHaltList.Length; i++)
                {
                    DataRow m_MachineHaltDownTimeRow = myRunindicatorsAndHaltTable.NewRow();
                    DataRow m_MachineHaltDownCountRow = myRunindicatorsAndHaltTable.NewRow();
                    for (int j = 0; j < 12; j++)
                    {
                        m_MachineHaltDownTimeRow[0] = myMasterMachineHaltList[i];
                        m_MachineHaltDownTimeRow[1] = myMasterMachineHaltList[i] + "时间";
                        m_MachineHaltDownTimeRow["ShowDetail"] = "ShowDetail";
                        m_MachineHaltDownTimeRow["Type"] = "MachineHalt";
                        m_MachineHaltDownCountRow[0] = myMasterMachineHaltList[i];
                        m_MachineHaltDownCountRow[1] = myMasterMachineHaltList[i] + "次数";
                        m_MachineHaltDownCountRow["ShowDetail"] = "ShowDetail";
                        m_MachineHaltDownCountRow["Type"] = "MachineHalt";
                        DataTable m_RunIndictorsDetailTable = RunIndicators.EquipmentHalt.GetEquipmentHalt(new string[] { myEquipmentCommonId}, myOrganizationId, m_CurrentStartTime.AddMonths(j).ToString("yyyy-MM-dd"), m_CurrentStartTime.AddMonths(j + 1).AddDays(-1).ToString("yyyy-MM-dd"), myMasterMachineHaltListTime[i], _dataFactory);
                        if (m_RunIndictorsDetailTable != null && m_RunIndictorsDetailTable.Rows.Count > 0)
                        {
                            m_MachineHaltDownTimeRow[j + 2] = m_RunIndictorsDetailTable.Rows[0]["DowntimeTime"];
                            m_MachineHaltDownCountRow[j + 2] = m_RunIndictorsDetailTable.Rows[0]["DowntimeCount"];
                            
                        }
                    }
                    myRunindicatorsAndHaltTable.Rows.Add(m_MachineHaltDownTimeRow);
                    myRunindicatorsAndHaltTable.Rows.Add(m_MachineHaltDownCountRow);
                }
            }
        }
        private static DataTable GetRunindicatorsAndHaltTable()
        {
            DataTable m_RunindicatorsAndHaltTable = new DataTable();
            m_RunindicatorsAndHaltTable.Columns.Add("QuotasID", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("QuotasName", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("01", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("02", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("03", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("04", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("05", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("06", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("07", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("08", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("09", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("10", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("11", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("12", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("Type", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("ShowDetail", typeof(string));
            //if (myRunIndictorsList != null)             //初始化运行指标
            //{
            //    for (int i = 0; i < myRunIndictorsList.Length; i++)
            //    {
            //        m_RunindicatorsAndHaltTable.Rows.Add(myRunIndictorsList[i], myRunIndictorsList[i], 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, "RunIndictors", "");
            //    }
            //}
            //if (myMasterMachineHaltList != null)       //初始化故障统计
            //{
            //    for (int i = 0; i < myMasterMachineHaltList.Length; i++)
            //    {
            //        m_RunindicatorsAndHaltTable.Rows.Add(myRunIndictorsList[i], myRunIndictorsList[i], 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, 0.0m, "MachineHalt", "ShowDetail");
            //    }
            //}
            return m_RunindicatorsAndHaltTable;
        }
    }
}
