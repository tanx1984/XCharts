using System.Collections.Generic;
using UnityEngine;

namespace XCharts.Runtime
{
    public static class SerieLabelHelper
    {

        public static Color GetLabelColor(Serie serie, ThemeStyle theme, int index)
        {
            if (serie.label != null && !ChartHelper.IsClearColor(serie.label.textStyle.color))
            {
                return serie.label.textStyle.color;
            }
            else
            {
                return theme.GetColor(index);
            }
        }

        public static void ResetLabel(ChartText labelObject, LabelStyle label, ThemeStyle theme,
            Color textColor, float rotate)
        {
            if (labelObject == null) return;
            labelObject.SetColor(textColor);
            labelObject.SetLocalEulerAngles(new Vector3(0, 0, rotate));
            labelObject.SetFontSize(label.textStyle.GetFontSize(theme.common));
            labelObject.SetFontStyle(label.textStyle.fontStyle);
        }

        public static bool CanShowLabel(Serie serie, SerieData serieData, LabelStyle label, int dimesion)
        {
            return serie.show && serieData.context.canShowLabel && !serie.IsIgnoreValue(serieData, dimesion);
        }

        public static string GetFormatterContent(Serie serie, SerieData serieData,
            double dataValue, double dataTotal, LabelStyle serieLabel, Color color)
        {
            if (serieLabel == null)
            {
                serieLabel = SerieHelper.GetSerieLabel(serie, serieData);
            }
            var numericFormatter = serieLabel == null ? "" : serieLabel.numericFormatter;
            var serieName = serie.serieName;
            var dataName = serieData != null ? serieData.name : null;
            if (serieLabel.formatterFunction != null)
            {
                return serieLabel.formatterFunction(serieData.index, dataValue);
            }
            if (string.IsNullOrEmpty(serieLabel.formatter))
                return ChartCached.NumberToStr(dataValue, numericFormatter);
            else
            {
                var content = serieLabel.formatter;
                FormatterHelper.ReplaceSerieLabelContent(ref content, numericFormatter, dataValue,
                    dataTotal, serieName, dataName, dataName, color);
                return content;
            }
        }

        public static void SetGaugeLabelText(Serie serie)
        {
            var serieData = serie.GetSerieData(0);
            if (serieData == null) return;
            if (serieData.labelObject == null) return;
            var label = SerieHelper.GetSerieLabel(serie, serieData);
            if (label == null) return;
            var value = serieData.GetData(1);
            var total = serie.max;
            var content = SerieLabelHelper.GetFormatterContent(serie, serieData, value, total, null, Color.clear);
            serieData.labelObject.SetText(content);
            serieData.labelObject.SetLabelPosition(serie.context.center + label.offset);
            if (!ChartHelper.IsClearColor(label.textStyle.color))
            {
                serieData.labelObject.label.SetColor(label.textStyle.color);
            }
        }

        public static void UpdatePieLabelPosition(Serie serie, SerieData serieData)
        {
            if (serieData.labelObject == null) return;
            var startAngle = serie.context.startAngle;
            var currAngle = serieData.context.halfAngle;
            var currRad = currAngle * Mathf.Deg2Rad;
            var offsetRadius = serieData.context.offsetRadius;
            var insideRadius = serieData.context.insideRadius;
            var outsideRadius = serieData.context.outsideRadius;
            var serieLabel = SerieHelper.GetSerieLabel(serie, serieData);
            var labelLine = SerieHelper.GetSerieLabelLine(serie, serieData);
            switch (serieLabel.position)
            {
                case LabelStyle.Position.Center:
                    serieData.context.labelPosition = serie.context.center;
                    break;
                case LabelStyle.Position.Inside:
                    var labelRadius = offsetRadius + insideRadius + (outsideRadius - insideRadius) / 2 + serieLabel.distance;
                    var labelCenter = new Vector2(serie.context.center.x + labelRadius * Mathf.Sin(currRad),
                        serie.context.center.y + labelRadius * Mathf.Cos(currRad));
                    serieData.context.labelPosition = labelCenter;
                    break;
                case LabelStyle.Position.Outside:
                    if (labelLine != null && labelLine.lineType == LabelLine.LineType.HorizontalLine)
                    {
                        var radius1 = serie.context.outsideRadius;
                        var radius3 = insideRadius + (outsideRadius - insideRadius) / 2;
                        var currSin = Mathf.Sin(currRad);
                        var currCos = Mathf.Cos(currRad);
                        var pos0 = new Vector3(serie.context.center.x + radius3 * currSin, serie.context.center.y + radius3 * currCos);
                        if ((currAngle - startAngle) % 360 > 180)
                        {
                            currSin = Mathf.Sin((360 - currAngle) * Mathf.Deg2Rad);
                            currCos = Mathf.Cos((360 - currAngle) * Mathf.Deg2Rad);
                        }
                        var r4 = Mathf.Sqrt(radius1 * radius1 - Mathf.Pow(currCos * radius3, 2)) - currSin * radius3;
                        r4 += labelLine.lineLength1 + labelLine.lineWidth * 4;
                        r4 += serieData.labelObject.label.GetPreferredWidth() / 2;
                        serieData.context.labelPosition = pos0 + ((currAngle - startAngle) % 360 > 180 ? Vector3.left : Vector3.right) * r4;
                    }
                    else
                    {
                        labelRadius = serie.context.outsideRadius + (labelLine == null ? 0 : labelLine.lineLength1);
                        labelCenter = new Vector2(serie.context.center.x + labelRadius * Mathf.Sin(currRad),
                            serie.context.center.y + labelRadius * Mathf.Cos(currRad));
                        serieData.context.labelPosition = labelCenter;
                    }
                    break;
            }
        }

