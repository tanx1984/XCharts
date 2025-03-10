
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XUGL;

namespace XCharts.Runtime
{
    [UnityEngine.Scripting.Preserve]
    internal sealed class RingHandler : SerieHandler<Ring>
    {

        public override int defaultDimension { get { return 0; } }

        public override void Update()
        {
            base.Update();
            UpdateSerieContext();
        }

        private void UpdateSerieContext()
        {
            var needCheck = chart.isPointerInChart || m_LegendEnter;
            var needInteract = false;
            if (!needCheck)
            {
                if (m_LastCheckContextFlag != needCheck)
                {
                    m_LastCheckContextFlag = needCheck;
                    serie.context.pointerItemDataIndex = -1;
                    serie.context.pointerEnter = false;
                    foreach (var serieData in serie.data)
                    {
                        serieData.context.highlight = false;
                    }
                    chart.RefreshPainter(serie);
                }
                return;
            }
            m_LastCheckContextFlag = needCheck;
            if (m_LegendEnter)
            {
                serie.context.pointerEnter = true;
                foreach (var serieData in serie.data)
                {
                    serieData.context.highlight = true;
                }
            }
            else
            {
                serie.context.pointerEnter = false;
                serie.context.pointerItemDataIndex = -1;
                var ringIndex = GetRingIndex(chart.pointerPos);
                foreach (var serieData in serie.data)
                {
                    if (!needInteract && ringIndex == serieData.index)
                    {
                        serie.context.pointerEnter = true;
                        serie.context.pointerItemDataIndex = ringIndex;
                        serieData.context.highlight = true;
                        needInteract = true;
                    }
                    else
                    {
                        serieData.context.highlight = false;
                    }
                }
            }
            if (needInteract)
            {
                chart.RefreshPainter(serie);
            }
        }

        public override void UpdateTooltipSerieParams(int dataIndex, bool showCategory, string category,
            string marker, string itemFormatter, string numericFormatter,
            ref List<SerieParams> paramList, ref string title)
        {
            if (dataIndex < 0)
                dataIndex = serie.context.pointerItemDataIndex;

            if (dataIndex < 0)
                return;

            var serieData = serie.GetSerieData(dataIndex);
            if (serieData == null)
                return;

            var param = serie.context.param;
            param.serieName = serie.serieName;
            param.serieIndex = serie.index;
            param.category = category;
            param.dimension = defaultDimension;
            param.serieData = serieData;
            param.value = serieData.GetData(0);
            param.total = serieData.GetData(1);
            param.color = chart.theme.GetColor(dataIndex);
            param.marker = SerieHelper.GetItemMarker(serie, serieData, marker);
            param.itemFormatter = SerieHelper.GetItemFormatter(serie, serieData, itemFormatter);
            param.numericFormatter = SerieHelper.GetNumericFormatter(serie, serieData, numericFormatter); ;
            param.columns.Clear();

            param.columns.Add(param.marker);
            param.columns.Add(serieData.name);
            param.columns.Add(ChartCached.NumberToStr(param.value, param.numericFormatter));

            paramList.Add(param);
        }

        public override Vector3 GetSerieDataLabelPosition(SerieData serieData, LabelStyle label)
        {
            var centerRadius = (serieData.context.outsideRadius + serieData.context.insideRadius) / 2;
            var startAngle = serieData.context.startAngle;
            var toAngle = serieData.context.toAngle;
            switch (label.position)
            {
                case LabelStyle.Position.Center:
                    serieData.context.labelPosition = serie.context.center + label.offset;
                    break;
                case LabelStyle.Position.Bottom:
                    var px1 = Mathf.Sin(startAngle * Mathf.Deg2Rad) * centerRadius;
                    var py1 = Mathf.Cos(startAngle * Mathf.Deg2Rad) * centerRadius;
                    var xDiff = serie.clockwise ? -label.distance : label.distance;
                    serieData.context.labelPosition = serie.context.center + new Vector3(px1 + xDiff, py1);
                    break;
                case LabelStyle.Position.Top:
                    startAngle += serie.clockwise ? -label.distance : label.distance;
                    toAngle += serie.clockwise ? label.distance : -label.distance;
                    var px2 = Mathf.Sin(toAngle * Mathf.Deg2Rad) * centerRadius;
                    var py2 = Mathf.Cos(toAngle * Mathf.Deg2Rad) * centerRadius;
                    serieData.context.labelPosition = serie.context.center + new Vector3(px2, py2);
                    break;
            }
            return serieData.context.labelPosition;
        }

