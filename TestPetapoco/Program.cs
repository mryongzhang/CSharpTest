using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DefaultConnection;

//////////////////////////////////////////////////////////////////////////
// 测试Petapoco，连接AdventureWorks库
// 两种方式：1，标准的通过自动生成的表对象进行访问
//          2, 通过dynamic方式访问（无需事先定义跟表对应的类）
namespace TestPetapoco
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var db = new DefaultConnectionDB())
            {
                IEnumerable<Address> adds = db.Query<Address>("select top 1000 * from Person.Address");

                foreach (var a in adds)
                {
                    Console.WriteLine(a.City);
                }

                long count = db.ExecuteScalar<long>("select count(*) from Person.Address");
                Console.WriteLine("count:{0}", count);

                db.Dispose();
            }

            using (var db = new DefaultConnectionDB())
            {
                IEnumerable<dynamic> adds = db.Query<dynamic>("select top 1000 * from Person.Address");
                foreach (var a in adds)
                {
                    Console.WriteLine(a.City);
                }
                db.Dispose();                
            }

            Console.ReadKey();
        }
    }
}
