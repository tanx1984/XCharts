
using System;
using UnityEngine;

namespace XCharts.Runtime
{
    [Serializable]
    [SerieHandler(typeof(LineHandler), true)]
    [SerieConvert(typeof(Bar), typeof(Pie))]
    [CoordOptions(typeof(GridCoord), typeof(PolarCoord))]
    [DefaultAnimation(AnimationType.LeftToRight)]
    [SerieExtraComponent(
        typeof(LabelStyle),
        typeof(EndLabelStyle),
        typeof(LineArrow),
        typeof(AreaStyle),
        typeof(IconStyle),
        typeof(Emphasis))]
    public class Line : Serie, INeedSerieContainer
    {
        public int containerIndex { get; internal set; }
        public int containterInstanceId { get; internal set; }
        public static Serie AddDefaultSerie(BaseChart chart, string serieName)
        {
            var serie = chart.AddSerie<Line>(serieName);
            serie.symbol.show = true;
            for (int i = 0; i < 5; i++)
            {
                chart.AddData(serie.index, UnityEngine.Random.Range(10, 90));
            }
            return serie;
        }

        public static Line CovertSerie(Serie serie)
        {
            var newSerie = serie.Clone<Line>();
            return newSerie;
        }
    }
}