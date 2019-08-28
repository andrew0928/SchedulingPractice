using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SchedulingPractice.Core
{
    public class JobsRepo : IDisposable
    {
        private SqlConnection _conn = null;
        private readonly string _client = null;
        private Semaphore _flags = new Semaphore(5, 5);

        public JobsRepo(string connstr = null, string client = null)
        {
            if (string.IsNullOrEmpty(client))
            {
                this._client = $"pid:{System.Diagnostics.Process.GetCurrentProcess().Id}";
            }
            else
            {
                this._client = client;
            }

            if (string.IsNullOrEmpty(connstr))
            {
                this._conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=JobsDB;Integrated Security=True;Pooling=False");
            }
            else
            {
                this._conn = new SqlConnection(connstr);
            }
        }

        public IEnumerable<JobInfo> GetReadyJobs(TimeSpan duration)
        {
            return this._conn.Query<JobInfo>(
                @"
select * from [jobs] where state = 0 and datediff(millisecond, getdate(), runat) < @msec order by runat asc;
insert [workerlogs] (action, clientid, results) values ('QUERYLIST', @client, @@rowcount);
",
                new
                {
                    msec = duration.TotalMilliseconds,
                    client = this._client,
                });
        }

        public JobInfo GetJob(int jobid)
        {
            return this._conn.QuerySingle<JobInfo>(
                @"
select * from [jobs] where id = @jobid;
insert [workerlogs] (jobid, action, clientid, results) values (@jobid, 'QUERYJOB', @client, 1);
",
                new
                {
                    jobid,
                    client = this._client
                });
        }

        public int CreateJob(DateTime runat)
        {
            return _conn.ExecuteScalar<int>(
    @"
--
-- create job definition
--
declare @id int;
insert [jobs] (RunAt) values (@runat); 
set @id = @@identity;
insert [workerlogs] (jobid, action, clientid) values (@id, 'CREATE', @clientid);
select @id;
",
                new
                {
                    runat,
                    clientid = _client,
                });
        }

        public bool AcquireJobLock(int jobId)
        {
            return this._conn.Execute(
                @"
update [jobs] set state = 1 where id = @id and state = 0;
insert [workerlogs] (jobid, action, clientid) values (@id, case @@rowcount when 1 then 'ACQUIRE_SUCCESS' else 'ACQUIRE_FAIL' end, @clientid);
",
                new
                {
                    id = jobId,
                    clientid = this._client,
                }) == 2;
        }

        public bool ProcessLockedJob(int jobId)
        {
            this._flags.WaitOne();
            Task.Delay(83).Wait();
            this._flags.Release();

            return this._conn.Execute(
                @"
update [jobs] set state = 2, executeat = getdate() where id = @id and state = 1;
insert [workerlogs] (jobid, action, clientid) values (@id, case @@rowcount when 1 then 'COMPLETE' else 'ERROR' end, @clientid);
",
                new
                {
                    id = jobId,
                    clientid = _client,
                }) == 2;
        }

        public void ResetDatabase()
        {
            this._conn.Execute(@"truncate table [jobs]; truncate table [workerlogs];");
        }

        public void Dispose()
        {
            this._conn.Close();
        }
    }

}
