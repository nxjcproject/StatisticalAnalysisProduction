﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RunIndicatorsStatisitics.aspx.cs" Inherits="StatisticalAnalysisProduction.Web.UI_RunIndicatorsStatisitics.RunIndicatorsStatisitics" %>

<%@ Register Src="~/UI_WebUserControls/OrganizationSelector/OrganisationTree.ascx" TagPrefix="uc1" TagName="OrganisationTree" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>设备运行指标分析</title>
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/gray/easyui.css" />
    <link rel="stylesheet" type="text/css" href="/lib/ealib/themes/icon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtIcon.css" />
    <link rel="stylesheet" type="text/css" href="/lib/extlib/themes/syExtCss.css" />

    <script type="text/javascript" src="/lib/ealib/jquery.min.js" charset="utf-8"></script>
    <script type="text/javascript" src="/js/common/jquery.utility.js"></script>
    <script type="text/javascript" src="/lib/ealib/jquery.easyui.min.js" charset="utf-8"></script>
    <!--[if lt IE 8 ]><script type="text/javascript" src="/js/common/json2.min.js"></script><![endif]-->
    <script type="text/javascript" src="/lib/ealib/easyui-lang-zh_CN.js" charset="utf-8"></script>

    <script type="text/javascript" src="js/page/RunIndicatorsStatisitics.js" charset="utf-8"></script>
</head>
<body>
    <div class="easyui-layout" data-options="fit:true,border:false">
        <div data-options="region:'west',border:false " style="width: 230px;">
            <uc1:OrganisationTree ID="OrganisationTree_ProductionLine" runat="server" />
        </div>
        <div id="toolbar_ProductionResultInfo" style="display: none;">
            <table>
                <tr>
                    <td>
                        <table>
                            <tr>
                                <td>选择日期</td>
                                <td style="width: 90px;">
                                    <input id="StartTimeF" class="easyui-datetimespinner" style="width: 80px" />
                                </td>
                                <td>生产区域</td>
                                <td style="width: 100px;">
                                    <input id="TextBox_OrganizationText" class="easyui-textbox" data-options="editable:false, readonly:true" style="width: 120px;" />
                                </td>
                                <td>主要设备</td>
                                <td style="width: 100px;">
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
        <div data-options="region:'center',border:false,collapsible:false" style="padding-left: 10px;">
            <div class="easyui-layout" data-options="fit:true,border:false">
                <div data-options="region:'north',border:false " style="height: 170px;">
                    <table id="grid_ProductionResultInfo" class="easyui-datagrid" data-options="fit:true, rownumbers: true,striped:true, singleSelect:true, border:true, toolbar: '#toolbar_ProductionResultInfo'" >
                        <thead>
                            <tr>
                                <th data-options="field:'EquipmentId',width:110, hidden:true">设备ID</th>
                                <th data-options="field:'EquipmentName',width:120">设备名称</th>
                                <th data-options="field:'运转率',width:120">运转率(%)</th>
                                <th data-options="field:'可靠性',width:120">可靠性(%)</th>
                                <th data-options="field:'故障率',width:120">故障率(%)</th>
                                <th data-options="field:'台时产量',width:120">台时产量(t/h)</th>
                                <th data-options="field:'运转时间',width:120">运转时间(h)</th>
                            </tr>
                        </thead>
                    </table>
                </div>
                <div data-options="region:'center',border:false " style="padding-top: 5px;">
                    <table id="grid_MasterMachineHaltDetail" class="easyui-datagrid" data-options="fit:true, rownumbers: true,striped:true, singleSelect:true, border:true">
                        <thead>
                            <tr>
                                <th data-options="field:'StatisticsCycle',width:60">统计周期</th>
                                <th data-options="field:'EquipmentId',width:110, hidden:true">设备ID</th>
                                <th data-options="field:'EquipmentName',width:110">设备名称</th>
                                <th data-options="field:'DowntimeCount',width:80">总次数</th>
                                <th data-options="field:'ProcessDowntimeCount',width:100">工艺故障</th>
                                <th data-options="field:'MechanicalDowntimeCount',width:100">机械故障</th>
                                <th data-options="field:'ElectricalDowntimeCount',width:100">电气故障</th>
                                <th data-options="field:'EnvironmentDowntimeCount',width:100">环境停机</th>
                                <th data-options="field:'DowntimeTime',width:80">总时间</th>
                                <th data-options="field:'ProcessDowntimeTime',width:100">工艺故障</th>
                                <th data-options="field:'MechanicalDowntimeTime',width:100">机械故障</th>
                                <th data-options="field:'ElectricalDowntimeTime',width:100">电气故障</th>
                                <th data-options="field:'EnvironmentDowntimeTime',width:100">环境停机</th>
                            </tr>
                        </thead>
                    </table>

                </div>
            </div>
        </div>
    </div>
    <form id="form1" runat="server">
        <div>
        </div>
    </form>
</body>
</html>
