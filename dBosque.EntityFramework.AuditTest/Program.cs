using dBosque.EntityFramework.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AuditTest
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var db = new audittestEntities())
            {
                db.Settings.IsEnabled = true;
                db.Settings.UseDatabaseValueCompare = false;
                
                System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();
                w.Start();
                for (int ii = 0; ii < 100; ii++)
                {
                    var p1 = db.company.OrderByDescending(a => a.com_code).Take(2).ToList();
                    p1[1].com_name = Guid.NewGuid().ToString();
                    p1[1].com_phonenumber = Guid.NewGuid().ToString();

                    db.company.Remove(p1[0]);
                    db.company.Add(new company() { com_name = "name", com_address = "address", com_city = "city", com_code = Guid.NewGuid().ToString(), com_phonenumber = "1" });
                    db.SaveChanges();
                    Console.WriteLine(ii);
                }
                w.Stop();
                Console.WriteLine(w.ElapsedMilliseconds);
            }
        }
    }
}
