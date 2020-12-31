/// <reference path="Sdk.Soap.vsdoc.js" />
/// <reference path="jquery-1.11.1.js" />
/// <reference path="Sdk.Soap.vsdoc.js" />
/// <reference path="Xrm.Page.js" />
/// <reference path="../../../../Yagasoft.Libraries/Yagasoft.Libraries.Common/CrmSchemaJs.js" />
/// <reference path="ldv_CommonGeneric.js" />

var Ys = (function(ys)
{
    const customJob = (function()
    {
        const fields =
        {
            Url: "ys_url"
        };

        return {
            Fields: fields
        };
    })();
    return Object.assign(ys,
        {
            CustomJob: customJob
        });
})(Ys || {});


function CustomJob_OnLoad()
{
	SetFieldSubmitMode(Sdk.CustomJob.StatusReason, SubmitMode.Always);
	SetFieldSubmitMode(Sdk.CustomJob.RecurrenceUpdatedTrigger, SubmitMode.Always);
	SetFieldSubmitMode(Sdk.CustomJob.RecurrentJob, SubmitMode.Always);

	if (GetFormType() === FormType.Create)
	{
        SetFieldValue(Sdk.CustomJob.StatusReason, Sdk.CustomJob.StatusReasonEnum.Draft);
	}
	else
	{
		SetFormLockedState();
		IsRecurrentJob_OnChange(true);
	}

	Workflow_OnChange();
	Action_OnChange();
	Url_OnChange();

	TargetName_OnChange(true);
	RecordsPerPage_OnChange();

	LoadAutoAdvancedFind(Sdk.CustomJob.TargetXML, Sdk.CustomJob.TargetLogicalName);

    if (GetFieldValue(Sdk.CustomJob.StatusReason) !== Sdk.CustomJob.StatusReasonEnum.Draft)
	{
		HideNonUsedSections();
	}
}

function CustomJob_OnSave()
{
	ExecuteOnNotDirty(SetFormLockedState);

    if (GetFieldValue(Sdk.CustomJob.Status) === Sdk.CustomJob.StatusEnum.Inactive
		|| IsFieldDirty(Sdk.CustomJob.StatusReason))
	{
		RefreshOnNotDirty();
	}
}

function TargetName_OnChange(isOnLoad)
{
	var entityName = GetFieldValue(Sdk.CustomJob.TargetLogicalName);

	if (entityName)
	{
		SetSectionsVisible(['TargetTab:SingleRecordSection', 'TargetTab:MultipleRecordsSection',
			'TargetTab:TargetXmlSection'], true);

		TargetId_OnChange();
		TargetXml_OnChange();
	}
	else
	{
		ClearControlError(Sdk.CustomJob.TargetID);
		SetFieldValue(Sdk.CustomJob.TargetID, null);
		SetFieldValue(Sdk.CustomJob.TargetXML, null);
		SetFieldValue(Sdk.CustomJob.RecordsPerPage, null, true);
		SetFieldRequired(Sdk.CustomJob.TargetID, false);
		SetFieldRequired(Sdk.CustomJob.TargetXML, false);

		SetSectionsVisible(['TargetTab:SingleRecordSection', 'TargetTab:MultipleRecordsSection',
			'TargetTab:TargetXmlSection'], false);
	}

	if (!isOnLoad)
	{
		ClearFieldValue(Sdk.CustomJob.ActionName);
	}

	SetupActionNames(entityName, Sdk.CustomJob.ActionName);
}

function TargetId_OnChange()
{
	if (GetFieldValue(Sdk.CustomJob.TargetID))
	{
		if (GetFieldValue(Sdk.CustomJob.TargetXML))
		{
			ShowControlError(Sdk.CustomJob.TargetID, 'Please clear XML field.');
		}

		SetFieldRequired(Sdk.CustomJob.TargetXML, false);
		SetFieldValue(Sdk.CustomJob.RecordsPerPage, null, true);
	}
	else
	{
		SetFieldRequired(Sdk.CustomJob.TargetXML, true);
		ClearControlError(Sdk.CustomJob.TargetID);
	}
}

