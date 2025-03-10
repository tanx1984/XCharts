using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XCharts.Runtime
{
    public static class LegendHelper
    {
        public static Color GetContentColor(int legendIndex, Legend legend, ThemeStyle theme, bool active)
        {
            var textStyle = legend.textStyle;
            if (active)
            {
                if (legend.textAutoColor) return theme.GetColor(legendIndex);
                else return !ChartHelper.IsClearColor(textStyle.color) ? textStyle.color : theme.legend.textColor;
            }
            else return theme.legend.unableColor;
        }

        public static Color GetIconColor(BaseChart chart, Legend legend, int readIndex, string legendName, bool active)
        {
            if (active)
            {
                if (legend.itemAutoColor || legend.GetIcon(readIndex) == null)
                {
                    return SeriesHelper.GetNameColor(chart, readIndex, legendName);
                }
                else
                    return Color.white;
            }
            else return chart.theme.legend.unableColor;
        }

        public static LegendItem AddLegendItem(Legend legend, int i, string legendName, Transform parent,
            ThemeStyle theme, string content, Color itemColor, bool active, int legendIndex)
        {
            var objName = i + "_" + legendName;
            var anchorMin = new Vector2(0, 0.5f);
            var anchorMax = new Vector2(0, 0.5f);
            var pivot = new Vector2(0, 0.5f);
            var sizeDelta = new Vector2(100, 30);
            var iconSizeDelta = new Vector2(legend.itemWidth, legend.itemHeight);
            var textStyle = legend.textStyle;
            var contentColor = GetContentColor(legendIndex, legend, theme, active);

            var objAnchorMin = new Vector2(0, 1);
            var objAnchorMax = new Vector2(0, 1);
            var objPivot = new Vector2(0, 1);
            var btnObj = ChartHelper.AddObject(objName, parent, objAnchorMin, objAnchorMax, objPivot, sizeDelta, i);
            var iconObj = ChartHelper.AddObject("icon", btnObj.transform, anchorMin, anchorMax, pivot, iconSizeDelta);
            var contentObj = ChartHelper.AddObject("content", btnObj.transform, anchorMin, anchorMax, pivot, sizeDelta);
            var img = ChartHelper.GetOrAddComponent<Image>(btnObj);
            img.color = Color.clear;
            ChartHelper.GetOrAddComponent<Button>(btnObj);
            ChartHelper.GetOrAddComponent<Image>(iconObj);
            ChartHelper.GetOrAddComponent<Image>(contentObj);
            var txt = ChartHelper.AddTextObject("Text", contentObj.transform, anchorMin, anchorMax, pivot, sizeDelta,
                textStyle, theme.legend);
            txt.SetAlignment(textStyle.GetAlignment(TextAnchor.MiddleLeft));
            txt.SetColor(contentColor);
            var item = new LegendItem();
            item.index = i;
            item.name = objName;
            item.legendName = legendName;
            item.SetObject(btnObj);
            item.SetIconSize(legend.itemWidth, legend.itemHeight);
            item.SetIconColor(itemColor);
            item.SetIconImage(legend.GetIcon(i));
            item.SetContentPosition(textStyle.offsetv3);
            item.SetContent(content);
            item.SetContentBackgroundColor(textStyle.backgroundColor);
            return item;
        }

        public static void ResetItemPosition(Legend legend, Vector3 chartPos, float chartWidth, float chartHeight)
        {
            legend.location.UpdateRuntimeData(chartWidth, chartHeight);
            var startX = 0f;
            var startY = 0f;
            var legendMaxWidth = chartWidth - legend.location.runtimeLeft - legend.location.runtimeRight;
            var legendMaxHeight = chartHeight - legend.location.runtimeTop - legend.location.runtimeBottom;
            UpdateLegendWidthAndHeight(legend, legendMaxWidth, legendMaxHeight);
            var legendRuntimeWidth = legend.context.width;
            var legendRuntimeHeight = legend.context.height;
            var isVertical = legend.orient == Orient.Vertical;
            switch (legend.location.align)
            {
                case Location.Align.TopCenter:
                    startX = chartPos.x + chartWidth / 2 - legendRuntimeWidth / 2;
                    startY = chartPos.y + chartHeight - legend.location.runtimeTop;
                    break;
                case Location.Align.TopLeft:
                    startX = chartPos.x + legend.location.runtimeLeft;
                    startY = chartPos.y + chartHeight - legend.location.runtimeTop;
                    break;
                case Location.Align.TopRight:
                    startX = chartPos.x + chartWidth - legendRuntimeWidth - legend.location.runtimeRight;
                    startY = chartPos.y + chartHeight - legend.location.runtimeTop;
                    break;
                case Location.Align.Center:
                    startX = chartPos.x + chartWidth / 2 - legendRuntimeWidth / 2;
                    startY = chartPos.y + chartHeight / 2 + legendRuntimeHeight / 2;
                    break;
                case Location.Align.CenterLeft:
                    startX = chartPos.x + legend.location.runtimeLeft;
                    startY = chartPos.y + chartHeight / 2 + legendRuntimeHeight / 2;
                    break;
                case Location.Align.CenterRight:
                    startX = chartPos.x + chartWidth - legendRuntimeWidth - legend.location.runtimeRight;
                    startY = chartPos.y + chartHeight / 2 + legendRuntimeHeight / 2;
                    break;
                case Location.Align.BottomCenter:
                    startX = chartPos.x + chartWidth / 2 - legendRuntimeWidth / 2;
                    startY = chartPos.y + legendRuntimeHeight + legend.location.runtimeBottom;
                    break;
                case Location.Align.BottomLeft:
                    startX = chartPos.x + legend.location.runtimeLeft;
                    startY = chartPos.y + legendRuntimeHeight + legend.location.runtimeBottom;
                    break;
                case Location.Align.BottomRight:
                    startX = chartPos.x + chartWidth - legendRuntimeWidth - legend.location.runtimeRight;
                    startY = chartPos.y + legendRuntimeHeight + legend.location.runtimeBottom;
                    break;
            }
            if (isVertical) SetVerticalItemPosition(legend, legendMaxHeight, startX, startY);
            else SetHorizonalItemPosition(legend, legendMaxWidth, startX, startY);
        }

        private static void SetVerticalItemPosition(Legend legend, float legendMaxHeight, float startX, float startY)
        {
            var currHeight = 0f;
            var offsetX = 0f;
            var row = 0;
            foreach (var kv in legend.context.buttonList)
            {
                var item = kv.Value;
                if (currHeight + item.height > legendMaxHeight)
                {
                    currHeight = 0;
                    offsetX += legend.context.eachWidthDict[row];
                    row++;
                }
                item.SetPosition(new Vector3(startX + offsetX, startY - currHeight));
                currHeight += item.height + legend.itemGap;
            }
        }
        private static void SetHorizonalItemPosition(Legend legend, float legendMaxWidth, float startX, float startY)
        {
            var currWidth = 0f;
            var offsetY = 0f;
            foreach (var kv in legend.context.buttonList)
            {
                var item = kv.Value;
                if (currWidth + item.width > legendMaxWidth)
                {
                    currWidth = 0;
                    offsetY += legend.context.eachHeight;
                }
                item.SetPosition(new Vector3(startX + currWidth, startY - offsetY));
                currWidth += item.width + legend.itemGap;
            }
        }

        private static void UpdateLegendWidthAndHeight(Legend legend, float maxWidth, float maxHeight)
        {
            var width = 0f;
            var height = 0f;
            var realHeight = 0f;
            var realWidth = 0f;
            legend.context.eachWidthDict.Clear();
            legend.context.eachHeight = 0;
            if (legend.orient == Orient.Horizonal)
            {
                foreach (var kv in legend.context.buttonList)
                {
                    if (width + kv.Value.width > maxWidth)
                    {
                        realWidth = width - legend.itemGap;
                        realHeight += height + legend.itemGap;
                        if (legend.context.eachHeight < height + legend.itemGap)
                        {
                            legend.context.eachHeight = height + legend.itemGap;
                        }
                        height = 0;
                        width = 0;
                    }
                    width += kv.Value.width + legend.itemGap;
                    if (kv.Value.height > height)
                        height = kv.Value.height;
                }
                width -= legend.itemGap;
                legend.context.height = realHeight + height;
                legend.context.width = realWidth > 0 ? realWidth : width;
            }
            else
            {
                var row = 0;
                foreach (var kv in legend.context.buttonList)
                {
                    if (height + kv.Value.height > maxHeight)
                    {
                        realHeight = height - legend.itemGap;
                        realWidth += width + legend.itemGap;
                        legend.context.eachWidthDict[row] = width + legend.itemGap;
                        row++;
                        height = 0;
                        width = 0;
                    }
                    height += kv.Value.height + legend.itemGap;
                    if (kv.Value.width > width)
                        width = kv.Value.width;
                }
                height -= legend.itemGap;
                legend.context.height = realHeight > 0 ? realHeight : height;
                legend.context.width = realWidth + width;
            }
        }

        private static bool IsBeyondWidth(Legend legend, float maxWidth)
        {
            var totalWidth = 0f;
            foreach (var kv in legend.context.buttonList)
            {
                var item = kv.Value;
                totalWidth += item.width + legend.itemGap;
                if (totalWidth > maxWidth) return true;
            }
            return false;
        }

        public static bool CheckDataShow(Serie serie, string legendName, bool show)
        {
            bool needShow = false;
            if (legendName.Equals(serie.serieName))
            {
                serie.show = show;
                serie.highlight = false;
                if (serie.show) needShow = true;
            }
            else
            {
                foreach (var data in serie.data)
                {
                    if (legendName.Equals(data.name))
                    {
                        data.show = show;
                        data.context.highlight = false;
                        if (data.show) needShow = true;
                    }
                }
            }
            return needShow;
        }

        public static bool CheckDataHighlighted(Serie serie, string legendName, bool heighlight)
        {
            bool show = false;
            if (legendName.Equals(serie.serieName))
            {
                serie.highlight = heighlight;
            }
            else
            {
                foreach (var data in serie.data)
                {
                    if (legendName.Equals(data.name))
                    {
                        data.context.highlight = heighlight;
                        if (data.context.highlight) show = true;
                    }
                }
            }
            return show;
        }
    }
}