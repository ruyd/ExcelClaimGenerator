using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelClaimGenerator
{

    public class ErrorLog
    {
        public int ID { get; set; }
        public string StackTrace { get; set; }
        public string Module { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public string SourceName { get; set; }
        public DateTime? DateTimeCreated { get; set; } = DateTime.UtcNow;
        public string Data { get; set; }
        public int Filter { get; set; }
         
    }

    public partial class TypeEx
    {
        public static ErrorLog ToLog(this Exception ex)
        {
            ErrorLog l = new ErrorLog();
            l.Message = ex.InnerException?.Message ?? ex.Message;
            l.StackTrace = ex.StackTrace;
            l.Module = ex.TargetSite?.Name;
            l.SourceName = ex.Source;
    

            return l;
        }
    }


}
