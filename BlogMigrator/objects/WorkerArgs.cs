using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlogMigrator
{
   /// <summary>
   /// This object stores the various items used when working with the various
   /// processes on the background worker thread. Since most objects are not
   /// accessible to the worker thread, we simply pass the object along to the
   /// worker and retrive what the worker did when complete.
   /// </summary>
   class WorkerArgs
   {
      public string processToRun { get; set; }        
      public string status { get; set; }
   }
}
