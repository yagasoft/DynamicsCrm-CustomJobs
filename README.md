# DynamicsCrm-CustomJobs

[![Join the chat at https://gitter.im/yagasoft/DynamicsCrm-CustomJobs](https://badges.gitter.im/yagasoft/DynamicsCrm-CustomJobs.svg)](https://gitter.im/yagasoft/DynamicsCrm-CustomJobs?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

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
  + Platforms
    + Integrated into CRM
	+ Windows Service
	+ Soon: Azure WebJob
  + Supports logging, including exception details on every execution of an action
  + Contingency processes for failure recovery

### Install

Import solution found at [Dynamics365-YsCommonSolution](https://github.com/yagasoft/Dynamics365-YsCommonSolution).

### Guide

  + Set parameter values in Common or Generic Configuration table
  + Create a record in the Custom Job Engine entity
	+ Not required for 'Service' platform
	+ "Start the Engine"
  + Create a job in the Custom Job Entity
	+ "Enqueue" the job

I will post a complete guide soon.

### Dependencies

  + Common.cs, CommonGeneric.js, and CrmSchema.js
    + Can be found in the DynamicsCrm-Libraries repository
  + Generic Base solution ([Dynamics365-YsCommonSolution](https://github.com/yagasoft/Dynamics365-YsCommonSolution))
  + CRM Logger solution ([DynamicsCrm-CrmLogger](https://github.com/yagasoft/DynamicsCrm-CrmLogger))
		
## Changes
+ Check Releases page for the later changes
#### _v3.2.1.1 (2021-01-01)_
+ Added: Windows Service
+ Fixed: issues
#### _v3.1.1.1 (2019-02-27)_
+ Changed: moved to a new namespace
#### _v2.1.1.1 (2018-09-06)_
+ Changed: cleaned the project of obsolete components

---
**Copyright &copy; by Ahmed Elsawalhy ([Yagasoft](https://yagasoft.com))** -- _GPL v3 Licence_
