﻿
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using XUGL;

namespace XCharts.Runtime
{
    /// <summary>
    /// Tooltip component.
    /// |提示框组件。
    /// </summary>
    [System.Serializable]
    [ComponentHandler(typeof(TooltipHandler), true)]
    public class Tooltip : MainComponent
    {
        /// <summary>
        /// Indicator type.
        /// |指示器类型。
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// line indicator.
            /// |直线指示器
            /// </summary>
            Line,
            /// <summary>
            /// shadow crosshair indicator.
            /// |阴影指示器
            /// </summary>
            Shadow,
            /// <summary>
            /// no indicator displayed.
            /// |无指示器
            /// </summary>
            None,
            /// <summary>
            /// crosshair indicator, which is actually the shortcut of enable two axisPointers of two orthometric axes.
            /// |十字准星指示器。坐标轴显示Label和交叉线。
            /// </summary>
            Corss
        }

        public enum Trigger
        {
            /// <summary>
            /// Triggered by data item, which is mainly used for charts that don't have a category axis like scatter charts or pie charts.
            /// |数据项图形触发，主要在散点图，饼图等无类目轴的图表中使用。
            /// </summary>
            Item,
            /// <summary>
            /// Triggered by axes, which is mainly used for charts that have category axes, like bar charts or line charts.
            /// |坐标轴触发，主要在柱状图，折线图等会使用类目轴的图表中使用。
            /// </summary>
            Axis,
            /// <summary>
            /// Trigger nothing.
            /// |什么都不触发。
            /// </summary>
            None
        }

        [SerializeField] private bool m_Show = true;
        [SerializeField] private Type m_Type;
        [SerializeField] private Trigger m_Trigger = Trigger.Item;
        [SerializeField] private string m_ItemFormatter;
        [SerializeField] private string m_TitleFormatter;
        [SerializeField] private string m_Marker = "●";
        [SerializeField] private float m_FixedWidth = 0;
        [SerializeField] private float m_FixedHeight = 0;
        [SerializeField] private float m_MinWidth = 0;
        [SerializeField] private float m_MinHeight = 0;
        [SerializeField] private string m_NumericFormatter = "";
        [SerializeField] private int m_PaddingLeftRight = 10;
        [SerializeField] private int m_PaddingTopBottom = 10;
        [SerializeField] private bool m_IgnoreDataShow = false;
        [SerializeField] private string m_IgnoreDataDefaultContent = "-";
        [SerializeField] private bool m_ShowContent = true;
        [SerializeField] private bool m_AlwayShowContent = false;
        [SerializeField] private Vector2 m_Offset = new Vector2(18f, -25f);
        [SerializeField] private Sprite m_BackgroundImage;
        [SerializeField] private Color m_BackgroundColor;
        [SerializeField] private float m_BorderWidth = 2f;
        [SerializeField] private bool m_FixedXEnable = false;
        [SerializeField] private float m_FixedX = 0f;
        [SerializeField] private bool m_FixedYEnable = false;
        [SerializeField] private float m_FixedY = 0f;
        [SerializeField] private float m_TitleHeight = 25f;
        [SerializeField] private float m_ItemHeight = 25f;
        [SerializeField] private Color32 m_BorderColor = new Color32(230, 230, 230, 255);
        [SerializeField] private LineStyle m_LineStyle = new LineStyle(LineStyle.Type.None);
        [SerializeField] private TextStyle m_LabelTextStyle = new TextStyle();
        [SerializeField] private TextStyle m_TitleTextStyle = new TextStyle() { alignment = TextAnchor.MiddleLeft };
        [SerializeField]
        private List<TextStyle> m_ColumnsTextStyle = new List<TextStyle>() {
            new TextStyle() { alignment = TextAnchor.MiddleLeft, extraWidth = 5 },
            new TextStyle() { alignment = TextAnchor.MiddleLeft, extraWidth = 20 },
            new TextStyle() { alignment = TextAnchor.MiddleRight, extraWidth = 5 }
        };

        public TooltipContext context = new TooltipContext();
        public TooltipView view;

