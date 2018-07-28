# DynamicsCrm-CustomJobs
### Version: 1.1
---

A very powerful CRM solution that gives a multitude of options to control scheduling jobs in CRM.

### Features

  + Action
    + Either an action or a workflow
    + Actions take input parameters as JSON
    + Option to delete jobs on success
    + Failure action
      + Includes max retry counts
      + A retry schedule
      + Failure and retry expiry actions
  + Target
    + Single target or multiple targets
    + For multiple targets
      + Specify filter using GUI supports paging
      + Supports paging
  + Timers
    + Define a countdown for execution
    + Option to take into account the working hours
  + Recurrent jobs
    + Supports granularity of 1 minute
    + Supports ‘nth day of the month’ pattern
    + Supports exclusions
      + Can define range of dates
      + Can define days, months, … etc.
      + Exclusions can be grouped for easier reference
  + Supports logging, including exception details on every execution of an action
  + Contingency processes for failure recovery

### Guide

  + Create a record in the Custom Job Engine entity
	+ "Start the Engine"
  + Create a job in the Custom Job Entity
	+ "Enqueue" the job

I will post a complete guide soon.

### Dependencies

  + Common.cs, CommonGeneric.js, and CrmSchema.js
    + Can be found in the DynamicsCrm-Libraries repository
  + Generic Base solution
  + CRM Logger solution

---
**Copyright &copy; by Ahmed el-Sawalhy ([YagaSoft](http://yagasoft.com))** -- _GPL v3 Licence_
