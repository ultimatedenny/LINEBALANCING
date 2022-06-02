using System;

namespace LineBalancing.Helpers
{
    public static class IDHelper
    {
        public static string LineBalancing(string _lineName, int _lastInsertId)
        {
            var code = "LB";
            var lineName = _lineName;
            var dateTimeNow = DateTime.Now.ToString("yyMMdd");

            if (_lastInsertId == 0)
            {
                _lastInsertId = 1;
            }
            else
            {
                _lastInsertId++;
            }

            var sequenceNo = _lastInsertId.ToString("D5");

            var idFormat = code + "-" + lineName + "-" + dateTimeNow + sequenceNo;
            return idFormat;
        }
    }
}