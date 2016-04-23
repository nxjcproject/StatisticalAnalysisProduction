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

    <script type="text/javascript" src="/lib/ealib/jquery.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/jquery.utility.js"></script>
    <script type="text/javascript" src="/lib/ealib/jquery.easyui.min.js" charset="utf-8"></script>
    <!--[if lt IE 8 ]><script type="text/javascript" src="/js/common/json2.min.js"></script><![endif]-->
    <script type="text/javascript" src="/lib/ealib/easyui-lang-zh_CN.js" charset="utf-8"></script>

    <script type="text/javascript" src="js/page/RunIndicatorsStatisiticsMonthly.js" charset="utf-8"></script>
</head>
<body>
    <div class="easyui-layout" data-options="fit:true,border:false">
        <div data-options="region:'west',border:false " style="width: 230px;">
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
                                <td style="width: 130px;">
                                    <input id="TextBox_OrganizationText" class="easyui-textbox" data-options="editable:false, readonly:true" style="width: 120px;" />
                                </td>
                                <td>主要设备</td>
                                <td style="width: 160px;">
                                    <select id="Combobox_EquipmentCommonF" class="easyui-combobox" name="Combobox_EquipmentCommonF" data-options="panelHeight:'auto', editable:true, valueField: 'EquipmentCommonId',textField: 'Name'" style="width: 120px;"></select>
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
                    <th data-options="field:'Id',width:60, hidden:true">指标ID</th>
                    <th data-options="field:'Name',width:140">名称</th>
                    <th data-options="field:'01',width:60">1月</th>
                    <th data-options="field:'01',width:60">2月</th>
                    <th data-options="field:'01',width:60">3月</th>
                    <th data-options="field:'01',width:60">4月</th>
                    <th data-options="field:'01',width:60">5月</th>
                    <th data-options="field:'01',width:60">6月</th>
                    <th data-options="field:'01',width:60">7月</th>
                    <th data-options="field:'01',width:60">8月</th>
                    <th data-options="field:'01',width:60">9月</th>
                    <th data-options="field:'01',width:60">10月</th>
                    <th data-options="field:'01',width:60">11月</th>
                    <th data-options="field:'01',width:60">12月</th>
                </tr>
            </thead>
        </table>
    </div>
    <div id="dlg_QuickTrend" class="easyui-dialog">
        
    </div>
    <form id="formMain" runat="server">
        <div>
        </div>
    </form>
</body>
</html>
