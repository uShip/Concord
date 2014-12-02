using System.Collections.Generic;
using System.Linq;
using concord.Output;
using concord.Output.Dto;
using concord.Tests.Framework;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace concord.Tests.Output
{
    [TestFixture]
    public class HtmlGanttChartTests : InteractionContext<HtmlGanttChart>
    {
        [Test]
        public void Should_organize_this_data_correctly()
        {
            //Arrange
            var testData = GetTestData();

            //Act
            var results = ClassUnderTest.SeperateIntoLines(testData);

            //Assert
            results.Count.Should().Be(11);
            results.SelectMany(x => x).Count()
                   .Should().Be(testData.Count(x => x.TestRunId == 0));
        }

        [Test]
        public void Should_correctly_respect_testRunId_param()
        {
            //Arrange
            var testData = GetTestData();
            var testRunId = -1;

            //Act
            var results = ClassUnderTest.SeperateIntoLines(testData, testRunId);

            //Assert
            results.SelectMany(x => x).Count()
                   .Should().Be(testData.Count(x => x.TestRunId == testRunId));
        }


        private IEnumerable<RunStats> GetTestData()
        {
            var testData = @"[
{'StartTime':'00:05:06.2579228','EndTime':'00:05:35.8297707','TestRunId':0},
{'StartTime':'00:07:14.7025022','EndTime':'00:07:30.4260658','TestRunId':-1},
{'StartTime':'00:04:51.2569198','EndTime':'00:05:10.5561160','TestRunId':0},
{'StartTime':'00:04:22.2544235','EndTime':'00:04:51.0469285','TestRunId':0},
{'StartTime':'00:05:01.2573924','EndTime':'00:05:34.1272486','TestRunId':0},
{'StartTime':'00:00:02.1023684','EndTime':'00:03:02.0020451','TestRunId':0},
{'StartTime':'00:07:45.3785921','EndTime':'00:08:39.4092359','TestRunId':0},
{'StartTime':'00:02:52.2324768','EndTime':'00:04:52.4365489','TestRunId':0},
{'StartTime':'00:04:15.2537919','EndTime':'00:05:27.3532336','TestRunId':0},
{'StartTime':'00:08:33.7184411','EndTime':'00:09:18.1692986','TestRunId':-1},
{'StartTime':'00:06:36.5358081','EndTime':'00:07:05.8650187','TestRunId':0},
{'StartTime':'00:01:02.2194707','EndTime':'00:02:50.2600177','TestRunId':0},
{'StartTime':'00:03:49.7514005','EndTime':'00:04:14.9061795','TestRunId':0},
{'StartTime':'00:04:27.2550128','EndTime':'00:05:20.6761743','TestRunId':0},
{'StartTime':'00:08:35.6093662','EndTime':'00:08:48.7618246','TestRunId':0},
{'StartTime':'00:03:18.2350241','EndTime':'00:04:27.2031300','TestRunId':0},
{'StartTime':'00:06:30.4670428','EndTime':'00:07:25.1624354','TestRunId':0},
{'StartTime':'00:03:05.7337061','EndTime':'00:04:21.8339747','TestRunId':0},
{'StartTime':'00:08:15.9942718','EndTime':'00:08:48.7878185','TestRunId':0},
{'StartTime':'00:08:42.7193984','EndTime':'00:09:06.7860687','TestRunId':-1},
{'StartTime':'00:00:00.1477860','EndTime':'00:01:06.3257330','TestRunId':-1},
{'StartTime':'00:02:50.7324045','EndTime':'00:03:43.4281886','TestRunId':0},
{'StartTime':'00:01:28.2228486','EndTime':'00:01:52.0966463','TestRunId':0},
{'StartTime':'00:07:06.1630141','EndTime':'00:07:32.2783064','TestRunId':0},
{'StartTime':'00:02:14.7286677','EndTime':'00:02:31.1403666','TestRunId':0},
{'StartTime':'00:07:02.1517826','EndTime':'00:07:34.9400467','TestRunId':0},
{'StartTime':'00:09:21.7235708','EndTime':'00:09:41.6087308','TestRunId':-1},
{'StartTime':'00:00:00.1476034','EndTime':'00:01:00.6287751','TestRunId':-1},
{'StartTime':'00:08:14.9917130','EndTime':'00:08:48.7714729','TestRunId':0},
{'StartTime':'00:01:52.2271724','EndTime':'00:03:17.9883423','TestRunId':0},
{'StartTime':'00:01:57.2276404','EndTime':'00:02:50.2898153','TestRunId':0},
{'StartTime':'00:06:10.3271689','EndTime':'00:06:55.1908473','TestRunId':0},
{'StartTime':'00:08:21.5288201','EndTime':'00:08:48.7597559','TestRunId':0},
{'StartTime':'00:00:00.1937264','EndTime':'00:01:03.6823933','TestRunId':0},
{'StartTime':'00:02:04.7281822','EndTime':'00:02:23.4781259','TestRunId':0},
{'StartTime':'00:04:49.7568220','EndTime':'00:05:49.0801508','TestRunId':0},
{'StartTime':'00:05:10.7591213','EndTime':'00:05:49.2707645','TestRunId':0},
{'StartTime':'00:07:45.3785974','EndTime':'00:08:14.5407414','TestRunId':0},
{'StartTime':'00:01:25.2207015','EndTime':'00:03:05.0364423','TestRunId':0},
{'StartTime':'00:03:44.7509637','EndTime':'00:04:10.3830750','TestRunId':0},
{'StartTime':'00:00:00.1937363','EndTime':'00:01:13.6867997','TestRunId':0},
{'StartTime':'00:08:44.7325879','EndTime':'00:09:32.2532854','TestRunId':-1},
{'StartTime':'00:07:05.1623330','EndTime':'00:07:52.0568937','TestRunId':0},
{'StartTime':'00:01:24.2259294','EndTime':'00:01:56.7483380','TestRunId':0},
{'StartTime':'00:00:59.7190023','EndTime':'00:01:24.9795299','TestRunId':0},
{'StartTime':'00:03:34.2371088','EndTime':'00:05:09.7795525','TestRunId':0},
{'StartTime':'00:00:00.1953773','EndTime':'00:03:16.8461844','TestRunId':0},
{'StartTime':'00:03:02.2333306','EndTime':'00:04:35.3434772','TestRunId':0},
{'StartTime':'00:00:00.1477742','EndTime':'00:01:01.3208430','TestRunId':-1},
{'StartTime':'00:00:01.0384577','EndTime':'00:01:53.8617747','TestRunId':0},
{'StartTime':'00:06:44.5765499','EndTime':'00:07:45.0412182','TestRunId':0},
{'StartTime':'00:03:43.7508682','EndTime':'00:05:01.6814531','TestRunId':0},
{'StartTime':'00:07:35.3495269','EndTime':'00:08:35.2698688','TestRunId':0},
{'StartTime':'00:08:42.1762485','EndTime':'00:08:48.7555508','TestRunId':0},
{'StartTime':'00:01:13.7206034','EndTime':'00:03:05.3976737','TestRunId':0},
{'StartTime':'00:06:03.2790571','EndTime':'00:07:02.0471309','TestRunId':0},
{'StartTime':'00:09:18.2232009','EndTime':'00:09:30.9245812','TestRunId':-1},
{'StartTime':'00:02:31.2294801','EndTime':'00:03:44.2926077','TestRunId':0},
{'StartTime':'00:07:32.3497735','EndTime':'00:08:34.5787704','TestRunId':0},
{'StartTime':'00:02:40.7319053','EndTime':'00:03:00.7328203','TestRunId':0},
{'StartTime':'00:08:56.2208378','EndTime':'00:09:39.1261448','TestRunId':-1},
{'StartTime':'00:05:01.7575758','EndTime':'00:06:10.0927410','TestRunId':0},
{'StartTime':'00:05:50.7633918','EndTime':'00:07:03.7399148','TestRunId':0},
{'StartTime':'00:00:00.1936971','EndTime':'00:01:27.9151612','TestRunId':0},
{'StartTime':'00:04:10.7533455','EndTime':'00:04:49.2864781','TestRunId':0},
{'StartTime':'00:08:42.6784279','EndTime':'00:08:48.7449593','TestRunId':0},
{'StartTime':'00:08:14.9917084','EndTime':'00:08:41.9501684','TestRunId':0},
{'StartTime':'00:08:16.4943144','EndTime':'00:08:46.4287148','TestRunId':0},
{'StartTime':'00:02:23.7290867','EndTime':'00:02:40.4350331','TestRunId':0},
{'StartTime':'00:07:35.3495269','EndTime':'00:08:09.3332723','TestRunId':0},
{'StartTime':'00:06:23.9234424','EndTime':'00:06:53.6526509','TestRunId':0},
{'StartTime':'00:08:36.6094827','EndTime':'00:08:48.7432034','TestRunId':0},
{'StartTime':'00:03:19.2351276','EndTime':'00:03:49.3176683','TestRunId':0},
{'StartTime':'00:06:06.2891453','EndTime':'00:06:33.1756568','TestRunId':0},
{'StartTime':'00:09:16.7230339','EndTime':'00:09:39.1611071','TestRunId':-1},
{'StartTime':'00:06:15.3514187','EndTime':'00:06:44.3265609','TestRunId':0},
{'StartTime':'00:03:05.2337069','EndTime':'00:03:34.2006432','TestRunId':0},
{'StartTime':'00:09:07.2220157','EndTime':'00:09:31.5991723','TestRunId':-1},
{'StartTime':'00:03:17.2349141','EndTime':'00:03:44.6427518','TestRunId':0},
{'StartTime':'00:07:52.4097899','EndTime':'00:08:42.4922799','TestRunId':0},
{'StartTime':'00:08:09.4840109','EndTime':'00:08:41.7758808','TestRunId':0},
{'StartTime':'00:00:00.1966565','EndTime':'00:05:10.6172613','TestRunId':0},
{'StartTime':'00:08:01.2140686','EndTime':'00:08:26.3395929','TestRunId':-1},
{'StartTime':'00:00:00.1954055','EndTime':'00:00:59.3311256','TestRunId':0},
{'StartTime':'00:03:58.2522224','EndTime':'00:05:22.4573108','TestRunId':0},
{'StartTime':'00:00:00.1952799','EndTime':'00:01:32.9843822','TestRunId':0},
{'StartTime':'00:04:35.7558035','EndTime':'00:05:06.2326784','TestRunId':0},
{'StartTime':'00:03:34.2371137','EndTime':'00:03:57.9142168','TestRunId':0},
{'StartTime':'00:05:36.2620335','EndTime':'00:06:02.8947083','TestRunId':0},
{'StartTime':'00:05:49.7624014','EndTime':'00:06:23.8654289','TestRunId':0},
{'StartTime':'00:07:43.3584158','EndTime':'00:08:15.7853682','TestRunId':0},
{'StartTime':'00:04:52.7569883','EndTime':'00:06:05.8836982','TestRunId':0},
{'StartTime':'00:08:50.2243367','EndTime':'00:09:16.4635736','TestRunId':-1},
{'StartTime':'00:03:00.7332526','EndTime':'00:03:33.9353877','TestRunId':0},
{'StartTime':'00:02:50.7324045','EndTime':'00:03:18.8627495','TestRunId':0},
{'StartTime':'00:03:44.7509637','EndTime':'00:05:00.7988054','TestRunId':0},
{'StartTime':'00:06:55.6053712','EndTime':'00:07:44.9564235','TestRunId':0},
{'StartTime':'00:00:01.5742639','EndTime':'00:01:02.1157580','TestRunId':0},
{'StartTime':'00:07:23.8138668','EndTime':'00:08:14.7602285','TestRunId':0},
{'StartTime':'00:06:31.9756825','EndTime':'00:07:04.8407534','TestRunId':0},
{'StartTime':'00:08:16.7222828','EndTime':'00:08:55.7844585','TestRunId':-1},
{'StartTime':'00:01:29.7236530','EndTime':'00:02:52.1779062','TestRunId':0},
{'StartTime':'00:08:26.7222383','EndTime':'00:08:53.2502393','TestRunId':-1},
{'StartTime':'00:07:47.8827428','EndTime':'00:08:36.1251228','TestRunId':0},
{'StartTime':'00:05:58.7633515','EndTime':'00:06:23.8454006','TestRunId':0},
{'StartTime':'00:08:10.7164900','EndTime':'00:08:33.4483082','TestRunId':-1},
{'StartTime':'00:05:20.7593427','EndTime':'00:05:50.5166511','TestRunId':0},
{'StartTime':'00:05:10.7598883','EndTime':'00:06:15.3053177','TestRunId':0},
{'StartTime':'00:07:48.8844820','EndTime':'00:08:21.4382398','TestRunId':0},
{'StartTime':'00:01:33.2252753','EndTime':'00:02:04.5795268','TestRunId':0},
{'StartTime':'00:07:04.1626268','EndTime':'00:07:48.8687406','TestRunId':0},
{'StartTime':'00:06:33.4995761','EndTime':'00:07:34.9847106','TestRunId':0},
{'StartTime':'00:05:22.7595623','EndTime':'00:06:36.4132203','TestRunId':0},
{'StartTime':'00:08:53.7205718','EndTime':'00:09:17.0202147','TestRunId':-1},
{'StartTime':'00:06:54.1056185','EndTime':'00:07:23.6178777','TestRunId':0},
{'StartTime':'00:06:23.9227301','EndTime':'00:08:16.4513144','TestRunId':0},
{'StartTime':'00:01:03.7196636','EndTime':'00:01:23.9064234','TestRunId':0},
{'StartTime':'00:00:00.1476045','EndTime':'00:01:05.2922862','TestRunId':-1},
{'StartTime':'00:08:34.5979981','EndTime':'00:08:48.7486694','TestRunId':0},
{'StartTime':'00:07:04.6622722','EndTime':'00:07:42.8719664','TestRunId':0},
{'StartTime':'00:05:49.2623702','EndTime':'00:06:31.8337909','TestRunId':0},
{'StartTime':'00:05:10.2598762','EndTime':'00:06:30.0981678','TestRunId':0},
{'StartTime':'00:08:46.6910067','EndTime':'00:08:48.7186835','TestRunId':0},
{'StartTime':'00:08:42.1762428','EndTime':'00:08:48.7437292','TestRunId':0},
{'StartTime':'00:07:25.3819898','EndTime':'00:07:47.5573864','TestRunId':0},
{'StartTime':'00:05:27.7601045','EndTime':'00:05:58.3288366','TestRunId':0},
{'StartTime':'00:05:34.2608316','EndTime':'00:07:04.6444457','TestRunId':0},
{'StartTime':'00:00:00.1937032','EndTime':'00:01:29.6498478','TestRunId':0},
{'StartTime':'00:01:54.2273315','EndTime':'00:02:14.3398942','TestRunId':0},
{'StartTime':'00:08:31.7219541','EndTime':'00:09:06.8439852','TestRunId':-1},
{'StartTime':'00:08:39.6457968','EndTime':'00:08:48.7212704','TestRunId':0}]";

            return JsonConvert.DeserializeObject<List<RunStats>>(testData);
        }
    }
}