        /// <summary>
        /// Whether to show the tooltip component.
        /// |是否显示提示框组件。
        /// </summary>
        public bool show
        {
            get { return m_Show; }
            set { if (PropertyUtil.SetStruct(ref m_Show, value)) { SetAllDirty(); SetActive(value); } }
        }
        /// <summary>
        /// Indicator type.
        /// |提示框指示器类型。
        /// </summary>
        public Type type
        {
            get { return m_Type; }
            set { if (PropertyUtil.SetStruct(ref m_Type, value)) SetAllDirty(); }
        }
        /// <summary>
        /// Type of triggering.
        /// |触发类型。
        /// </summary>
        public Trigger trigger
        {
            get { return m_Trigger; }
            set { if (PropertyUtil.SetStruct(ref m_Trigger, value)) SetAllDirty(); }
        }
        /// <summary>
        /// The string template formatter for the tooltip title content. Support for wrapping lines with \n.
        /// The placeholder {I} can be set separately to indicate that the title is ignored and not displayed.
        /// Template variables are {.}, {a}, {b}, {c}, {d}.</br>
        /// {.} is the dot of the corresponding color of a Serie that is currently indicated or whose index is 0.</br>
        /// {a} is the series name of the serie that is currently indicated or whose index is 0.</br>
        /// {b} is the name of the data item serieData that is currently indicated or whose index is 0, or a category value (such as the X-axis of a line chart).</br>
        /// {c} is the value of a Y-dimension (dimesion is 1) from a Serie that is currently indicated or whose index is 0.</br>
        /// {d} is the percentage value of Y-dimensions (dimesion is 1) from serie that is currently indicated or whose index is 0, with no % sign.</br>
        /// {e} is the name of the data item serieData that is currently indicated or whose index is 0.</br>
        /// {.1} represents a dot from serie corresponding color that specifies index as 1.</br>
        /// 1 in {a1}, {b1}, {c1} represents a serie that specifies an index of 1.</br>
        /// {c1:2} represents the third data from serie's current indication data item indexed to 1 (a data item has multiple data, index 2 represents the third data).</br>
        /// {c1:2-2} represents the third data item from serie's third data item indexed to 1 (i.e., which data item must be specified to specify).</br>
        /// {d1:2: F2} indicates that a formatted string with a value specified separately is F2 (numericFormatter is used when numericFormatter is not specified).</br>
        /// {d:0.##} indicates that a formatted string with a value specified separately is 0.##   (used for percentage, reserved 2 valid digits while avoiding the situation similar to "100.00%" when using f2 ).</br>
        /// Example: "{a}, {c}", "{a1}, {c1: f1}", "{a1}, {c1:0: f1}", "{a1} : {c1:1-1: f1}"</br>
        /// |提示框标题内容的字符串模版格式器。支持用 \n 换行。可以单独设置占位符{i}表示忽略不显示title。</br>
        /// 模板变量有{.}、{a}、{b}、{c}、{d}、{e}。</br>
        /// {.}为当前所指示或index为0的serie的对应颜色的圆点。</br>
        /// {a}为当前所指示或index为0的serie的系列名name。</br>
        /// {b}为当前所指示或index为0的serie的数据项serieData的name，或者类目值（如折线图的X轴）。</br>
        /// {c}为当前所指示或index为0的serie的y维（dimesion为1）的数值。</br>
        /// {d}为当前所指示或index为0的serie的y维（dimesion为1）百分比值，注意不带%号。</br>
        /// {e}为当前所指示或index为0的serie的数据项serieData的name。</br>
        /// {.1}表示指定index为1的serie对应颜色的圆点。</br>
        /// {a1}、{b1}、{c1}中的1表示指定index为1的serie。</br>
        /// {c1:2}表示索引为1的serie的当前指示数据项的第3个数据（一个数据项有多个数据，index为2表示第3个数据）。</br>
        /// {c1:2-2}表示索引为1的serie的第3个数据项的第3个数据（也就是要指定第几个数据项时必须要指定第几个数据）。</br>
        /// {d1:2:f2}表示单独指定了数值的格式化字符串为f2（不指定时用numericFormatter）。</br>
        /// {d:0.##} 表示单独指定了数值的格式化字符串为 0.## （用于百分比，保留2位有效数同时又能避免使用 f2 而出现的类似于"100.00%"的情况 ）。</br>
        /// 示例："{a}:{c}"、"{a1}:{c1:f1}"、"{a1}:{c1:0:f1}"、"{a1}:{c1:1-1:f1}"
        /// </summary>
        public string titleFormatter { get { return m_TitleFormatter; } set { m_TitleFormatter = value; } }
        /// <summary>
        /// a string template formatter for a single Serie or data item content. Support for wrapping lines with \n.
        /// When formatter is not null, use formatter first, otherwise use itemFormatter.
        /// |提示框单个serie或数据项内容的字符串模版格式器。支持用 \n 换行。当formatter不为空时，优先使用formatter，否则使用itemFormatter。
        /// </summary>
        public string itemFormatter { get { return m_ItemFormatter; } set { m_ItemFormatter = value; } }
        /// <summary>
        /// the marker of serie.
        /// |serie的符号标志。
        /// </summary>
        public string marker { get { return m_Marker; } set { m_Marker = value; } }
        /// <summary>
        /// Fixed width. Higher priority than minWidth.
        /// |固定宽度。比 minWidth 优先。
        /// </summary>
        public float fixedWidth { get { return m_FixedWidth; } set { m_FixedWidth = value; } }
        /// <summary>
        /// Fixed height. Higher priority than minHeight.
        /// |固定高度。比 minHeight 优先。
        /// </summary>
        public float fixedHeight { get { return m_FixedHeight; } set { m_FixedHeight = value; } }
        /// <summary>
        /// Minimum width. If fixedWidth has a value, get fixedWidth first.
        /// |最小宽度。如若 fixedWidth 设有值，优先取 fixedWidth。
        /// </summary>
        public float minWidth { get { return m_MinWidth; } set { m_MinWidth = value; } }
        /// <summary>
        /// Minimum height. If fixedHeight has a value, take priority over fixedHeight.
        /// |最小高度。如若 fixedHeight 设有值，优先取 fixedHeight。
        /// </summary>
        public float minHeight { get { return m_MinHeight; } set { m_MinHeight = value; } }
        /// <summary>
        /// Standard numeric format string. Used to format numeric values to display as strings.
        /// Using 'Axx' form: 'A' is the single character of the format specifier, supporting 'C' currency, 
        /// 'D' decimal, 'E' exponent, 'F' number of vertices, 'G' regular, 'N' digits, 'P' percentage, 
        /// 'R' round tripping, 'X' hex etc. 'XX' is the precision specification, from '0' - '99'.
        /// |标准数字格式字符串。用于将数值格式化显示为字符串。
        /// 使用Axx的形式：A是格式说明符的单字符，支持C货币、D十进制、E指数、F定点数、G常规、N数字、P百分比、R往返、X十六进制的。xx是精度说明，从0-99。
        /// 参考：https://docs.microsoft.com/zh-cn/dotnet/standard/base-types/standard-numeric-format-strings
        /// </summary>
        /// <value></value>
        public string numericFormatter
        {
            get { return m_NumericFormatter; }
            set { if (PropertyUtil.SetClass(ref m_NumericFormatter, value)) SetComponentDirty(); }
        }
        /// <summary>
        /// the text padding of left and right. defaut:5.
        /// |左右边距。
        /// </summary>
        public int paddingLeftRight { get { return m_PaddingLeftRight; } set { m_PaddingLeftRight = value; } }
        /// <summary>
        /// the text padding of top and bottom. defaut:5.
        /// |上下边距。
        /// </summary>
        public int paddingTopBottom { get { return m_PaddingTopBottom; } set { m_PaddingTopBottom = value; } }
        /// <summary>
        /// Whether to show ignored data on tooltip.
        /// |是否显示忽略数据在tooltip上。
        /// </summary>
        public bool ignoreDataShow { get { return m_IgnoreDataShow; } set { m_IgnoreDataShow = value; } }
        /// <summary>
        /// The default display character information for ignored data.
        /// |被忽略数据的默认显示字符信息。
        /// </summary>
        public string ignoreDataDefaultContent { get { return m_IgnoreDataDefaultContent; } set { m_IgnoreDataDefaultContent = value; } }
        /// <summary>
        /// The background image of tooltip.
        /// |提示框的背景图片。
        /// </summary>
        public Sprite backgroundImage { get { return m_BackgroundImage; } set { m_BackgroundImage = value; SetComponentDirty(); } }
        /// <summary>
        /// The background color of tooltip.
        /// |提示框的背景颜色。
        /// </summary>
        public Color backgroundColor { get { return m_BackgroundColor; } set { m_BackgroundColor = value; SetComponentDirty(); } }
        /// <summary>
        /// Whether to trigger after always display.
        /// |是否触发后一直显示提示框浮层。
        /// </summary>
        public bool alwayShowContent { get { return m_AlwayShowContent; } set { m_AlwayShowContent = value; } }
        /// <summary>
        /// Whether to show the tooltip floating layer, whose default value is true.
        /// It should be configurated to be false, if you only need tooltip to trigger the event or show the axisPointer without content.
        /// |是否显示提示框浮层，默认显示。只需tooltip触发事件或显示axisPointer而不需要显示内容时可配置该项为false。
        /// </summary>
        public bool showContent { get { return m_ShowContent; } set { m_ShowContent = value; } }
        /// <summary>
        /// The position offset of tooltip relative to the mouse position.
        /// |提示框相对于鼠标位置的偏移。
        /// </summary>
        public Vector2 offset { get { return m_Offset; } set { m_Offset = value; } }
        /// <summary>
        /// the width of tooltip border.
        /// |边框线宽。
        /// </summary>
        public float borderWidth
        {
            get { return m_BorderWidth; }
            set { if (PropertyUtil.SetStruct(ref m_BorderWidth, value)) SetVerticesDirty(); }
        }
        /// <summary>
        /// the color of tooltip border.
        /// |边框颜色。
        /// </summary>
        public Color32 borderColor
        {
            get { return m_BorderColor; }
            set { if (PropertyUtil.SetColor(ref m_BorderColor, value)) SetVerticesDirty(); }
        }
        public bool fixedXEnable
        {
            get { return m_FixedXEnable; }
            set { if (PropertyUtil.SetStruct(ref m_FixedXEnable, value)) SetVerticesDirty(); }
        }
        public float fixedX
        {
            get { return m_FixedX; }
            set { if (PropertyUtil.SetStruct(ref m_FixedX, value)) SetVerticesDirty(); }
        }
        public bool fixedYEnable
        {
            get { return m_FixedYEnable; }
            set { if (PropertyUtil.SetStruct(ref m_FixedYEnable, value)) SetVerticesDirty(); }
        }
        public float fixedY
        {
            get { return m_FixedY; }
            set { if (PropertyUtil.SetStruct(ref m_FixedY, value)) SetVerticesDirty(); }
        }
        public float titleHeight
        {
            get { return m_TitleHeight; }
            set { if (PropertyUtil.SetStruct(ref m_TitleHeight, value)) SetComponentDirty(); }
        }
        public float itemHeight
        {
            get { return m_ItemHeight; }
            set { if (PropertyUtil.SetStruct(ref m_ItemHeight, value)) SetComponentDirty(); }
        }
        /// <summary>
        /// the text style of content.
        /// |提示框标签的文本样式。
        /// </summary>
        public TextStyle labelTextStyle
        {
            get { return m_LabelTextStyle; }
            set { if (value != null) { m_LabelTextStyle = value; SetComponentDirty(); } }
        }
        /// <summary>
        /// 标题的文本样式。
        /// </summary>
        public TextStyle titleTextStyle
        {
            get { return m_TitleTextStyle; }
            set { if (value != null) { m_TitleTextStyle = value; SetComponentDirty(); } }
        }

