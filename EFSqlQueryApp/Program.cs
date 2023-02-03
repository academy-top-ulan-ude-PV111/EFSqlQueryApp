using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EFSqlQueryApp
{
    class Country
    {
        public int Id { set; get; }
        public string Title { set; get; } = null!;
        List<Company> Companies { set; get; } = new();
    }
    class Company
    {
        public int Id { set; get; }
        public string Title { set; get; } = null!;
        public Country? Country { set; get; }
        public int CountryId { set; get; }
        public List<Employe> Employes { set; get; } = new();
    }
    class Position
    {
        public int Id { set; get; }
        public string Title { set; get; } = null!;
        List<Employe> Employes { set; get; } = new();
    }
    class Employe
    {
        public int Id { set; get; }
        public string Name { set; get; } = null!;
        public Company? Company { set; get; }
        public int CompanyId { set; get; }
        public Position? Position { set; get; }
        public int PositionId { set; get; }

    }
    class AppContext : DbContext
    {
        public DbSet<Employe> Employes { set; get; }
        public DbSet<Company> Companies { set; get; }
        public DbSet<Country> Countries { set; get; }
        public DbSet<Position> Positions { set; get; }

        public IQueryable<Employe> GetByCompanyId(int companyId)
        {
            return FromExpression(() => GetByCompanyId(companyId));
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CompanyDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDbFunction(() => GetByCompanyId(default));

            // for LincFilterModel()

            //modelBuilder.Entity<Employe>()
            //            .HasQueryFilter(e => e.PositionId == 1);

        }
    }
    internal class Program
    {
        static void SqlQueries()
        {
            // SQL SELECT -> FromSqlRaw
            //
            //using(AppContext context = new())
            //{
            //    string sqlQuery = "SELECT * FROM Employes WHERE CompanyId LIKE @cId";
            //    string sqlQuery2 = "SELECT * FROM Employes WHERE CompanyId LIKE {0}";
            //    SqlParameter param = new("@cId", "1");
            //    int cId = 2;

            //    var employes = context.Employes
            //                          .FromSqlRaw(sqlQuery2, cId)
            //                          .Include(e => e.Company)
            //                          .OrderBy(e => e.Company!.Title);

            //    foreach(var e in employes.ToList())
            //        Console.WriteLine($"{e.Company!.Title} {e.Name}");
            //}

            // SQL INSERT UPDATE DELETE -> ExecuteSqlRaw
            //
            /*
            using (AppContext context = new())
            {
                SqlParameter[] param = new SqlParameter[] 
                { 
                    new("@name", "Phill"),
                    new("@CompanyId", "3"),
                    new("@PositionId", "2")
                };
                string sqlInsert = @"INSERT INTO Employes 
                                    (Name, CompanyId, PositionId)
                                    VALUES(@name, @CompanyId, @PositionId)";

                string name = "Phill";
                string nameNew = "Phillip";

                string sqlUpdate = @"UPDATE Employes SET Name = {0} WHERE Name = {1}";

                string sqlDelete = @"DELETE FROM Employes WHERE Name = {0}";


                //int result = context.Database.ExecuteSqlRaw(sqlInsert, param);
                //int result = context.Database.ExecuteSqlRaw(sqlUpdate, nameNew, name);
                //int result = context.Database.ExecuteSqlRaw(sqlDelete, nameNew);
                Console.WriteLine(result);
            }
            */

            // SQL Interpolated -> FromSqlInterpolated, ExecuteSqlInterpolated
            //
            using (AppContext context = new())
            {
                int cId = 1;
                FormattableString sqlQuery = $"SELECT * FROM Employes WHERE CompanyId LIKE {cId}";

                var employes = context.Employes
                                      .FromSqlInterpolated(sqlQuery)
                                      .Include(e => e.Company)
                                      .OrderBy(e => e.Company!.Title);

                foreach (var e in employes.ToList())
                    Console.WriteLine($"{e.Company!.Title} {e.Name}");


                //string name = "Phill";
                //int companyId = 3;
                //int positionId = 2;

                //FormattableString sqlInsert = $@"INSERT INTO Employes 
                //                    (Name, CompanyId, PositionId)
                //                    VALUES ({name}, {companyId}, {positionId})";

                string name = "Phill";
                string nameNew = "Phillip";

                //FormattableString sqlUpdate = $@"UPDATE Employes 
                //                                SET Name = {nameNew} 
                //                                WHERE Name = {name}";

                FormattableString sqlDelete = $@"DELETE FROM Employes WHERE Name = {nameNew}";


                //int result = context.Database.ExecuteSqlInterpolated(sqlInsert);
                //int result = context.Database.ExecuteSqlInterpolated(sqlUpdate);
                int result = context.Database.ExecuteSqlInterpolated(sqlDelete);
                Console.WriteLine(result);

            }
        }
        static void Main(string[] args)
        {
            //SqlQueries();
            using(AppContext context = new())
            {
                // SQL 
                //SqlParameter param = new("@companyId", 1);
                //string sqlFunction = "SELECT * FROM GetByCompanyId(@companyId)";


                //var employes = context.Employes
                //                      .FromSqlRaw(sqlFunction, param)
                //                      .Include(e => e.Company);


                // HasDbFunction
                //var employes = context.GetByCompanyId(1)
                //                      .Include(e => e.Company);

                //SqlParameter param = new("@company", "Yandex");

                //var employes = context.Employes
                //                      .FromSqlRaw("GetEmployeByCompany @company", param);

                //foreach(var e in employes.ToList())
                //Console.WriteLine($"{e.Name}");

                SqlParameter[] param = new SqlParameter[]{
                    new("@company", "Yandex"),
                    new()
                    {
                        ParameterName = "@count",
                        SqlDbType = System.Data.SqlDbType.Int,
                        Direction = System.Data.ParameterDirection.Output
                    },
                };

                context.Database
                       .ExecuteSqlRaw("GetCountEmployesByCompany @company, @count OUTPUT", param);

                Console.WriteLine(param[1].Value);
            }
        }
    }
}