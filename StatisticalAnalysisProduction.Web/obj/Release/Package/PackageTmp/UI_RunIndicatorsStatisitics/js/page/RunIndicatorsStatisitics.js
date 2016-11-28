var WaitHiddenCount = 2;
$(document).ready(function () {
    InitializeDateTime();
    LoadEquipmentCommonInfo();
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
                if (m_MsgData.rows.length > 0) {
                    $('#Combobox_EquipmentCommonF').combobox('setValue', m_MsgData.rows[0].EquipmentCommonId);
                    ChangeSpecifications(m_MsgData.rows[0].EquipmentCommonId);
                }
            }
        }
    });
}
function ChangeSpecifications(myEquipmentCommonId) {
    $.ajax({
        type: "POST",
        url: "RunIndicatorsStatisitics.aspx/GetSpecificationsInfo",
        data: '{myEquipmentCommonId:"' + myEquipmentCommonId + '"}',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (msg) {
            var m_MsgData = jQuery.parseJSON(msg.d);
            var m_FinalData = [];
            m_FinalData.push({ "id": "All", "text": "全部" });
            if (m_MsgData != null && m_MsgData != undefined) {
                for (var i = 0; i < m_MsgData.rows.length; i++) {
                    m_FinalData.push(m_MsgData.rows[i]);
                }
            }
            $('#Combobox_SpecificationsF').combobox('loadData', m_FinalData);
            $('#Combobox_SpecificationsF').combobox("select", "All");
        }
    });
}
function RefreshStatisitics() {
    WaitHiddenCount = 2;
    $.messager.progress({
        title: 'Please waiting',
        msg: 'Loading data...'
    });

    var m_OrganizationText = $('#TextBox_OrganizationText').textbox('getText');
    if (m_OrganizationText == "") {
        $('#TextBox_OrganizationId').attr('value', "");
    }
    var m_FactoryOrganizationId = $('#TextBox_OrganizationId').val();
    var m_EquipmentCommonId = $('#Combobox_EquipmentCommonF').combobox('getValue');
    var m_StartMonth = $('#StartTimeF').datetimespinner('getValue');
    var m_Specifications = $('#Combobox_SpecificationsF').combobox('getValue');     //增加设备规格型号查询功能
    var m_RunindicatorsOrder = $('#Combobox_RunindicatorsOrderF').combobox('getValue');     //增加排序功能
    if (m_EquipmentCommonId != undefined && m_EquipmentCommonId != null && m_EquipmentCommonId != "") {

        $.ajax({
            type: "POST",
            url: "RunIndicatorsStatisitics.aspx/GetRunindicatorsInfo",
            data: "{myOrganizationId:'" + m_FactoryOrganizationId + "',myEquipmentCommonId:'" + m_EquipmentCommonId + "',mySpecifications:'" + m_Specifications + "',myRunindicatorsOrder:'" + m_RunindicatorsOrder + "',myStartMonth:'" + m_StartMonth + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (msg) {
                var m_MsgData = jQuery.parseJSON(msg.d);
                if (m_MsgData != null && m_MsgData != undefined) {
                    $('#grid_ProductionResultInfo').datagrid('loadData', m_MsgData);
                }
                if (WaitHiddenCount == 1) {
                    $.messager.progress('close');
                }
                else {
                    WaitHiddenCount = 1;
                }
            },
            error: function (msg) {
                if (WaitHiddenCount == 1) {
                    $.messager.progress('close');
                }
                else {
                    WaitHiddenCount = 1;
                }
            }
        });
        $.ajax({
            type: "POST",
            url: "RunIndicatorsStatisitics.aspx/GetMachineHaltInfo",
            data: "{myOrganizationId:'" + m_FactoryOrganizationId + "',myEquipmentCommonId:'" + m_EquipmentCommonId + "',mySpecifications:'" + m_Specifications + "',myStartMonth:'" + m_StartMonth + "'}",
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
                    var m_Index = 0;
                    var m_Name = "";
                    for (var i = 0; i < m_MsgData.rows.length; i++) {
                        if (i == 0 && m_Name != m_MsgData.rows[i]["OrganizationName"]) {
                            m_Index = i;
                            m_Name = m_MsgData.rows[i]["OrganizationName"];
                        }
                        else if (m_Name != m_MsgData.rows[i]["OrganizationName"]) {
                            $('#grid_MasterMachineHaltDetail').datagrid('mergeCells', {
                                index: m_Index,
                                field: 'OrganizationName',
                                rowspan: i - m_Index
                            });
                            m_Index = i;
                            m_Name = m_MsgData.rows[i]["OrganizationName"];
                        }

                        if (i + 1 == m_MsgData.rows.length && m_Index != i) {
                            $('#grid_MasterMachineHaltDetail').datagrid('mergeCells', {
                                index: m_Index,
                                field: 'OrganizationName',
                                rowspan: i - m_Index + 1
                            });
                        }

                    }
                }
                if (WaitHiddenCount == 1) {
                    $.messager.progress('close');
                }
                else {
                    WaitHiddenCount = 1;
                }
            },
            error: function (msg) {
                if (WaitHiddenCount == 1) {
                    $.messager.progress('close');
                }
                else {
                    WaitHiddenCount = 1;
                }
            }
        });
    }
    else {
        alert("请先选择设备名称!");
    }
}