        public List<TextStyle> columnsTextStyle
        {
            get { return m_ColumnsTextStyle; }
            set { if (value != null) { m_ColumnsTextStyle = value; SetComponentDirty(); } }
        }

        /// <summary>
        /// the line style of indicator line.
        /// |指示线样式。
        /// </summary>
        public LineStyle lineStyle
        {
            get { return m_LineStyle; }
            set { if (value != null) m_LineStyle = value; SetComponentDirty(); }
        }

        /// <summary>
        /// 组件是否需要刷新
        /// </summary>
        public override bool componentDirty
        {
            get { return m_ComponentDirty || lineStyle.componentDirty || labelTextStyle.componentDirty; }
        }

        public override void ClearComponentDirty()
        {
            base.ClearComponentDirty();
            lineStyle.ClearComponentDirty();
            labelTextStyle.ClearComponentDirty();
        }
        /// <summary>
        /// 当前提示框所指示的Serie索引（目前只对散点图有效）。
        /// </summary>
        public Dictionary<int, List<int>> runtimeSerieIndex = new Dictionary<int, List<int>>();
        /// <summary>
        /// The data index currently indicated by Tooltip.
        /// |当前提示框所指示的数据项索引。
        /// </summary>
        public List<int> runtimeDataIndex { get { return m_RuntimeDateIndex; } internal set { m_RuntimeDateIndex = value; } }
        private List<int> m_RuntimeDateIndex = new List<int>() { -1, -1 };

