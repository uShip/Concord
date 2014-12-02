using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using concord.Output.Dto;

namespace concord.Output
{
    public interface IHtmlGanttChart
    {
        string GenerateGanttChart(IEnumerable<RunStats> runners);

        //IEnumerable<LineData> BuildLineData(IEnumerable<RunStats> runners);

        /// <summary>
        /// This duplicates much of the logic in DrawHorizontalLines... but using only RunStats
        /// </summary>
        /// <param name="runners"></param>
        /// <param name="testRunId"></param>
        /// <returns></returns>
        /// <remarks>Note: This will ignore any RunStats where TestRunId does not equal testRunId</remarks>
        List<List<RunStats>> SeperateIntoLines(IEnumerable<RunStats> runners, int testRunId = 0);
    }

    public class HtmlGanttChart : IHtmlGanttChart
    {
        public string GenerateGanttChart(IEnumerable<RunStats> runners)
        {
            var data = runners.OrderBy(x => x.StartOrder);

            var DIVIDEND = 1000;
            var graphData = data.Select(x => new LineData
            {
                Offset = (float)x.StartTime.TotalMilliseconds / DIVIDEND,
                Length = (float)(x.EndTime - x.StartTime).TotalMilliseconds / DIVIDEND,
                Name = x.Name,
                Success = x.IsSuccess
            });
            var graphOptions = new LineDrawOptions
            {
                BarPadding = 2,
                BarHeight = 10
            };
            return DrawHorizontalLines(graphData, graphOptions);
        }

        /// <summary>
        /// This duplicates much of the logic in DrawHorizontalLines... but using only RunStats
        /// </summary>
        /// <param name="runners"></param>
        /// <param name="testRunId"></param>
        /// <returns></returns>
        public List<List<RunStats>> SeperateIntoLines(IEnumerable<RunStats> runners, int testRunId = 0)
        {
            Func<RunStats, double> getOffset = stats => stats.StartTime.TotalMilliseconds;
            Func<RunStats, double> getLength = x => (x.EndTime - x.StartTime).TotalMilliseconds;


            var data = runners.Where(x => x.TestRunId == testRunId).ToList();

            var outputContainer = new List<List<RunStats>>();
            var currentLine = new List<RunStats>();

            RunStats d = null;
            while (data.Count > 0)
            {
                if (d == null)
                    d = data.First(x => getOffset(x) == data.Min(y => getOffset(y)));
                else
                {
                    var d1 = d;
                    var filtered = data.Where(x => getOffset(x) >= (getOffset(d1) + getLength(d1))).ToArray();
                    d = filtered.FirstOrDefault(x => getOffset(x) == filtered.Min(y => getOffset(y)));
                }
                if (d == null)
                {
                    outputContainer.Add(currentLine);
                    currentLine = new List<RunStats>();
                    continue;
                }
                currentLine.Add(d);
                data.Remove(d);
            }

            outputContainer.Add(currentLine);

            return outputContainer;
        }

        string DrawHorizontalLines(IEnumerable<LineData> rawData, LineDrawOptions opts)
        {
            var data = rawData.ToList();

            var lineHeight = opts.BarHeight + opts.BarPadding;

            var h = (int)Math.Ceiling(data.Count * lineHeight);
            var w = (int)Math.Ceiling(data.Max(x => x.Offset + x.Length + 1));

            var g = new HtmlDraw();

                var i = 0;
                LineData d = null;
            while (data.Count > 0)
            {
                if (d == null)
                    d = data.First(x => x.Offset == data.Min(y => y.Offset));
                else
                {
                    LineData d1 = d;
                    var filtered = data.Where(x => x.Offset >= (d1.Offset + d1.Length)).ToArray();
                    d = filtered.FirstOrDefault(x => x.Offset == filtered.Min(y => y.Offset));
                }
                if (d == null)
                {
                    ++i;
                    continue;
                }
                g.DrawRectangle(
                    d.Success ? "#00008B" : "#8B0000",
                    d.Success ? "#0000FF" : "#FF0000",
                    1, d.Offset, i * lineHeight, d.Length, opts.BarHeight, d.Name);
                data.Remove(d);
            }

            return g.ToString();
        }
        class LineDrawOptions
        {
            public float BarHeight { get; set; }
            public float BarPadding { get; set; }
        }
        class LineData
        {
            public float Offset { get; set; }
            public float Length { get; set; }

            public string Name { get; set; }
            public bool Success { get; set; }
        }

        public class HtmlDraw
        {
            StringBuilder sb = new StringBuilder();
            private int _maxWidth = 0;
            private int _maxHeight = 0;

            ///Draw a relatively positioned rectangle
            public void DrawRectangle(string fillColor, string borderColor, float borderWidth, float x, float y, float width, float height, string title = null)
            {
                sb.AppendFormat("<div style='position:absolute;border:solid {0} {1}px;background-color:{2};left:{3}px;top:{4}px;width:{5}px;height:{6}px' {7}></div>",
                    borderColor,
                    borderWidth,
                    fillColor,
                    x, y,
                    width, height,
                    title == null ? "" : "title='" + title + "'");
                if (_maxWidth < (int)(x + width))
                    _maxWidth = (int)(x + width);
                if (_maxHeight < (int)(y + height))
                    _maxHeight = (int)(y + height);
            }

            public override string ToString()
            {
                //return sb.ToString();
                return string.Format("<div style='position:relative;width:{1}px;height:{2}px'>{0}</div>", sb, _maxWidth, _maxHeight);
            }
        }
    }
}