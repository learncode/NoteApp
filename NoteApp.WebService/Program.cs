using System;
using Microsoft.Owin.Hosting;

namespace NoteApp.WebService {
   class Program {
      static void Main() {
         const string baseAddress = "http://localhost:8181";

         using (WebApp.Start<Startup>(baseAddress)) {
            Console.WriteLine("Running Web Service at {0}. Press Enter to exit", baseAddress);
            Console.ReadLine();
         }
      }
   }
}
