using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ServiceLayer.Interface;
using System;
using System.Data;

namespace DBFactory
{
    public class TSQLFactory : IDisposable
    {
        private readonly ILoggerService _loggerService;

        private readonly IConfiguration Configuration;

        SqlConnection _con;
        SqlCommand _cmd;
        SqlDataAdapter _da;
        SqlTransaction _tran;

        ~TSQLFactory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            // If this function is being called the user wants to release the
            // resources. lets call the Dispose which will do this for us.
            Dispose(true);

            // Now since we have done the cleanup already there is nothing left
            // for the Finalizer to do. So lets tell the GC not to call it later.
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                //someone want the deterministic release of all resources
                //Let us release all the managed resources
                if (_cmd != null)
                {
                    _cmd.Dispose();
                }
                if (_con != null)
                {
                    if (_con.State == ConnectionState.Open)
                        _con.Close();
                    _con.Dispose();
                }

            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TSQLFactory()
        {
            var result = this.Configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(result))
            {
                _con = new SqlConnection(result);
            }
        }

        public void PrintStartExecutingSP()
        {
            _loggerService.Info("--------------- EXECUTING SP ---------------");
            _loggerService.Info("EXECUTING SP=  " + _cmd.CommandText + "  Start Time");
        }

        public void PrintEndExecutingSP()
        {
            _loggerService.Info("EXECUTING SP=  " + _cmd.CommandText + "  END Time");
        }

        /// <summary>
        /// Fill the DataTable and return the same.
        /// </summary>
        /// <returns>Datatable returne from the DB</returns>
        public DataTable GetTable()
        {
            PrintStartExecutingSP();
            DataTable dt = new DataTable();
            try
            {
                if (_con == null)
                {
                    throw new Exception("Connection object is not initialized");
                }
                else if (_cmd == null)
                {
                    throw new Exception("Command object is not initialized");
                }
                else if (_con.State != ConnectionState.Open)
                {
                    throw new Exception("Connection is not opened for the execution");
                }
                _da = new SqlDataAdapter(_cmd);
                _da.Fill(dt);
                PrintEndExecutingSP();
                return dt;
            }
            catch (Exception ex)
            {
                dt.Dispose();
                throw ex;
            }
            finally
            {

            }
        }


    }
}
