var PlotObj = { "Obj": [], "FirstLoadFlag": true };
///////////////获得折线图///////////////////
function ReleasePlotChart(containerId, plot) {
    if (plot) {
        plot.destroy();

        var elementId = '#' + containerId;
        $(elementId).unbind(); // for iexplorer  
        $(elementId).empty();

        plot = null;
    }
}
function GetChartData(myChartName, myData) {
    var m_ChartData = [];
    var m_legendData = [];
    var m_MaxValue = 0;
    for (var i = 0; i < myData["rows"].length; i++) {
        /////////////遍历每一列数据
        var j = 0;
        m_ChartData[i] = [];
        $.each(myData["rows"][i], function (myKey, myValue) {          //遍历采集服务器
            if (j == 0) {

            }
            else if (j == 1) {
                m_legendData[i] = myValue;
            }
            else {
                var m_Date = myKey.split('-');
                var m_PointValue = parseFloat(myValue);
                m_ChartData[i][j - 2] = [m_Date[1] + '/' + '01/' + m_Date[0], m_PointValue];
                if (m_PointValue > m_MaxValue) {
                    m_MaxValue = m_PointValue;        //找出最大的纵坐标
                }
            }
            j = j + 1;
        });
    }
    if (m_MaxValue <= 1) {
        m_MaxValue = 1;
    }
    else if (m_MaxValue > 1 && m_MaxValue < 20) {
        m_MaxValue = 20;
    }
    else if (m_MaxValue >= 20 && m_MaxValue < 40) {
        m_MaxValue = 40;
    }
    else if (m_MaxValue >= 40 && m_MaxValue < 60) {
        m_MaxValue = 60;
    }
    else if (m_MaxValue >= 60 && m_MaxValue < 80) {
        m_MaxValue = 80;
    }
    else if (m_MaxValue >= 80 && m_MaxValue < 100) {
        m_MaxValue = 100;
    }
    else if (m_MaxValue >= 100 && m_MaxValue < 120) {
        m_MaxValue = 120;
    }
    else if (m_MaxValue >= 120 && m_MaxValue < 140) {
        m_MaxValue = 140;
    }
    else if (m_MaxValue >= 140 && m_MaxValue < 160) {
        m_MaxValue = 160;
    }
    else {
        m_MaxValue = m_MaxValue + 10;
    }
    if (PlotObj["FirstLoadFlag"] == true) {
        GetLineChart(myChartName, m_ChartData, m_MaxValue);
        PlotObj["FirstLoadFlag"] = false;
    }
    else {
        ReleasePlotChart(myChartName, PlotObj["Obj"])
        GetLineChart(myChartName, m_ChartData, m_MaxValue);
        //PlotObj["Obj"].series[0].data = [];
        //PlotObj["Obj"].series[0].data = m_ChartData;
        //PlotObj["Obj"].replot();
    }
    GetLineChartLegend(myChartName, m_legendData);
}
function GetLineChartLegend(myChartName, mylegendData) {
    var m_SeriesColors = ["#01b3f9", "#fef102", "#f8000e", "#a400ed", "#aaf900", "#fe0072", "#0c6c92", "#fea002", "#c1020a", "#62008d", "#3c8300"];
    var m_LegendObjId = myChartName.substring(0, myChartName.length - '_Content'.length) + "_Legend";
    var m_LegendObj = $('#' + m_LegendObjId);
    m_LegendObj.empty();
    var m_LegendHtml = '<table id="' + m_LegendObjId + '_DefinedLegendTable" style="bottom: 0px;"><tbody><tr class="jqplot-table-legend"><td id="' + m_LegendObjId + '_DefinedLegendBlankTd"></td>';
    for (var i = 0; i < mylegendData.length; i++) {
        m_LegendHtml = m_LegendHtml + '<td style="text-align: center; padding-top: 0px;" class="jqplot-table-legend jqplot-table-legend-swatch"><div class="jqplot-table-legend-swatch-outline"><div style="background-color: ' + m_SeriesColors[i] + '; border-color:  ' + m_SeriesColors[i] + '" class="jqplot-table-legend-swatch"></div></div></td><td style="padding-top: 0px; padding-right:4px; color:#555555; font-size:8pt; font-family: SimSun;">' + mylegendData[i] + '</td>';
    }
    m_LegendHtml = m_LegendHtml + '</tr></tbody></table>';
    m_LegendObj.append($(m_LegendHtml));
    var m_LegendBlankTdWidth = ($('#' + m_LegendObjId).width() - $('#' + m_LegendObjId + '_DefinedLegendTable').width()) / 2;
    $('#' + m_LegendObjId + '_DefinedLegendBlankTd').css("width", m_LegendBlankTdWidth);
    $.parser.parse('#' + m_LegendObjId);
}
function GetLineChart(myChartName, myData, myMaxValue) {
    //    var line3 = [[['01/01/2008', 0.42], ['02/01/2008', 0.80], ['03/01/2008', 0.56], ['04/01/2008', 0.68],
    //                ['05/01/2008', 0.43], ['06/01/2008', 0.87]]];
    var m_SeriesColors = ["#01b3f9", "#fef102", "#f8000e", "#a400ed", "#aaf900", "#fe0072", "#0c6c92", "#fea002", "#c1020a", "#62008d", "#3c8300"];
    PlotObj["Obj"] = $.jqplot(myChartName, myData, {
        animate: true,
        seriesColors: m_SeriesColors,
        title: "",
        animateReplot: true,
        seriesDefaults: {
            lineWidth: 1,
            markerOptions: { size: 0 }
        },
        axes: {
            xaxis: {
                renderer: $.jqplot.DateAxisRenderer,
                tickOptions: {
                    formatString: "%Y-%m"
                },
                labelOptions: {
                    fontFamily: 'Helvetica',
                    fontSize: '8pt'
                },
            },
            yaxis: {
                tickOptions: {
                    formatString: "%.2f"
                },
                labelOptions: {
                    fontFamily: 'Helvetica',
                    fontSize: '8pt'
                },
                min: 0,
                max: myMaxValue,
                numberTicks: 5,
            }
        },
        highlighter: {
            show: true,
            sizeAdjust: 15
        },
        //cursor: {
        //    show: true,
        //    tooltipLocation: 'sw'
        //},
        grid: {
            drawGridLines: true, // wether to draw lines across the grid or not.
            gridLineColor: '#cccccc', // 设置整个图标区域网格背景线的颜色
            background: '#fffdf6', // 设置整个图表区域的背景色
            borderColor: '#999999', // 设置图表的(最外侧)边框的颜色
            borderWidth: 2.0, //设置图表的（最外侧）边框宽度
            shadow: false, // 为整个图标（最外侧）边框设置阴影，以突出其立体效果
            shadowAngle: 45, // 设置阴影区域的角度，从x轴顺时针方向旋转
            shadowOffset: 1.5, // 设置阴影区域偏移出图片边框的距离
            shadowWidth: 3, // 设置阴影区域的宽度
            shadowDepth: 3, // 设置影音区域重叠阴影的数量
            shadowAlpha: 0.07, // 设置阴影区域的透明度
            renderer: $.jqplot.CanvasGridRenderer, // renderer to use to draw the grid.
            rendererOptions: {} // options to pass to the renderer. Note, the default
            // CanvasGridRenderer takes no additional options.
        }
    });
}