function TargetXml_OnChange()
{
	if (GetFieldValue(Sdk.CustomJob.TargetXML))
	{
		if (GetFieldValue(Sdk.CustomJob.TargetID))
		{
			SetFieldValue(Sdk.CustomJob.TargetID, null, true);
		}

		SetFieldRequired(Sdk.CustomJob.TargetID, false);
	}
	else
	{
		SetFieldRequired(Sdk.CustomJob.TargetID, true);
		ClearControlError(Sdk.CustomJob.TargetID);
	}
}

function RecordsPerPage_OnChange()
{
	if (GetFieldValue(Sdk.CustomJob.RecordsPerPage))
	{
		SetFieldValue(Sdk.CustomJob.ContinueOnError, true, true);
		SetFieldVisible(Sdk.CustomJob.ContinueOnError, false);
	}
	else
	{
		SetFieldVisible(Sdk.CustomJob.ContinueOnError, true);
	}
}

function Workflow_OnChange()
{
	if (GetFieldValue(Sdk.CustomJob.Workflow))
	{
		if (GetFieldValue(Sdk.CustomJob.ActionName))
		{
			SetFieldValue(Sdk.CustomJob.ActionName, null, true);
			SetFieldValue(Sdk.CustomJob.SerialisedInputParams, null, true);
		}

		if (GetFieldValue(Ys.CustomJob.Fields.Url))
		{
			SetFieldValue(Ys.CustomJob.Fields.Url, null, true);
			SetFieldValue(Sdk.CustomJob.SerialisedInputParams, null, true);
		}

		SetFieldRequired(Sdk.CustomJob.ActionName, false);
		SetFieldRequired(Ys.CustomJob.Fields.Url, false);
		SetFieldRequired(Sdk.CustomJob.TargetLogicalName, true);
	}
	else
	{
		SetFieldRequired(Sdk.CustomJob.ActionName, true);
		SetFieldRequired(Sdk.CustomJob.TargetLogicalName, false);
		SetFieldRequired(Ys.CustomJob.Fields.Url, true);
	}
}

function Action_OnChange()
{
	if (GetFieldValue(Sdk.CustomJob.ActionName))
	{
		if (GetFieldValue(Sdk.CustomJob.Workflow))
		{
			SetFieldValue(Sdk.CustomJob.Workflow, null, true);
		}

		if (GetFieldValue(Ys.CustomJob.Fields.Url))
		{
			SetFieldValue(Ys.CustomJob.Fields.Url, null, true);
		}

		SetFieldRequired(Sdk.CustomJob.Workflow, false);
		SetFieldRequired(Ys.CustomJob.Fields.Url, false);
	}
	else
	{
		SetFieldRequired(Sdk.CustomJob.Workflow, true);
        SetFieldRequired(Ys.CustomJob.Fields.Url, true);
	}
}

function Url_OnChange()
{
	if (GetFieldValue(Ys.CustomJob.Fields.Url))
	{
		if (GetFieldValue(Sdk.CustomJob.Workflow))
		{
			SetFieldValue(Sdk.CustomJob.Workflow, null, true);
		}

		if (GetFieldValue(Sdk.CustomJob.ActionName))
		{
			SetFieldValue(Sdk.CustomJob.ActionName, null, true);
		}

		SetFieldRequired(Sdk.CustomJob.Workflow, false);
		SetFieldRequired(Sdk.CustomJob.ActionName, false);
	}
	else
	{
        SetFieldRequired(Sdk.CustomJob.Workflow, true);
        SetFieldRequired(Sdk.CustomJob.ActionName, true);
        SetFieldRequired(Sdk.CustomJob.TargetLogicalName, false);
	}
}

function Timer_OnChange()
{
	if (GetFieldValue(Sdk.CustomJob.Timer))
	{
		if (GetFieldValue(Sdk.CustomJob.TargetDate))
		{
			SetFieldValue(Sdk.CustomJob.TargetDate, null, true);
		}
	}
}

function Date_OnChange()
{
	ValidateDateFieldInFuture(Sdk.CustomJob.TargetDate);

	if (GetFieldValue(Sdk.CustomJob.TargetDate))
	{
		if (GetFieldValue(Sdk.CustomJob.Timer))
		{
			SetFieldValue(Sdk.CustomJob.Timer, null, true);
			SetFieldValue(Sdk.CustomJob.TimerBase, null, true);
		}
	}
}

