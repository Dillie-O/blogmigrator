using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlogMigrator
{
   public class LogData
   {
      public int sourceId { get; set; }
      public string sourceUrl { get; set; }
      public int destinationId { get; set; }
      public string destinationUrl { get; set; }

      public LogData()
      {
      }

      public LogData(int SourceId, string SourceUrl, int DestId, string DestUrl)
      {
         sourceId = SourceId;
         sourceUrl = SourceUrl;
         destinationId = DestId;
         destinationUrl = DestUrl;
      }
   }
}
