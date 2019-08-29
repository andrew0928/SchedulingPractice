--select * from jobs order by RunAt asc

select count(*), action
from workerlogs 
--where action = 'QUERYLIST'
group by action

select
	count(*) as total,
	avg(datediff(millisecond, RunAt, ExecuteAt)) as avg_delay,
	stdev(datediff(millisecond, RunAt, ExecuteAt)) as std_delay
from jobs
where State = 2 and RunAt < ExecuteAt



-------------------------------------------------------------------------------------------------------



--select * from jobs order by RunAt asc

select count(*) as count from workerlogs where action = 'CREATE'
select count(*) as count from workerlogs where action = 'ACQUIRE_SUCCESS'
select count(*) as count from workerlogs where action = 'ACQUIRE_FAILURE'
select count(*) as count from workerlogs where action = 'COMPLETE'
select count(*) as count from workerlogs where action = 'QUERYJOB'
select count(*) as count from workerlogs where action = 'QUERYLIST'

--  CREATE | ACQUIRE_SUCCESS | ACQUIRE_FAILURE | COMPLETE | QUERYJOB | QUERYLIST



--select
--	count(*) as total,
--	avg(datediff(millisecond, RunAt, ExecuteAt)) as avg_delay,
--	stdev(datediff(millisecond, RunAt, ExecuteAt)) as std_delay
--from jobs
--where State = 2 and RunAt < ExecuteAt

select count(*) as total from jobs where state = 0
select count(*) as total from jobs where state = 1
select count(*) as total from jobs where state = 2

select avg(datediff(millisecond, RunAt, ExecuteAt)) as average_delay from jobs where state = 2
select stdev(datediff(millisecond, RunAt, ExecuteAt)) as stdev_delay from jobs where state = 2