function IsRecurrentJob_OnChange(isOnLoad)
{
	if (GetFieldValue(Sdk.CustomJob.RecurrentJob))
	{
		if (GetFormType() === FormType.Create)
		{
			SetFieldValue(Sdk.CustomJob.RecurrentJob, false);
			alert("Please save first before setting job as recurrent.");
		}
		else
		{
			SetSectionVisible('RecurrenceTab', 'RecurrenceRulesSection', true);
		}
	}
	else
	{
		IsRecurrentJob(function(isRecurrent)
		{
			if (!isRecurrent)
			{
				SetSectionVisible('RecurrenceTab', 'RecurrenceRulesSection', false);
			}
			else
			{
				SetFieldValue(Sdk.CustomJob.RecurrentJob, true, true);

				if (!isOnLoad)
				{
					alert("Please clear recurrent config records from grid first before setting job as non-recurrent.");
				}
			}
		});
	}
}

function IsRecurrentJob(callback)
{
		ShowBusyIndicator('Checking recurrent job config records ...', 'IsRecurrentJob');

		$.ajax({
				type: "GET",
				contentType: "application/json; charset=utf-8",
				datatype: "json",
				url: Xrm.Page.context.getClientUrl() +
					"/XRMServices/2011/OrganizationData.svc/ldv_customjobSet?" +
					"$select=ldv_ldv_customjob_ldv_recurrencerule/ldv_recurrenceruleId" +
					"&$expand=ldv_ldv_customjob_ldv_recurrencerule" +
					"&$filter=ldv_customjobId eq (guid'" + GetRecordId(true) + "')&$top=1",
				beforeSend: function(xmlHttpRequest)
				{
					xmlHttpRequest.setRequestHeader("Accept", "application/json");
				},
				async: true,
				success: function(data, textStatus, xhr)
				{
					var results = data.d.results;

					HideBusyIndicator('IsRecurrentJob');

					if (callback)
					{
						callback(results.length && results[0].ldv_ldv_customjob_ldv_recurrencerule.results.length);
					}
				},
				error: function(xhr, textStatus, errorThrown)
				{
					console.error(xhr);
					console.error(textStatus + ": " + errorThrown);

					HideBusyIndicator('IsRecurrentJob');

					if (callback)
					{
						callback(true);
					}
				}
			});
}

function SetFormLockedState()
{
    if (GetFieldValue(Sdk.CustomJob.StatusReason) === Sdk.CustomJob.StatusReasonEnum.Draft)
	{
		SetFormLocked(false,
			[Sdk.CustomJob.Status,
				Sdk.CustomJob.StatusReason,
				Sdk.CustomJob.PreviousTargetDate,
				Sdk.CustomJob.LatestRunMessage,
				Sdk.CustomJob.PageNumber,
				Sdk.CustomJob.CurrentRetryRun,
				Sdk.CustomJob.RecurrenceUpdatedTrigger,
				Sdk.CustomJob.RunTrigger]);
	}
	else
	{
		SetFormLocked(true);
	}
}