        /// <summary>
        /// Keep Tooltiop displayed at the top.
        /// |保持Tooltiop显示在最顶上
        /// </summary>
        public void KeepTop()
        {
            gameObject.transform.SetAsLastSibling();
        }

        public override void ClearData()
        {
            ClearValue();
        }

        /// <summary>
        /// 清除提示框指示数据
        /// </summary>
        internal void ClearValue()
        {
            for (int i = 0; i < runtimeDataIndex.Count; i++) runtimeDataIndex[i] = -1;
        }

        /// <summary>
        /// 提示框是否显示
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
        {
            return gameObject != null && gameObject.activeInHierarchy;
        }

        /// <summary>
        /// 设置Tooltip组件是否显示
        /// </summary>
        /// <param name="flag"></param>
        public void SetActive(bool flag)
        {
            if (gameObject && gameObject.activeInHierarchy != flag)
            {
                gameObject.SetActive(alwayShowContent ? true : flag);
            }
            SetContentActive(flag);
        }

        /// <summary>
        /// 更新文本框位置
        /// </summary>
        /// <param name="pos"></param>
        public void UpdateContentPos(Vector2 pos)
        {
            if (view != null)
            {
                if (fixedXEnable) pos.x = fixedX;
                if (fixedYEnable) pos.y = fixedY;
                view.UpdatePosition(pos);
            }
        }

