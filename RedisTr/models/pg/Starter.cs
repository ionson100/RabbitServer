using System;
using System.Linq;
using ORM_1_21_;
using RedisTr.utils;

namespace RedisTr.models.pg
{
    public static class Starter
    {
        public static string MyHost = "host.docker.internal";
       public static void Start()
        {
            // todo Не решил как хранить секреты, на время тестов оставлю 
            new Configure($"Server={Auth.Host};Port=5432;Database=hotel;" +
                          $"User Id = {MPgSecret2.Secret.PgName}; " +
                          $"Password={MPgSecret2.Secret.PgPwd};",
                ProviderName.Postgresql, null);
            var ses=Configure.GetSession();
            var tr = ses.BeginTransaction();
            try
            {
              

                if (ses.TableExists<MPgLog>() == false)
                {
                    ses.TableCreate<MPgLog>();
                }

                if (ses.TableExists<MPrinter>() == false)
                {
                    ses.TableCreate<MPrinter>();
                }

                if (ses.TableExists<MHotel>() == false)
                {
                    ses.TableCreate<MHotel>();
                    string sql = " ALTER TABLE hotel ADD CONSTRAINT constraint_hotel UNIQUE(id);";
                    ses.ExecuteNonQuery(sql);

                }

                if (!ses.Querion<MHotel>().Any())
                {
                    MHotel hotel = new MHotel
                    {
                        CountryId = 267,
                        Description = "test",
                        IsActive = true,
                        Id = 100,
                        Name = "Тестовый отель"
                    };
                    ses.Save(hotel);
                    MPrinter mPrinter = new MPrinter()
                    {
                        HotelPkId = hotel.PkId,
                        Id = 1,
                        IsActive = true,
                        Description = "test"
                    };
                    ses.Save(mPrinter);
                }

                tr.Commit();

            }
            catch (Exception)
            {
                tr.Rollback();
               throw;
            }
            finally
            {
                ses.Dispose();
            }
        
           

            

         
        }

    }

    
}