        public override void DrawSerie(VertexHelper vh)
        {
            if (!serie.show || serie.animation.HasFadeOut()) return;
            var data = serie.data;
            serie.animation.InitProgress(serie.startAngle, serie.startAngle + 360);
            SerieHelper.UpdateCenter(serie, chart.chartPosition, chart.chartWidth, chart.chartHeight);
            var dataChangeDuration = serie.animation.GetUpdateAnimationDuration();
            var ringWidth = serie.context.outsideRadius - serie.context.insideRadius;
            var dataChanging = false;
            for (int j = 0; j < data.Count; j++)
            {
                var serieData = data[j];
                serieData.index = j;
                if (!serieData.show) continue;
                if (serieData.IsDataChanged()) dataChanging = true;
                var value = serieData.GetFirstData(dataChangeDuration);
                var max = serieData.GetLastData();
                var degree = (float)(360 * value / max);
                var startDegree = GetStartAngle(serie);
                var toDegree = GetToAngle(serie, degree);
                var itemStyle = SerieHelper.GetItemStyle(serie, serieData, serieData.context.highlight);
                var itemColor = SerieHelper.GetItemColor(serie, serieData, chart.theme, j, serieData.context.highlight);
                var itemToColor = SerieHelper.GetItemToColor(serie, serieData, chart.theme, j, serieData.context.highlight);
                var outsideRadius = serie.context.outsideRadius - j * (ringWidth + serie.gap);
                var insideRadius = outsideRadius - ringWidth;
                var borderWidth = itemStyle.borderWidth;
                var borderColor = itemStyle.borderColor;
                var roundCap = serie.roundCap && insideRadius > 0;

                serieData.context.startAngle = serie.clockwise ? startDegree : toDegree;
                serieData.context.toAngle = serie.clockwise ? toDegree : startDegree;
                serieData.context.insideRadius = insideRadius;
                serieData.context.outsideRadius = serieData.radius > 0 ? serieData.radius : outsideRadius;
                if (itemStyle.backgroundColor.a != 0)
                {
                    UGL.DrawDoughnut(vh, serie.context.center, insideRadius, outsideRadius, itemStyle.backgroundColor,
                        itemStyle.backgroundColor, Color.clear, 0, 360, borderWidth, borderColor, 0,
                        chart.settings.cicleSmoothness, false, serie.clockwise);
                }
                UGL.DrawDoughnut(vh, serie.context.center, insideRadius, outsideRadius, itemColor, itemToColor,
                    Color.clear, startDegree, toDegree, borderWidth, borderColor, 0, chart.settings.cicleSmoothness,
                    roundCap, serie.clockwise);
                DrawCenter(vh, serie, serieData, insideRadius, j == data.Count - 1);
            }
            if (!serie.animation.IsFinish())
            {
                serie.animation.CheckProgress(360);
                chart.RefreshChart();
            }
            if (dataChanging)
            {
                chart.RefreshChart();
            }
        }

        public override void OnLegendButtonClick(int index, string legendName, bool show)
        {
            if (!serie.IsLegendName(legendName))
                return;
            LegendHelper.CheckDataShow(serie, legendName, show);
            chart.UpdateLegendColor(legendName, show);
            chart.RefreshPainter(serie);
        }

        public override void OnLegendButtonEnter(int index, string legendName)
        {
            if (!serie.IsLegendName(legendName))
                return;
            LegendHelper.CheckDataHighlighted(serie, legendName, true);
            chart.RefreshPainter(serie);
        }

        public override void OnLegendButtonExit(int index, string legendName)
        {
            if (!serie.IsLegendName(legendName))
                return;
            LegendHelper.CheckDataHighlighted(serie, legendName, false);
            chart.RefreshPainter(serie);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
        }