        public static void AvoidLabelOverlap(Serie serie, ComponentTheme theme)
        {
            if (!serie.avoidLabelOverlap) return;
            var lastCheckPos = Vector3.zero;
            var data = serie.data;
            var splitCount = 0;
            for (int n = 0; n < data.Count; n++)
            {
                var serieData = data[n];
                if (serieData.context.labelPosition.x != 0 && serieData.context.labelPosition.x < serie.context.center.x)
                {
                    splitCount = n;
                    break;
                }
            }
            for (int n = 0; n < splitCount; n++)
            {
                CheckSerieDataLabel(serie, data[n], false, theme, ref lastCheckPos);
            }
            lastCheckPos = Vector3.zero;
            for (int n = data.Count - 1; n >= splitCount; n--)
            {
                CheckSerieDataLabel(serie, data[n], true, theme, ref lastCheckPos);
            }
        }

        private static void CheckSerieDataLabel(Serie serie, SerieData serieData, bool isLeft, ComponentTheme theme,
            ref Vector3 lastCheckPos)
        {
            if (!serieData.context.canShowLabel)
            {
                serieData.SetLabelActive(false);
                return;
            }
            if (!serieData.show) return;
            var serieLabel = SerieHelper.GetSerieLabel(serie, serieData);
            var labelLine = SerieHelper.GetSerieLabelLine(serie, serieData);
            var fontSize = serieLabel.textStyle.GetFontSize(theme);
            if (!serieLabel.show) return;
            if (serieLabel.position != LabelStyle.Position.Outside) return;
            if (lastCheckPos == Vector3.zero)
            {
                lastCheckPos = serieData.context.labelPosition;
            }
            else if (serieData.context.labelPosition.x != 0)
            {
                if (lastCheckPos.y - serieData.context.labelPosition.y < fontSize)
                {
                    var labelRadius = serie.context.outsideRadius + labelLine.lineLength1;
                    var y1 = lastCheckPos.y - fontSize;
                    var cy = serie.context.center.y;
                    var diff = Mathf.Abs(y1 - cy);
                    var diffX = labelRadius * labelRadius - diff * diff;
                    diffX = diffX <= 0 ? 0 : diffX;
                    var x1 = serie.context.center.x + Mathf.Sqrt(diffX) * (isLeft ? -1 : 1);
                    serieData.context.labelPosition = new Vector3(x1, y1);
                }
                lastCheckPos = serieData.context.labelPosition;
                serieData.labelObject.SetPosition(SerieLabelHelper.GetRealLabelPosition(serie, serieData, serieLabel, labelLine));
            }
        }

        public static Vector3 GetRealLabelPosition(Serie serie, SerieData serieData, LabelStyle label, LabelLine labelLine)
        {
            if (label == null || labelLine == null)
                return serieData.context.labelPosition;

            if (label.position == LabelStyle.Position.Outside && labelLine.lineType != LabelLine.LineType.HorizontalLine)
            {
                var currAngle = serieData.context.halfAngle;
                var offset = labelLine.lineLength2 + serieData.labelObject.GetLabelWidth() / 2;
                if ((currAngle - serie.context.startAngle) % 360 > 180)
                    return serieData.context.labelPosition + new Vector3(-offset, 0, 0);
                else
                    return serieData.context.labelPosition + new Vector3(offset, 0, 0);
            }
            else
            {
                return serieData.context.labelPosition;
            }
        }
    }
}