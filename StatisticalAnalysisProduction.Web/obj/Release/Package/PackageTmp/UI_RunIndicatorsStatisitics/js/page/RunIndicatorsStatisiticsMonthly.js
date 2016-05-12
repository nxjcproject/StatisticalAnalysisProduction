$(document).ready(function () {
    InitializeDateTime();
    InitializeGrid({ "rows": [], "total": 0 });

    //////////////////////初始化对话框//////////////////////
     loadQuickTrendDialog(); 
     loadQuickMachineHaltDetailDialog();
});
function InitializeDateTime() {
    //StartTime; EndTime
    var m_CurrentYear = new Date().getFullYear();  //默认今年  
    $('#StartTimeF').numberspinner('setValue', m_CurrentYear);
}

function onOrganisationTreeClick(myNode) {
    //alert(myNode.text);
    $('#TextBox_OrganizationId').attr('value', myNode.OrganizationId);  //textbox('setText', myNode.OrganizationId);
    $('#TextBox_OrganizationText').textbox('setText', myNode.text);
    //$('#TextBox_OrganizationType').textbox('setText', myNode.OrganizationType);
    LoadEquipmentCommonInfo();
}
function LoadEquipmentCommonInfo() {
    var m_FactoryOrganizationId = $('#TextBox_OrganizationId').val();
    $('#Combotree_EquipmentF').combobox('clear');
    if (m_FactoryOrganizationId != undefined && m_FactoryOrganizationId != null && m_FactoryOrganizationId != "") {
        $.ajax({
            type: "POST",
            url: "RunIndicatorsStatisiticsMonthly.aspx/GetEquipmentInfo",
            data: '{myOrganizationId:"' + m_FactoryOrganizationId + '"}',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var m_MsgData = jQuery.parseJSON(msg.d);
                if (m_MsgData != null && m_MsgData != undefined) {
                    $('#Combobox_EquipmentCommonF').combobox('loadData', m_MsgData.rows);
                }
            }
        });
    }
    else {
        alert("请先选择生产区域!");
    }
}
function RefreshStatisitics() {
    var m_OrganizationId = $('#TextBox_OrganizationId').val();
    var m_EquipmentCommonId = $('#Combobox_EquipmentCommonF').combobox('getValue');
    var m_StartYear = $('#StartTimeF').datetimespinner('getValue');
    if (m_OrganizationId != undefined && m_OrganizationId != null && m_EquipmentCommonId != ""
        && m_EquipmentCommonId != undefined && m_StartYear != null && m_StartYear != "") {

        $.ajax({
            type: "POST",
            url: "RunIndicatorsStatisiticsMonthly.aspx/GetRunindicatorsAndHaltInfo",
            data: "{myOrganizationId:'" + m_OrganizationId + "',myEquipmentCommonId:'" + m_EquipmentCommonId + "',myStartYear:'" + m_StartYear + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var m_MsgData = jQuery.parseJSON(msg.d);
                if (m_MsgData != null && m_MsgData != undefined) {
                    $('#grid_MasterMachineHaltInfo').datagrid('loadData', m_MsgData);
                }
            }
        });
    }
    else {
        alert("请先选择生产区域和设备名称!");
    }
}
function MasterMachineHaltDetailFun(myStatisticalRange, myStatisticalType) {
    var m_OrganizationId = $('#TextBox_OrganizationId').val();
    var m_EquipmentCommonId = $('#Combobox_EquipmentCommonF').combobox('getValue');
    //var $('#grid_MasterMachineHaltInfo').datagrid();
    var m_StartYear = $('#StartTimeF').datetimespinner('getValue');
    $.ajax({
        type: "POST",
        url: "RunIndicatorsStatisiticsMonthly.aspx/GetMasterMachineHaltDetail",
        data: "{myOrganizationId:'" + m_OrganizationId + "',myEquipmentCommonId:'" + m_EquipmentCommonId + "',myStartYear:'" + m_StartYear + "',myStatisticalRange:'" + myStatisticalRange + "',myStatisticalType:'" + myStatisticalType + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var m_MsgData = jQuery.parseJSON(msg.d);
            if (m_MsgData != null && m_MsgData != undefined) {
                $('#grid_QuickMachineHaltDetail').datagrid('loadData', m_MsgData);
            }
        }
    });

    $('#dlg_QuickMachineHaltDetail').dialog('open');
}
function TrendFun(myQuotasID, myType, myStatisticalRange, myStatisticalType) {
    var m_OrganizationId = $('#TextBox_OrganizationId').val();
    var m_EquipmentCommonId = $('#Combobox_EquipmentCommonF').combobox('getValue');
    var m_StartYear = $('#StartTimeF').datetimespinner('getValue');
    $.ajax({
        type: "POST",
        url: "RunIndicatorsStatisiticsMonthly.aspx/GetDetailTrend",
        data: "{myOrganizationId:'" + m_OrganizationId + "',myEquipmentCommonId:'" + m_EquipmentCommonId + "',myStartYear:'" + m_StartYear + "',myQuotasID:'" + myQuotasID + "',myType:'" + myType + "',myStatisticalRange:'" + myStatisticalRange + "',myStatisticalType:'" + myStatisticalType + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var m_MsgData = jQuery.parseJSON(msg.d);
            if (m_MsgData != null && m_MsgData != undefined) {
                GetChartData("chart_QuickTrend_Content", m_MsgData);
            }
        }
    });
    $('#dlg_QuickTrend').dialog('open');
}
function InitializeGrid(myData) {
    grid = $('#grid_MasterMachineHaltInfo').datagrid({
        fit: true,
        rownumbers: true,
        striped: true,
        singleSelect: true,
        border: true,
        frozenColumns: [[{
            width: '60',
            title: '指标ID',
            field: 'QuotasID',
            hidden: true
        }, {
            width: '120',
            title: '名称',
            field: 'QuotasName',
            sortable: true

        }]],
        columns: [[{
            width: '70',
            title: '1月',
            field: '01'

        }, {
            width: '70',
            title: '2月',
            field: '02'

        }, {
            width: '70',
            title: '3月',
            field: '03'

        }, {
            width: '70',
            title: '4月',
            field: '04'

        }, {
            width: '70',
            title: '5月',
            field: '05'

        }, {
            width: '70',
            title: '6月',
            field: '06'

        }, {
            width: '70',
            title: '7月',
            field: '07'

        }, {
            width: '70',
            title: '8月',
            field: '08'

        }, {
            width: '70',
            title: '9月',
            field: '09'

        }, {
            width: '70',
            title: '10月',
            field: '10'

        }, {
            width: '70',
            title: '11月',
            field: '11'

        }, {
            width: '70',
            title: '12月',
            field: '12'

        }, {
            width: '70',
            title: '类型',
            field: 'Type',
            hidden: true
        }, {
            width: '70',
            title: '故障时间',
            field: 'StatisticalRange',
            hidden: true
        }, {
            width: '70',
            title: '统计类型',
            field: 'StatisticalType',
            hidden: true
        }, {
            width: '50',
            title: '操作',
            field: 'Op',
            formatter: function (value, row, index) {
                var str = '';
                str = '<img class="iconImg" src = "/lib/extlib/themes/images/ext_icons/chart/chart_curve.png" title="趋势" onclick="TrendFun(\'' + row.QuotasID + '\',\'' + row.Type + '\',\'' + row.StatisticalRange + '\',\'' + row.StatisticalType + '\');"/>';
                if (row["ShowDetail"] == "ShowDetail") {
                    str = str + '<img class="iconImg" src = "/lib/extlib/themes/images/ext_icons/zoom/zoom.png" title="详细信息" onclick="MasterMachineHaltDetailFun(\'' + row.StatisticalRange + '\',\'' + row.StatisticalType + '\');"/>';
                }
                return str;
            }
        }]],
        toolbar: '#toolbar_MasterMachineHaltInfo'
    });
}

//////////////////////初始化对话框//////////////////////
function loadQuickMachineHaltDetailDialog() {
    $('#dlg_QuickMachineHaltDetail').dialog({
        title: '详细信息',
        width: 1000,
        height: 300,
        left: 10,
        top: 20,
        closed: true,
        cache: false,
        modal: true,
        iconCls: 'icon-search',
        resizable: false
    });
}
function loadQuickTrendDialog() {
    $('#dlg_QuickTrend').dialog({
        title: '趋势',
        width: 1000,
        height: 300,
        left: 10,
        top: 20,
        closed: true,
        cache: false,
        modal: true,
        iconCls: 'icon-search',
        resizable: false
    });
}
