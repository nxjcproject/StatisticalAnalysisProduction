<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RunIndicatorsStatisiticsMonthly.aspx.cs" Inherits="StatisticalAnalysisProduction.Web.UI_RunIndicatorsStatisitics.RunIndicatorsStatisiticsMonthly" %>

<%@ Register Src="~/UI_WebUserControls/OrganizationSelector/OrganisationTree.ascx" TagPrefix="uc1" TagName="OrganisationTree" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>设备运行指标月统计</title>
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/gray/easyui.css" />
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/icon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtIcon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtCss.css" />


    <link rel="stylesheet" type="text/css" href="/lib/pllib/themes/jquery.jqplot.min.css" />
    <link type="text/css" rel="stylesheet" href="/lib/pllib/syntaxhighlighter/styles/shCoreDefault.min.css" />
    <link type="text/css" rel="stylesheet" href="/lib/pllib/syntaxhighlighter/styles/shThemejqPlot.min.css" />
    <link type="text/css" rel="stylesheet" href="/css/common/charts.css" />
    <link type="text/css" rel="stylesheet" href="/css/common/NormalPage.css" />
    <!--<link type="text/css" rel="stylesheet" href="/UI_ComprehensiveDailyReport/css/page/DispatchDailyReport.css" />-->
    <link rel="stylesheet" type="text/css" href="css/page/Style_OverView_Factory.css" />

    <script type="text/javascript" src="/lib/ealib/jquery.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/jquery.easyui.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/lib/ealib/easyui-lang-zh_CN.js" charset="utf-8"></script>

    <!--[if lt IE 9]><script type="text/javascript" src="/lib/pllib/excanvas.js"></script><![endif]-->
    <script type="text/javascript" src="/lib/pllib/jquery.jqplot.min.js"></script>
    <!--<script type="text/javascript" src="/lib/pllib/syntaxhighlighter/scripts/shCore.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/syntaxhighlighter/scripts/shBrushJScript.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/syntaxhighlighter/scripts/shBrushXml.min.js"></script>-->

    <!-- Additional plugins go here -->
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.barRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.pieRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.categoryAxisRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.pointLabels.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.cursor.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.canvasTextRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.canvasAxisTickRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.dateAxisRenderer.min.js"></script>
    <script type="text/javascript" src="/lib/pllib/plugins/jqplot.highlighter.min.js"></script>





    <!--<script type="text/javascript" src="/lib/pllib/themes/jquery.jqplot.js"></script>
    <script type="text/javascript" src="/lib/pllib/themes/jjquery.jqplot.min.js"></script>-->
    <!--[if lt IE 8 ]><script type="text/javascript" src="/js/common/json2.min.js"></script><![endif]-->

    <script type="text/javascript" src="/js/common/components/Charts.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/components/DataGrid.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/components/WindowsDialog.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/components/GridChart.js" charset="utf-8"></script>


    <script type="text/javascript" src="js/page/TimeLineChart.js" charset="utf-8"></script>
    <script type="text/javascript" src="js/page/RunIndicatorsStatisiticsMonthly.js" charset="utf-8"></script>

</head>
<body>
    <div class="easyui-layout" data-options="fit:true,border:false">
        <div data-options="region:'west',border:false " style="width: 150px;">
            <uc1:OrganisationTree ID="OrganisationTree_ProductionLine" runat="server" />
        </div>
        <div data-options="region:'center',border:false,collapsible:false" style="padding-left: 10px;">
            <table id="grid_MasterMachineHaltInfo" class="easyui-datagrid" data-options="fit:true, rownumbers: true,striped:true, singleSelect:true, border:true">
            </table>
        </div>
        <div id="toolbar_MasterMachineHaltInfo" style="display: none;">
            <table>
                <tr>
                    <td>
                        <table>
                            <tr>
                                <td>选择年份</td>
                                <td style="width: 90px;">
                                    <input id="StartTimeF" class="easyui-numberspinner" style="width: 80px" required="required" data-options="min:1900,max:9999, editable:false" />
                                </td>
                                <td>生产区域</td>
                                <td style="width: 100px;">
                                    <input id="TextBox_OrganizationText" class="easyui-textbox" data-options="editable:false, readonly:true" style="width: 90px;" />
                                </td>
                                <td>主要设备</td>
                                <td style="width: 90px;">
                                    <select id="Combobox_EquipmentCommonF" class="easyui-combobox" name="Combobox_EquipmentCommonF" data-options="panelHeight:'auto', editable:true, valueField: 'EquipmentCommonId',textField: 'Name', onSelect: function(myValue){LoadEquipmentInfoByCommonId(myValue.EquipmentCommonId);}" style="width: 80px;"></select>
                                </td>
                                <td style="width: 100px;">
                                    <select id="Combobox_EquipmentF" class="easyui-combobox" name="Combobox_EquipmentF" data-options="panelHeight:'auto', editable:true, valueField: 'EquipmentId',textField: 'EquipmentName'" style="width: 90px;"></select>
                                </td>
                                <td>
                                    <a href="javascript:void(0);" class="easyui-linkbutton" data-options="iconCls:'icon-search',plain:true"
                                        onclick="RefreshStatisitics();">查询</a>
                                </td>
                                <td>
                                    <input id="TextBox_OrganizationId" style="width: 10px; visibility: hidden;" />
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </div>
    </div>
    <!--快捷菜单-->
    <div id="dlg_QuickMachineHaltDetail" class="easyui-dialog">
        <table id="grid_QuickMachineHaltDetail" class="easyui-datagrid" data-options="fit:true, rownumbers: true,striped:true, singleSelect:true, border:false">
            <thead>
                <tr>
                    <th data-options="field:'QuotasId',width:60, hidden:true">指标ID</th>
                    <th data-options="field:'QuotasName',width:140">名称</th>
                    <th data-options="field:'01',width:60">1月</th>
                    <th data-options="field:'02',width:60">2月</th>
                    <th data-options="field:'03',width:60">3月</th>
                    <th data-options="field:'04',width:60">4月</th>
                    <th data-options="field:'05',width:60">5月</th>
                    <th data-options="field:'06',width:60">6月</th>
                    <th data-options="field:'07',width:60">7月</th>
                    <th data-options="field:'08',width:60">8月</th>
                    <th data-options="field:'09',width:60">9月</th>
                    <th data-options="field:'10',width:60">10月</th>
                    <th data-options="field:'11',width:60">11月</th>
                    <th data-options="field:'12',width:60">12月</th>
                </tr>
            </thead>
        </table>
    </div>
    <form id="formMain" runat="server">
        <div>
        </div>
    </form>
</body>
</html>