        private float GetStartAngle(Serie serie)
        {
            return serie.clockwise ? serie.startAngle : 360 - serie.startAngle;
        }

        private float GetToAngle(Serie serie, float angle)
        {
            var toAngle = angle + serie.startAngle;
            if (!serie.clockwise)
            {
                toAngle = 360 - angle - serie.startAngle;
            }
            if (!serie.animation.IsFinish())
            {
                var currAngle = serie.animation.GetCurrDetail();
                if (serie.clockwise)
                {
                    toAngle = toAngle > currAngle ? currAngle : toAngle;
                }
                else
                {
                    toAngle = toAngle < 360 - currAngle ? 360 - currAngle : toAngle;
                }
            }
            return toAngle;
        }

        private void DrawCenter(VertexHelper vh, Serie serie, SerieData serieData, float insideRadius, bool last)
        {
            var itemStyle = SerieHelper.GetItemStyle(serie, serieData);
            if (!ChartHelper.IsClearColor(itemStyle.centerColor) && last)
            {
                var radius = insideRadius - itemStyle.centerGap;
                var smoothness = chart.settings.cicleSmoothness;
                UGL.DrawCricle(vh, serie.context.center, radius, itemStyle.centerColor, smoothness);
            }
        }

        private void DrawBackground(VertexHelper vh, Serie serie, SerieData serieData, int index, float insideRadius, float outsideRadius)
        {
            var itemStyle = SerieHelper.GetItemStyle(serie, serieData);
            var backgroundColor = SerieHelper.GetItemBackgroundColor(serie, serieData, chart.theme, index, false);
            if (itemStyle.backgroundWidth != 0)
            {
                var centerRadius = (outsideRadius + insideRadius) / 2;
                var inradius = centerRadius - itemStyle.backgroundWidth / 2;
                var outradius = centerRadius + itemStyle.backgroundWidth / 2;
                UGL.DrawDoughnut(vh, serie.context.center, inradius,
                    outradius, backgroundColor, Color.clear, chart.settings.cicleSmoothness);
            }
            else
            {
                UGL.DrawDoughnut(vh, serie.context.center, insideRadius,
                    outsideRadius, backgroundColor, Color.clear, chart.settings.cicleSmoothness);
            }
        }

        private void DrawBorder(VertexHelper vh, Serie serie, SerieData serieData, float insideRadius, float outsideRadius)
        {
            var itemStyle = SerieHelper.GetItemStyle(serie, serieData);
            if (itemStyle.show && itemStyle.borderWidth > 0 && !ChartHelper.IsClearColor(itemStyle.borderColor))
            {
                UGL.DrawDoughnut(vh, serie.context.center, outsideRadius,
                outsideRadius + itemStyle.borderWidth, itemStyle.borderColor,
                Color.clear, chart.settings.cicleSmoothness);
                UGL.DrawDoughnut(vh, serie.context.center, insideRadius,
                insideRadius + itemStyle.borderWidth, itemStyle.borderColor,
                Color.clear, chart.settings.cicleSmoothness);
            }
        }

        private int GetRingIndex(Vector2 local)
        {
            var dist = Vector2.Distance(local, serie.context.center);
            if (dist > serie.context.outsideRadius) return -1;
            Vector2 dir = local - new Vector2(serie.context.center.x, serie.context.center.y);
            float angle = VectorAngle(Vector2.up, dir);
            for (int i = 0; i < serie.data.Count; i++)
            {
                var serieData = serie.data[i];
                serieData.index = i;
                if (dist >= serieData.context.insideRadius &&
                    dist <= serieData.context.outsideRadius &&
                    angle >= serieData.context.startAngle &&
                    angle <= serieData.context.toAngle)
                {
                    return i;
                }
            }
            return -1;
        }

        private float VectorAngle(Vector2 from, Vector2 to)
        {
            float angle;

            Vector3 cross = Vector3.Cross(from, to);
            angle = Vector2.Angle(from, to);
            angle = cross.z > 0 ? -angle : angle;
            angle = (angle + 360) % 360;
            return angle;
        }
    }
}