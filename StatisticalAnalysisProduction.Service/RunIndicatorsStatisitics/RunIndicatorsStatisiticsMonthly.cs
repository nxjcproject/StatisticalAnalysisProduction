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
                        m_MachineHaltDownTimeRow["StatisticalType"] = "DowntimeTime";
                        m_MachineHaltDownCountRow[0] = myMasterMachineHaltList[i];
                        m_MachineHaltDownCountRow[1] = myMasterMachineHaltList[i] + "次数";
                        m_MachineHaltDownCountRow["ShowDetail"] = "ShowDetail";
                        m_MachineHaltDownCountRow["Type"] = "MachineHalt";
                        m_MachineHaltDownCountRow["StatisticalType"] = "DowntimeCount";
                        DataTable m_RunIndictorsDetailTable = null;
                        if (m_CurrentStartTime.Year == DateTime.Now.Year && m_CurrentStartTime.AddMonths(j).Month== DateTime.Now.Month)   //如果是本年并且是本月
                        {
                            m_RunIndictorsDetailTable = RunIndicators.EquipmentHalt.GetEquipmentHalt(new string[] { myEquipmentCommonId }, myOrganizationId, m_CurrentStartTime.AddMonths(j).ToString("yyyy-MM-dd"), DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), myMasterMachineHaltListTime[i], _dataFactory);
                        }
                        else
                        {
                            m_RunIndictorsDetailTable = RunIndicators.EquipmentHalt.GetEquipmentHalt(new string[] { myEquipmentCommonId }, myOrganizationId, m_CurrentStartTime.AddMonths(j).ToString("yyyy-MM-dd"), m_CurrentStartTime.AddMonths(j + 1).AddDays(-1).ToString("yyyy-MM-dd"), myMasterMachineHaltListTime[i], _dataFactory);
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
        public static string GetMasterMachineHaltDetail(string myOrganizationId, string myEquipmentCommonId, string myStatisticalRange, string myStatisticalType, string myStartYear)
        {
            DateTime m_StartTime = DateTime.Parse(myStartYear + "-01-01");
            DataTable m_MasterMachineHaltDetailTable = GetRunindicatorsAndHaltTable();
            m_MasterMachineHaltDetailTable.Columns.Remove("Type");
            m_MasterMachineHaltDetailTable.Columns.Remove("ShowDetail");
            m_MasterMachineHaltDetailTable.Columns.Remove("StatisticalRange");
            m_MasterMachineHaltDetailTable.Columns.Remove("StatisticalType");
            for (int i = 0; i < 12; i++)
            {
                bool m_Exits = false;
                DataTable m_RunIndictorsDetailTable = null;
                if (myStartYear == DateTime.Now.Year.ToString() && (i + 1) == DateTime.Now.Month)   //如果是本年并且是本月
                {
                    m_RunIndictorsDetailTable = RunIndicators.EquipmentHalt.GetEquipmentHaltDetailByReasonType(myEquipmentCommonId, myOrganizationId, myStatisticalRange, m_StartTime.AddMonths(i).ToString("yyyy-MM-dd"), DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), _dataFactory);
                }
                else
                {
                    m_RunIndictorsDetailTable = RunIndicators.EquipmentHalt.GetEquipmentHaltDetailByReasonType(myEquipmentCommonId, myOrganizationId, myStatisticalRange, m_StartTime.AddMonths(i).ToString("yyyy-MM-dd"), m_StartTime.AddMonths(i + 1).AddDays(-1).ToString("yyyy-MM-dd"), _dataFactory);
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
        public static string GetDetailTrend(string myOrganizationId, string myEquipmentCommonId, string myStartYear, string myQuotasID, string myType,string myStatisticalRange, string myStatisticalType)
        {
            DataTable m_TrendTable = GetRunindicatorsAndHaltTable(myStartYear);
            m_TrendTable.Columns.Remove("Type");
            m_TrendTable.Columns.Remove("ShowDetail");
            m_TrendTable.Columns.Remove("StatisticalRange");
            m_TrendTable.Columns.Remove("StatisticalType");
            if (myType == "RunIndictors")
            {
                GetRunIndictorTrend(ref m_TrendTable, myOrganizationId, myEquipmentCommonId, myStartYear, myQuotasID);
            }
            else if(myType == "MachineHalt")
            {
                GetMachineHaltTrend(ref m_TrendTable, myOrganizationId, myEquipmentCommonId, myStartYear, myQuotasID, myStatisticalRange, myStatisticalType);
            }
            string m_ValueString = EasyUIJsonParser.DataGridJsonParser.DataTableToJson(m_TrendTable);
            return m_ValueString;
        }
        public static void GetRunIndictorTrend(ref DataTable m_TrendTable, string myOrganizationId, string myEquipmentCommonId, string myYear, string myQuotasID)
        {
            DataTable m_RunIndictorDetail = RunIndicators.EquipmentRunIndicators.GetEquipmentCommonUtilizationPerMonth(myQuotasID, myEquipmentCommonId, myOrganizationId, myYear + "-01-01", myYear + "-12-31", _dataFactory);
            if (m_RunIndictorDetail != null && m_RunIndictorDetail.Rows.Count > 0)
            {
                if (myQuotasID == "运转率" || myQuotasID == "故障率" || myQuotasID == "可靠性")
                {
                    for (int i = 2; i <= 13; i++)
                    {
                        m_RunIndictorDetail.Rows[0][i] = (decimal)m_RunIndictorDetail.Rows[0][i] * 100;
                    }
                }
                m_TrendTable.Rows.Add(m_RunIndictorDetail.Rows[0].ItemArray);
            }
            string m_Sql = @"SELECT A.EquipmentId
                                  ,A.EquipmentName
                                  FROM equipment_EquipmentDetail A
                                  where A.Enabled = 1
                                  and A.EquipmentCommonId = '{0}'
                                  and A.OrganizationId = '{1}'
                                  order by A.DisplayIndex";
            m_Sql = string.Format(m_Sql, myEquipmentCommonId, myOrganizationId);
            try
            {
                DataTable m_EquipmentCommonInfoTable = _dataFactory.Query(m_Sql);
                if (m_EquipmentCommonInfoTable != null)
                {
                    for (int i = 0; i < m_EquipmentCommonInfoTable.Rows.Count; i++)
                    {
                        DataTable m_EquipimentInfoByCommonId = RunIndicators.EquipmentRunIndicators.GetEquipmentUtilizationPerMonth(myQuotasID, myOrganizationId, m_EquipmentCommonInfoTable.Rows[i]["EquipmentId"].ToString(), myYear + "-01-01", myYear + "-12-31", _dataFactory);
                        if (m_EquipimentInfoByCommonId != null && m_EquipimentInfoByCommonId.Rows.Count > 0)
                        {
                            if (myQuotasID == "运转率" || myQuotasID == "故障率" || myQuotasID == "可靠性")
                            {
                                for (int w = 2; w <= 13; w++)
                                {
                                    m_EquipimentInfoByCommonId.Rows[0][w] = (decimal)m_EquipimentInfoByCommonId.Rows[0][w] * 100;
                                }
                            }
                            m_TrendTable.Rows.Add(m_EquipimentInfoByCommonId.Rows[0].ItemArray);
                        }
                    }
                }
            }
            catch(Exception e)
            {

            }
        }
        public static void GetMachineHaltTrend(ref DataTable m_TrendTable, string myOrganizationId, string myEquipmentCommonId, string myStartYear, string myQuotasID, string myStatisticalRange, string myStatisticalType)
        {
            DateTime m_StartTime = DateTime.Parse(myStartYear + "-01-01");

            for(int i=0;i< 12;i++)
            {
                DataTable m_RunIndictorsCommonTable = null;
                DataTable m_RunIndictorsDetailTable = null;
                if (myStartYear == DateTime.Now.Year.ToString() && (i + 1) == DateTime.Now.Month)   //如果是本年并且是本月
                {
                    m_RunIndictorsCommonTable = RunIndicators.EquipmentHalt.GetEquipmentHalt(new string[] { myEquipmentCommonId }, myOrganizationId, m_StartTime.AddMonths(i).ToString("yyyy-MM-dd"), DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), myStatisticalRange, _dataFactory);
                    m_RunIndictorsDetailTable = RunIndicators.EquipmentHalt.GetEquipmentHaltDetail(myEquipmentCommonId, myOrganizationId, myStatisticalRange, m_StartTime.AddMonths(i).ToString("yyyy-MM-dd"), DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), _dataFactory);
                }
                else
                {
                    m_RunIndictorsCommonTable = RunIndicators.EquipmentHalt.GetEquipmentHalt(new string[] { myEquipmentCommonId }, myOrganizationId, m_StartTime.AddMonths(i).ToString("yyyy-MM-dd"), m_StartTime.AddMonths(i + 1).AddDays(-1).ToString("yyyy-MM-dd"), myStatisticalRange, _dataFactory);
                    m_RunIndictorsDetailTable = RunIndicators.EquipmentHalt.GetEquipmentHaltDetail(myEquipmentCommonId, myOrganizationId, myStatisticalRange, m_StartTime.AddMonths(i).ToString("yyyy-MM-dd"), m_StartTime.AddMonths(i + 1).AddDays(-1).ToString("yyyy-MM-dd"), _dataFactory);
                }

                if (m_RunIndictorsCommonTable != null && m_RunIndictorsDetailTable != null)
                {
                    if (myStatisticalType == "DowntimeTime")
                    {
                        for (int x = 0; x < m_RunIndictorsCommonTable.Rows.Count; x++)
                        {
                            bool m_Exits = false;
                            /////////////////////////总的设备指标
                            for (int y = 0; y < m_TrendTable.Rows.Count; y++)
                            {
                                if (m_RunIndictorsCommonTable.Rows[x]["EquipmentCommonId"].ToString() == m_TrendTable.Rows[y]["QuotasID"].ToString())
                                {
                                    m_TrendTable.Rows[y][i + 2] = m_RunIndictorsCommonTable.Rows[x]["DowntimeTime"];
                                    m_Exits = true;
                                    break;
                                }
                            }
                            if (m_Exits == false)
                            {
                                DataRow m_NewRow = m_TrendTable.NewRow();
                                for (int m = 2; m < m_TrendTable.Columns.Count; m++)
                                {
                                    m_NewRow[m] = 0.0m;
                                }
                                m_NewRow["QuotasID"] = m_RunIndictorsCommonTable.Rows[x]["EquipmentCommonId"].ToString();
                                m_NewRow["QuotasName"] = m_RunIndictorsCommonTable.Rows[x]["EquipmentName"].ToString();
                                m_NewRow[i + 2] = m_RunIndictorsCommonTable.Rows[x]["DowntimeTime"];
                                m_TrendTable.Rows.Add(m_NewRow);
                            }
                        }
                        for (int x = 0; x < m_RunIndictorsDetailTable.Rows.Count; x++)
                        {
                            bool m_Exits = false;
                            /////////////////////////总的设备指标
                            for (int y = 0; y < m_TrendTable.Rows.Count; y++)
                            {
                                if (m_RunIndictorsDetailTable.Rows[x]["EquipmentId"].ToString() == m_TrendTable.Rows[y]["QuotasID"].ToString())
                                {
                                    m_TrendTable.Rows[y][i + 2] = m_RunIndictorsDetailTable.Rows[x]["DowntimeTime"];
                                    m_Exits = true;
                                    break;
                                }
                            }
                            if (m_Exits == false)
                            {
                                DataRow m_NewRow = m_TrendTable.NewRow();
                                for (int m = 2; m < m_TrendTable.Columns.Count; m++)
                                {
                                    m_NewRow[m] = 0.0m;
                                }
                                m_NewRow["QuotasID"] = m_RunIndictorsDetailTable.Rows[x]["EquipmentId"].ToString();
                                m_NewRow["QuotasName"] = m_RunIndictorsDetailTable.Rows[x]["EquipmentName"].ToString();
                                m_NewRow[i + 2] = m_RunIndictorsDetailTable.Rows[x]["DowntimeTime"];
                                m_TrendTable.Rows.Add(m_NewRow);
                            }
                        }
                    }
                    else if (myStatisticalType == "DowntimeCount")
                    {
                        for (int x = 0; x < m_RunIndictorsCommonTable.Rows.Count; x++)
                        {
                            bool m_Exits = false;
                            /////////////////////////总的设备指标
                            for (int y = 0; y < m_TrendTable.Rows.Count; y++)
                            {
                                if (m_RunIndictorsCommonTable.Rows[x]["EquipmentCommonId"].ToString() == m_TrendTable.Rows[y]["QuotasID"].ToString())
                                {
                                    m_TrendTable.Rows[y][i + 2] = m_RunIndictorsCommonTable.Rows[x]["DowntimeCount"];
                                    m_Exits = true;
                                    break;
                                }
                            }
                            if (m_Exits == false)
                            {
                                DataRow m_NewRow = m_TrendTable.NewRow();
                                for (int m = 2; m < m_TrendTable.Columns.Count; m++)
                                {
                                    m_NewRow[m] = 0.0m;
                                }
                                m_NewRow["QuotasID"] = m_RunIndictorsCommonTable.Rows[x]["EquipmentCommonId"].ToString();
                                m_NewRow["QuotasName"] = m_RunIndictorsCommonTable.Rows[x]["EquipmentName"].ToString();
                                m_NewRow[i + 2] = m_RunIndictorsCommonTable.Rows[x]["DowntimeCount"];
                                m_TrendTable.Rows.Add(m_NewRow);
                            }
                        }
                        for (int x = 0; x < m_RunIndictorsDetailTable.Rows.Count; x++)
                        {
                            bool m_Exits = false;
                            /////////////////////////总的设备指标
                            for (int y = 0; y < m_TrendTable.Rows.Count; y++)
                            {
                                if (m_RunIndictorsDetailTable.Rows[x]["EquipmentId"].ToString() == m_TrendTable.Rows[y]["QuotasID"].ToString())
                                {
                                    m_TrendTable.Rows[y][i + 2] = m_RunIndictorsDetailTable.Rows[x]["DowntimeCount"];
                                    m_Exits = true;
                                    break;
                                }
                            }
                            if (m_Exits == false)
                            {
                                DataRow m_NewRow = m_TrendTable.NewRow();
                                for (int m = 2; m < m_TrendTable.Columns.Count; m++)
                                {
                                    m_NewRow[m] = 0.0m;
                                }
                                m_NewRow["QuotasID"] = m_RunIndictorsDetailTable.Rows[x]["EquipmentId"].ToString();
                                m_NewRow["QuotasName"] = m_RunIndictorsDetailTable.Rows[x]["EquipmentName"].ToString();
                                m_NewRow[i + 2] = m_RunIndictorsDetailTable.Rows[x]["DowntimeCount"];
                                m_TrendTable.Rows.Add(m_NewRow);
                            }
                        }
                    }
                }
            }
        }
    }
}
