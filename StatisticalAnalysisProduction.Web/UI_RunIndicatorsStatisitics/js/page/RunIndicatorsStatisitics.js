$(document).ready(function () {
    InitializeDateTime();
});

function InitializeDateTime() {
    //StartTime; EndTime

    var lastMonthDate = new Date();  //上月日期  
    lastMonthDate.setMonth(lastMonthDate.getMonth() - 1);

    $('#StartTimeF').datetimespinner({
        formatter: formatter2,
        parser: parser2,
        selections: [[0, 4], [5, 7], [8, 9]],
        required: true
    });
    var m_LastMonthDateString = lastMonthDate.getFullYear();
    if (lastMonthDate.getMonth() + 1 < 10) {
        m_LastMonthDateString = m_LastMonthDateString + '-0' + (lastMonthDate.getMonth() + 1);
    }
    else {
        m_LastMonthDateString = m_LastMonthDateString + '-' + (lastMonthDate.getMonth() + 1);
    }
    if (lastMonthDate.getDate() < 10) {
        m_LastMonthDateString = m_LastMonthDateString + '-0' + lastMonthDate.getDate();
    }
    else {
        m_LastMonthDateString = m_LastMonthDateString + '-' + lastMonthDate.getDate();
    }
    $('#StartTimeF').datetimespinner('setValue', m_LastMonthDateString);
}
function formatter2(date) {
    if (!date) { return ''; }
    var y = date.getFullYear();
    var m = date.getMonth() + 1;
    return y + '-' + (m < 10 ? ('0' + m) : m);
}
function parser2(s) {
    if (!s) { return null; }
    var ss = s.split('-');
    var y = parseInt(ss[0], 10);
    var m = parseInt(ss[1], 10);
    if (!isNaN(y) && !isNaN(m)) {
        return new Date(y, m - 1, 1);
    } else {
        return new Date();
    }
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
    $('#Combobox_EquipmentCommonF').combobox('clear');
    if (m_FactoryOrganizationId != undefined && m_FactoryOrganizationId != null && m_FactoryOrganizationId != "") {
        $.ajax({
            type: "POST",
            url: "RunIndicatorsStatisitics.aspx/GetEquipmentCommonInfo",
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
    var m_FactoryOrganizationId = $('#TextBox_OrganizationId').val();
    var m_EquipmentCommonId = $('#Combobox_EquipmentCommonF').combobox('getValue');
    var m_StartMonth = $('#StartTimeF').datetimespinner('getValue');
    if (m_FactoryOrganizationId != undefined && m_FactoryOrganizationId != null && m_FactoryOrganizationId != ""
        && m_EquipmentCommonId != undefined && m_EquipmentCommonId != null && m_EquipmentCommonId != "") {


        $.ajax({
            type: "POST",
            url: "RunIndicatorsStatisitics.aspx/GetRunindicatorsInfo",
            data: "{myOrganizationId:'" + m_FactoryOrganizationId + "',myEquipmentCommonId:'" + m_EquipmentCommonId + "',myStartMonth:'" + m_StartMonth + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var m_MsgData = jQuery.parseJSON(msg.d);
                if (m_MsgData != null && m_MsgData != undefined) {
                    $('#grid_ProductionResultInfo').datagrid('loadData', m_MsgData);
                }
            }
        });
        $.ajax({
            type: "POST",
            url: "RunIndicatorsStatisitics.aspx/GetMachineHaltInfo",
            data: "{myOrganizationId:'" + m_FactoryOrganizationId + "',myEquipmentCommonId:'" + m_EquipmentCommonId + "',myStartMonth:'" + m_StartMonth + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var m_MsgData = jQuery.parseJSON(msg.d);
                if (m_MsgData != null && m_MsgData != undefined) {
                    $('#grid_MasterMachineHaltDetail').datagrid('loadData', m_MsgData);
                    var m_Index = 0;
                    var m_Name = "";
                    for (var i = 0; i < m_MsgData.rows.length; i++) {
                        if (i == 0 && m_Name != m_MsgData.rows[i]["StatisticsCycle"]) {
                            m_Index = i;
                            m_Name = m_MsgData.rows[i]["StatisticsCycle"];
                        }
                        else if (m_Name != m_MsgData.rows[i]["StatisticsCycle"]) {
                            $('#grid_MasterMachineHaltDetail').datagrid('mergeCells', {
                                index: m_Index,
                                field: 'StatisticsCycle',
                                rowspan: i - m_Index
                            });
                            m_Index = i;
                            m_Name = m_MsgData.rows[i]["StatisticsCycle"];
                        }

                        if (i + 1 == m_MsgData.rows.length && m_Index != i) {
                            $('#grid_MasterMachineHaltDetail').datagrid('mergeCells', {
                                index: m_Index,
                                field: 'StatisticsCycle',
                                rowspan: i - m_Index + 1
                            });
                        }

                    }
                }
            }
        });
    }
    else {
        alert("请先选择生产区域和设备名称!");
    }
}