function HideNonUsedSections()
{
	var isAction = GetFieldValue(Sdk.CustomJob.ActionName);
	var isWf = GetFieldValue(Sdk.CustomJob.Workflow);
	var isUrl = GetFieldValue(Ys.CustomJob.Fields.Url);

	var isTimer = GetFieldValue(Sdk.CustomJob.Timer);

	var isTarget = GetFieldValue(Sdk.CustomJob.TargetLogicalName);
	var isSingle = GetFieldValue(Sdk.CustomJob.TargetID);

	var isRetryRun = GetFieldValue(Sdk.CustomJob.CurrentRetryRun);
	var isRetrySchedule = GetFieldValue(Sdk.CustomJob.RetrySchedule);
	var isSubJobRetrySchedule = GetFieldValue('ldv_subjobsretryscheduleid');
	var isRetryCount = GetFieldValue(Sdk.CustomJob.MaxRetryCount);

	var isFailAction = GetFieldValue(Sdk.CustomJob.FailureAction);

	if (!isAction)
	{
		SetFieldVisible(Sdk.CustomJob.ActionName, false);
	}

	if (!isUrl)
	{
		SetFieldVisible(Ys.CustomJob.Fields.Url, false);
	}

	if (!isAction && !isWf)
	{
		SetFieldVisible(Sdk.CustomJob.SerialisedInputParams, false);
	}

	if (!isWf)
	{
		SetFieldVisible(Sdk.CustomJob.Workflow, false);
	}

	if (!isTimer)
	{
		SetSectionVisible('GeneralTab', 'TimerSection', false);
	}

	if (isTarget)
	{
		if (isSingle)
		{
			SetSectionVisible('TargetTab', 'MultipleRecordsSection', false);
			SetSectionVisible('TargetTab', 'TargetXmlSection', false);
		}
		else
		{
			SetSectionVisible('TargetTab', 'SingleRecordSection', false);
		}
	}
	else
	{
		SetTabVisible('TargetTab', false);
	}

	if (!isRetryRun)
	{
		SetFieldVisible(Sdk.CustomJob.CurrentRetryRun, false);
	}

	if (!isRetrySchedule)
	{
		SetFieldVisible(Sdk.CustomJob.RetrySchedule, false);
	}

	if (!isSubJobRetrySchedule)
	{
		SetFieldVisible('ldv_subjobsretryscheduleid', false);
	}

	if (!isRetryCount)
	{
		SetFieldVisible(Sdk.CustomJob.MaxRetryCount, false);
	}

	if (!isFailAction)
	{
		SetFieldVisible(Sdk.CustomJob.FailureAction, false);
	}
}

function SetupActionNames(logicalName, fieldName)
{
	ShowBusyIndicator('Loading action names ... ', 736582);

	logicalName = logicalName || 'none';

	$.ajax({
		type: "GET",
		contentType: "application/json; charset=utf-8",
		datatype: "json",
		url: Xrm.Page.context.getClientUrl() + "/api/data/v8.1/sdkmessages" +
			"?$select=name" +
			"&$expand=sdkmessageid_sdkmessagefilter($select=primaryobjecttypecode)" +
			"&$filter=template eq false and  customizationlevel eq 1",
		beforeSend: function (xmlHttpRequest)
		{
			xmlHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
			xmlHttpRequest.setRequestHeader("OData-Version", "4.0");
			xmlHttpRequest.setRequestHeader("Accept", "application/json");
		},
		async: true,
		success: function (data, textStatus, xhr)
		{
			var results = data;
			var actions = [];

			var actionsCount = results.value.length;

			if (!actionsCount)
			{
				HideBusyIndicator(736582);
			}

			var buildAutoComplete = function()
			{
				var result = actions.map(function(e)
				{
					return {
							id: e,
							name: e
						};
				});
				
				SetupAutoComplete(fieldName, result, actions, 999, true, true);
			}

			for (var i = 0; i < actionsCount; i++)
			{
				var name = results.value[i]["name"];
				//Use @odata.nextLink to query resulting related records
				var sdkmessageid_sdkmessagefilter_NextLink = results.value[i]["sdkmessageid_sdkmessagefilter@odata.nextLink"];

				(function(nameP, nextLink)
				{
					$.ajax({
							type: "GET",
							contentType: "application/json; charset=utf-8",
							datatype: "json",
							url: nextLink,
							beforeSend: function(xmlHttpRequest)
							{
								xmlHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
								xmlHttpRequest.setRequestHeader("OData-Version", "4.0");
								xmlHttpRequest.setRequestHeader("Accept", "application/json");
							},
							async: true,
							success: function(data, textStatus, xhr)
							{
								actionsCount--;

								var results = data;

								for (var i = 0; i < results.value.length; i++)
								{
									var primaryobjecttypecode = results.value[i]["primaryobjecttypecode"];

									if (primaryobjecttypecode === logicalName)
									{
										actions.push(nameP);
									}
								}

								if (!actionsCount)
								{
									HideBusyIndicator(736582);
									buildAutoComplete();
								}
							},
							error: function(xhr, textStatus, errorThrown)
							{
								actionsCount--;

								if (!actionsCount)
								{
									HideBusyIndicator(736582);
									buildAutoComplete();
								}

								console.error(xhr);
							}
						});
				})(name, sdkmessageid_sdkmessagefilter_NextLink);
			}
		},
		error: function (xhr, textStatus, errorThrown)
		{
			HideBusyIndicator(736582);
			console.error(xhr);
		}
	});

}