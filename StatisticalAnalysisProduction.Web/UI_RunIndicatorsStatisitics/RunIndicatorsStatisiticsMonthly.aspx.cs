﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.Services;
using WebStyleBaseForEnergy;
namespace StatisticalAnalysisProduction.Web.UI_RunIndicatorsStatisitics
{
    public partial class RunIndicatorsStatisiticsMonthly : WebStyleBaseForEnergy.webStyleBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            base.InitComponts();
            if (!IsPostBack)
            {
                ////////////////////调试用,自定义的数据授权
#if DEBUG
                List<string> m_DataValidIdItems = new List<string>() { "zc_nxjc_ychc", "zc_nxjc_qtx", "zc_nxjc_byc","zc_nxjc_znc" };
                AddDataValidIdGroup("ProductionOrganization", m_DataValidIdItems);
                mPageOpPermission = "1111";
#elif RELEASE
#endif
                this.OrganisationTree_ProductionLine.Organizations = GetDataValidIdGroup("ProductionOrganization");                 //向web用户控件传递数据授权参数
                this.OrganisationTree_ProductionLine.PageName = "RunIndicatorsStatisitics.aspx";                                     //向web用户控件传递当前调用的页面名称
                this.OrganisationTree_ProductionLine.LeveDepth = 5;
            }
        }
        [WebMethod]
        public static string GetEquipmentCommonInfo(string myOrganizationId)
        {
            string m_ReturnValue = StatisticalAnalysisProduction.Service.RunIndicatorsStatisitics.RunIndicatorsStatisiticsMonthly.GetEquipmentCommonInfo(myOrganizationId);
            return m_ReturnValue;
        }
        [WebMethod]
        public static string GetEquipmentInfoByEquipmentCommonId(string myEquipmentCommonId, string myOrganizationId)
        {
            string m_ReturnValue = StatisticalAnalysisProduction.Service.RunIndicatorsStatisitics.RunIndicatorsStatisiticsMonthly.GetEquipmentInfoByEquipmentCommonId(myEquipmentCommonId, myOrganizationId);
            return m_ReturnValue;
        }
        [WebMethod]
        public static string GetRunindicatorsAndHaltInfo(string myOrganizationId, string myEquipmentId, string myStartYear)
        {
            if (Int32.Parse(myStartYear) < DateTime.Now.Year)
            {
                string m_RunIndictorsDetailValue = StatisticalAnalysisProduction.Service.RunIndicatorsStatisitics.RunIndicatorsStatisiticsMonthly.GetRunindicatorsAndHaltInfo(myEquipmentId, myOrganizationId, myStartYear + "-01-01", myStartYear + "-12-31");
                return m_RunIndictorsDetailValue;
            }
            else if (Int32.Parse(myStartYear) == DateTime.Now.Year)
            {
                string m_RunIndictorsDetailValue = StatisticalAnalysisProduction.Service.RunIndicatorsStatisitics.RunIndicatorsStatisiticsMonthly.GetRunindicatorsAndHaltInfo(myEquipmentId, myOrganizationId, myStartYear + "-01-01", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
                return m_RunIndictorsDetailValue;
            }
            else
            {
                return "{\"rows\":[],\"total\":0}";
            }
        }
        //[WebMethod]
        //public static string GetMachineHaltInfo(string myOrganizationId, string myEquipmentCommonId, string myStartYear)
        //{
        //    DateTime m_EndTime = DateTime.Parse(myStartYear + "-01").AddMonths(1).AddDays(-1);

        //    string m_ReturnValue = StatisticalAnalysisProduction.Service.RunIndicatorsStatisitics.RunIndicatorsStatisitics.GetMachineHaltInfo(myEquipmentCommonId, myOrganizationId, myStartYear + "-01", m_EndTime.ToString("yyyy-MM-dd"));
        //    return m_ReturnValue;
        //}
        [WebMethod]
        public static string GetMasterMachineHaltDetail(string myOrganizationId, string myEquipmentId, string myStatisticalRange, string myStatisticalType, string myStartYear)
        {
            string m_MasterMachineHaltDetailValue = StatisticalAnalysisProduction.Service.RunIndicatorsStatisitics.RunIndicatorsStatisiticsMonthly.GetMasterMachineHaltDetail(myOrganizationId, myEquipmentId, myStatisticalRange, myStatisticalType, myStartYear);
            return m_MasterMachineHaltDetailValue;
        }
    }
}