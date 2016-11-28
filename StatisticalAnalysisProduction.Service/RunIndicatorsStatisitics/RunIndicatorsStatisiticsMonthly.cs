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
        public static string GetEquipmentInfoByEquipmentCommonId(string myEquipmentCommonId, string myOrganizationId)
        {
            string m_Sql = @"SELECT A.EquipmentId as EquipmentId
                            ,A.EquipmentName as EquipmentName
                            ,A.VariableId as VariableId
                        FROM equipment_EquipmentDetail A,system_MasterMachineDescription B, system_Organization C, system_Organization D
                        where A.EquipmentCommonId = '{0}'
	                    and A.Enabled = 1
                        and A.EquipmentId = B.Id
						and C.OrganizationID = '{1}'
                        and D.LevelCode like C.LevelCode + '%'
                        and A.OrganizationID = D.OrganizationID
						order by A.EquipmentName";
            m_Sql = string.Format(m_Sql, myEquipmentCommonId, myOrganizationId);
            try
            {
                DataTable m_EquipmentInfoTable = _dataFactory.Query(m_Sql);
                string m_ReturnString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_EquipmentInfoTable);
                return m_ReturnString;

            }
            catch
            {
                return "{\"rows\":[],\"total\":0}";
            }
        }
        public static string GetRunindicatorsAndHaltInfo(string myEquipmentId, string myOrganizationId, string myStartTime, string myEndTime)
        {
            string[] m_RunIndictorsList = new string[] { "运转率", "可靠性", "故障率", "台时产量", "运转时间", "计划检修时间" };
            string[] m_MasterMachineHaltList = new string[] { "总停机", "24小时停机", "72小时停机" };
            string[] m_MasterMachineHaltListTime = new string[] { "0", "24", "72" };

            DataTable m_RunindicatorsAndHaltTable = GetRunindicatorsAndHaltTable();
            GetRunIndictorsValue(m_RunIndictorsList, ref m_RunindicatorsAndHaltTable, myEquipmentId, myOrganizationId, myStartTime, myEndTime);
            GetMachineHaltValue(m_MasterMachineHaltList, m_MasterMachineHaltListTime, ref m_RunindicatorsAndHaltTable, myEquipmentId, myOrganizationId, myStartTime, myEndTime);
            string m_ReturnString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_RunindicatorsAndHaltTable);
            return m_ReturnString;
        }
        private static void GetRunIndictorsValue(string[] myRunIndictorsList, ref DataTable myRunindicatorsAndHaltTable, string myEquipmentId, string myOrganizationId, string myStartTime, string myEndTime)
        {
            if (myRunIndictorsList != null)
            {
                int m_MaxMonth = DateTime.Parse(myEndTime).Month;
                for (int i = 0; i < myRunIndictorsList.Length; i++)
                {
                    DataRow m_RunindicatorRow = myRunindicatorsAndHaltTable.NewRow();
                    m_RunindicatorRow[0] = myRunIndictorsList[i];
                    m_RunindicatorRow[1] = myRunIndictorsList[i];
                    m_RunindicatorRow["Type"] = "RunIndictors";

                    DataTable m_RunIndictorsDetailTable = RunIndicators.EquipmentRunIndicators.GetEquipmentUtilizationPerMonth(myRunIndictorsList[i], myOrganizationId, myEquipmentId, myStartTime, myEndTime, _dataFactory);
                    if (m_RunIndictorsDetailTable != null && m_RunIndictorsDetailTable.Rows.Count > 0)
                    {
                        for (int j = 2; j < m_MaxMonth + 2; j++)
                        {
                            if (myRunIndictorsList[i] == "运转率" || myRunIndictorsList[i] == "可靠性" || myRunIndictorsList[i] == "故障率")
                            {
                                m_RunindicatorRow[j] = m_RunIndictorsDetailTable.Rows[0][j] != DBNull.Value ? (decimal)m_RunIndictorsDetailTable.Rows[0][j] * 100 : 0;
                            }
                            else
                            {
                                m_RunindicatorRow[j] = m_RunIndictorsDetailTable.Rows[0][j] != DBNull.Value ? (decimal)m_RunIndictorsDetailTable.Rows[0][j] : 0;
                            }
                        }
                    }
                    myRunindicatorsAndHaltTable.Rows.Add(m_RunindicatorRow);
                }
            }
        }
        private static void GetMachineHaltValue(string[] myMasterMachineHaltList, string[] myMasterMachineHaltListTime, ref DataTable myRunindicatorsAndHaltTable, string myEquipmentId, string myOrganizationId, string myStartTime, string myEndTime)
        {
            if (myMasterMachineHaltList != null)
            {
                int m_MaxMonth = DateTime.Parse(myEndTime).Month;
                DateTime m_CurrentStartTime = DateTime.Parse(myStartTime);
                for (int i = 0; i < myMasterMachineHaltList.Length; i++)
                {
                    DataRow m_MachineHaltDownTimeRow = myRunindicatorsAndHaltTable.NewRow();
                    DataRow m_MachineHaltDownCountRow = myRunindicatorsAndHaltTable.NewRow();
                    for (int j = 0; j < m_MaxMonth; j++)
                    {
                        m_MachineHaltDownTimeRow[0] = myMasterMachineHaltList[i];
                        m_MachineHaltDownTimeRow[1] = myMasterMachineHaltList[i] + "时间";
                        m_MachineHaltDownTimeRow["ShowDetail"] = "ShowDetail";
                        m_MachineHaltDownTimeRow["Type"] = "MachineHalt";
                        m_MachineHaltDownTimeRow["StatisticalType"] = "DowntimeTime";
                        m_MachineHaltDownCountRow[0] = myMasterMachineHaltList[i];
                        m_MachineHaltDownCountRow[1] = myMasterMachineHaltList[i] + "次数";
                        m_MachineHaltDownCountRow["ShowDetail"] = "ShowDetail";
                        m_MachineHaltDownCountRow["Type"] = "MachineHalt";
                        m_MachineHaltDownCountRow["StatisticalType"] = "DowntimeCount";
                        DataTable m_RunIndictorsDetailTable = null;
                        if (j == m_MaxMonth - 1)   //如果是最后一个月
                        {
                            m_RunIndictorsDetailTable = RunIndicators.EquipmentHalt.GetEquipmentHalt(myEquipmentId, myOrganizationId, m_CurrentStartTime.AddMonths(j).ToString("yyyy-MM-dd"), myEndTime, myMasterMachineHaltListTime[i], _dataFactory);
                        }
                        else
                        {
                            m_RunIndictorsDetailTable = RunIndicators.EquipmentHalt.GetEquipmentHalt(myEquipmentId, myOrganizationId, m_CurrentStartTime.AddMonths(j).ToString("yyyy-MM-dd"), m_CurrentStartTime.AddMonths(j + 1).AddDays(-1).ToString("yyyy-MM-dd"), myMasterMachineHaltListTime[i], _dataFactory);
                        }

                        if (m_RunIndictorsDetailTable != null && m_RunIndictorsDetailTable.Rows.Count > 0)
                        {
                            m_MachineHaltDownTimeRow[j + 2] = m_RunIndictorsDetailTable.Rows[0]["DowntimeTime"];
                            m_MachineHaltDownCountRow[j + 2] = m_RunIndictorsDetailTable.Rows[0]["DowntimeCount"];
                            m_MachineHaltDownTimeRow["StatisticalRange"] = myMasterMachineHaltListTime[i];
                            m_MachineHaltDownCountRow["StatisticalRange"] = myMasterMachineHaltListTime[i];

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
            m_RunindicatorsAndHaltTable.Columns.Add("01", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("02", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("03", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("04", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("05", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("06", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("07", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("08", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("09", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("10", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("11", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("12", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("Type", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("ShowDetail", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("StatisticalRange", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("StatisticalType", typeof(string));
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
        private static DataTable GetRunindicatorsAndHaltTable(string myYear)
        {
            DataTable m_RunindicatorsAndHaltTable = new DataTable();
            m_RunindicatorsAndHaltTable.Columns.Add("QuotasID", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("QuotasName", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-01", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-02", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-03", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-04", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-05", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-06", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-07", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-08", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-09", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-10", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-11", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add(myYear + "-12", typeof(decimal));
            m_RunindicatorsAndHaltTable.Columns.Add("Type", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("ShowDetail", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("StatisticalRange", typeof(string));
            m_RunindicatorsAndHaltTable.Columns.Add("StatisticalType", typeof(string));
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
        public static string GetMasterMachineHaltDetail(string myOrganizationId, string myEquipmentId, string myStatisticalRange, string myStatisticalType, string myStartYear)
        {
            int m_MaxMonth = 0;
            if (Int32.Parse(myStartYear) < DateTime.Now.Year)
            {
                m_MaxMonth = 12;
            }
            else if (Int32.Parse(myStartYear) == DateTime.Now.Year)
            {
                m_MaxMonth = DateTime.Now.AddDays(-1).Month;    //取昨天以前的数据
            }

            DateTime m_StartTime = DateTime.Parse(myStartYear + "-01-01");
            DataTable m_MasterMachineHaltDetailTable = GetRunindicatorsAndHaltTable();
            m_MasterMachineHaltDetailTable.Columns.Remove("Type");
            m_MasterMachineHaltDetailTable.Columns.Remove("ShowDetail");
            m_MasterMachineHaltDetailTable.Columns.Remove("StatisticalRange");
            m_MasterMachineHaltDetailTable.Columns.Remove("StatisticalType");
            for (int i = 0; i < m_MaxMonth; i++)
            {
                bool m_Exits = false;
                DataTable m_RunIndictorsDetailTable = null;
                if (myStartYear == DateTime.Now.Year.ToString() && (i + 1) == DateTime.Now.Month)   //如果是本年并且是本月
                {
                    m_RunIndictorsDetailTable = RunIndicators.EquipmentHalt.GetEquipmentHaltByReasonType(myEquipmentId, myOrganizationId, myStatisticalRange, m_StartTime.AddMonths(i).ToString("yyyy-MM-dd"), DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), _dataFactory);
                }
                else
                {
                    m_RunIndictorsDetailTable = RunIndicators.EquipmentHalt.GetEquipmentHaltByReasonType(myEquipmentId, myOrganizationId, myStatisticalRange, m_StartTime.AddMonths(i).ToString("yyyy-MM-dd"), m_StartTime.AddMonths(i + 1).AddDays(-1).ToString("yyyy-MM-dd"), _dataFactory);
                }

                if (m_RunIndictorsDetailTable != null)
                {
                    for (int x = 0; x < m_RunIndictorsDetailTable.Rows.Count; x++)
                    {
                        m_Exits = false;
                        for (int y = 0; y < m_MasterMachineHaltDetailTable.Rows.Count; y++)
                        {
                            //如果已经找到该名称
                            if (m_RunIndictorsDetailTable.Rows[x]["ReasonStatisticsTypeId"].ToString() == m_MasterMachineHaltDetailTable.Rows[y]["QuotasID"].ToString())
                            {
                                if (myStatisticalType == "DowntimeTime")
                                {
                                    m_MasterMachineHaltDetailTable.Rows[y][i + 2] = m_RunIndictorsDetailTable.Rows[x]["DowntimeTime"];
                                }
                                else if (myStatisticalType == "DowntimeCount")
                                {
                                    m_MasterMachineHaltDetailTable.Rows[y][i + 2] = m_RunIndictorsDetailTable.Rows[x]["DowntimeCount"];
                                }
                                m_Exits = true;
                                break;
                            }
                        }
                        if (m_Exits == false)
                        {
                            DataRow m_NewDataRow = m_MasterMachineHaltDetailTable.NewRow();
                            m_NewDataRow["QuotasID"] = m_RunIndictorsDetailTable.Rows[x]["ReasonStatisticsTypeId"].ToString();
                            m_NewDataRow["QuotasName"] = m_RunIndictorsDetailTable.Rows[x]["ReasonStatisticsTypeName"].ToString();
                            if (myStatisticalType == "DowntimeTime")
                            {
                                m_NewDataRow[i + 2] = m_RunIndictorsDetailTable.Rows[x]["DowntimeTime"];
                            }
                            else if (myStatisticalType == "DowntimeCount")
                            {
                                m_NewDataRow[i + 2] = m_RunIndictorsDetailTable.Rows[x]["DowntimeCount"];
                            }
                            m_MasterMachineHaltDetailTable.Rows.Add(m_NewDataRow);
                        }
                    }
                }

            }
            string m_ReturnValue = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_MasterMachineHaltDetailTable);
            return m_ReturnValue;
        }
    }
}