        /// <summary>
        /// 设置文本框是否显示
        /// </summary>
        /// <param name="flag"></param>
        public void SetContentActive(bool flag)
        {
            if (view == null)
                return;
            
            view.SetActive(alwayShowContent ? true : flag);
        }

        /// <summary>
        /// 当前提示框是否选中数据项
        /// </summary>
        /// <returns></returns>
        public bool IsSelected()
        {
            foreach (var index in runtimeDataIndex)
                if (index >= 0) return true;
            return false;
        }

        /// <summary>
        /// 指定索引的数据项是否被提示框选中
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsSelected(int index)
        {
            foreach (var temp in runtimeDataIndex)
                if (temp == index) return true;
            return false;
        }

        public void ClearSerieDataIndex()
        {
            foreach (var kv in runtimeSerieIndex)
            {
                kv.Value.Clear();
            }
        }

        public void AddSerieDataIndex(int serieIndex, int dataIndex)
        {
            if (!runtimeSerieIndex.ContainsKey(serieIndex))
            {
                runtimeSerieIndex[serieIndex] = new List<int>();
            }
            runtimeSerieIndex[serieIndex].Add(dataIndex);
        }

        public bool isAnySerieDataIndex()
        {
            foreach (var kv in runtimeSerieIndex)
            {
                if (kv.Value.Count > 0) return true;
            }
            return false;
        }

        public bool IsTriggerItem()
        {
            return trigger == Trigger.Item;
        }

        public bool IsTriggerAxis()
        {
            return trigger == Trigger.Axis;
        }

        public TextStyle GetColumnTextStyle(int index)
        {
            if (m_ColumnsTextStyle.Count == 0)
                return null;

            if (index < 0)
                index = 0;
            else if (index > m_ColumnsTextStyle.Count - 1)
                index = m_ColumnsTextStyle.Count - 1;

            return m_ColumnsTextStyle[index];
        }
    }
}