﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using McTools.Xrm.Connection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace MarkMpn.CustomActionToApiConverter
{
    public partial class PluginControl : PluginControlBase, IGitHubPlugin, IHelpPlugin, IPayPalPlugin
    {
        public PluginControl()
        {
            InitializeComponent();
        }

        protected override void OnConnectionUpdated(ConnectionUpdatedEventArgs e)
        {
            base.OnConnectionUpdated(e);

            LoadSolutions();
        }

        private void LoadSolutions()
        {
            solutionComboBox.Items.Clear();

            if (ConnectionDetail == null)
                return;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading Solutions...",
                Work = (bw, args) =>
                {
                    var qry = new QueryExpression("solution");
                    qry.ColumnSet = new ColumnSet("solutionid", "friendlyname");
                    // Ignore Active and Default solutions
                    qry.Criteria.AddCondition("solutionid", ConditionOperator.NotIn, new Guid("FD140AAF-4DF4-11DD-BD17-0019B9312238"), new Guid("FD140AAE-4DF4-11DD-BD17-0019B9312238"));
                    qry.Criteria.AddCondition("ismanaged", ConditionOperator.Equal, false);
                    var componentLink = qry.AddLink("solutioncomponent", "solutionid", "solutionid", JoinOperator.Exists);
                    var workflowLink = componentLink.AddLink("workflow", "objectid", "workflowid");
                    workflowLink.LinkCriteria.AddCondition("category", ConditionOperator.Equal, 3); // Action
                    qry.AddOrder("friendlyname", OrderType.Ascending);

                    var results = Service.RetrieveMultiple(qry);
                    var solutions = results.Entities
                        .Select(entity => new EntityReference
                        {
                            LogicalName = entity.LogicalName,
                            Name = entity.GetAttributeValue<string>("friendlyname"),
                            Id = entity.Id
                        })
                        .ToList();

                    args.Result = solutions;
                },
                PostWorkCallBack = args =>
                {
                    var solutions = (List<EntityReference>)args.Result;
                    solutionComboBox.DisplayMember = nameof(EntityReference.Name);
                    solutionComboBox.ValueMember = nameof(EntityReference.Id);
                    solutionComboBox.DataSource = solutions;
                }
            });
        }

        string IGitHubPlugin.UserName => "MarkMpn";

        string IGitHubPlugin.RepositoryName => "CustomActionToApiConverter";

        string IHelpPlugin.HelpUrl => "https://markcarrington.dev/custom-action-to-api-converter/";

        string IPayPalPlugin.DonationDescription => "Custom Action to Custom API Converter Donation";

        string IPayPalPlugin.EmailAccount => "donate@markcarrington.dev";

        private void solutionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var solutionId = (Guid)solutionComboBox.SelectedValue;
            customActionListView.Items.Clear();

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading Custom Actions...",
                Work = (bw, args) =>
                {
                    var qry = new QueryExpression("workflow");
                    qry.ColumnSet = new ColumnSet("name", "uniquename");
                    qry.Criteria.AddCondition("category", ConditionOperator.Equal, 3);
                    var componentLink = qry.AddLink("solutioncomponent", "workflowid", "objectid");
                    componentLink.LinkCriteria.AddCondition("solutionid", ConditionOperator.Equal, solutionId);
                    var sdkMessageLink = qry.AddLink("sdkmessage", "sdkmessageid", "sdkmessageid");
                    sdkMessageLink.EntityAlias = "msg";
                    sdkMessageLink.Columns = new ColumnSet("name", "sdkmessageid");
                    qry.AddOrder("name", OrderType.Ascending);

                    var results = Service.RetrieveMultiple(qry);
                    args.Result = results;
                },
                PostWorkCallBack = args =>
                {
                    var actions = (EntityCollection)args.Result;

                    foreach (var action in actions.Entities)
                    {
                        var lvi = customActionListView.Items.Add(action.GetAttributeValue<string>("name"));
                        lvi.SubItems.Add((string) action.GetAttributeValue<AliasedValue>("msg.name").Value);
                        lvi.Tag = action;
                    }
                }
            });
        }

        private void customActionListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = null;
            convertButton.Enabled = false;

            if (customActionListView.SelectedItems.Count != 1)
                return;

            var sdkMessageId = (Guid) ((Entity)customActionListView.SelectedItems[0].Tag).GetAttributeValue<AliasedValue>("msg.sdkmessageid").Value;

            WorkAsync(new WorkAsyncInfo
            {
                Message = "Loading Custom Action Details...",
                Work = (bw, args) =>
                {
                    var qry = new QueryExpression("sdkmessage");
                    qry.ColumnSet = new ColumnSet("name");
                    qry.Criteria.AddCondition("sdkmessageid", ConditionOperator.Equal, sdkMessageId);
                    var workflowLink = qry.AddLink("workflow", "sdkmessageid", "sdkmessageid");
                    workflowLink.EntityAlias = "wf";
                    workflowLink.Columns = new ColumnSet("workflowid", "name", "description", "primaryentity", "xaml");
                    workflowLink.LinkCriteria.AddCondition("type", ConditionOperator.Equal, 1); // Definition

                    var workflowDetails = Service.RetrieveMultiple(qry).Entities.Single();

                    var reqParamQry = new QueryExpression("sdkmessagerequestfield");
                    reqParamQry.Distinct = true;
                    reqParamQry.ColumnSet = new ColumnSet("clrparser", "name", "optional", "parameterbindinginformation");
                    var reqLink = reqParamQry.AddLink("sdkmessagerequest", "sdkmessagerequestid", "sdkmessagerequestid");
                    var pairLink = reqLink.AddLink("sdkmessagepair", "sdkmessagepairid", "sdkmessagepairid");
                    pairLink.LinkCriteria.AddCondition("sdkmessageid", ConditionOperator.Equal, sdkMessageId);
                    pairLink.LinkCriteria.AddCondition("endpoint", ConditionOperator.Equal, "api/data");
                    reqParamQry.AddOrder("position", OrderType.Ascending);

                    var requestParameters = Service.RetrieveMultiple(reqParamQry);

                    var respParamQry = new QueryExpression("sdkmessageresponsefield");
                    respParamQry.Distinct = true;
                    respParamQry.ColumnSet = new ColumnSet("clrformatter", "formatter", "name", "parameterbindinginformation");
                    var respLink = respParamQry.AddLink("sdkmessageresponse", "sdkmessageresponseid", "sdkmessageresponseid");
                    reqLink = respLink.AddLink("sdkmessagerequest", "sdkmessagerequestid", "sdkmessagerequestid");
                    pairLink = reqLink.AddLink("sdkmessagepair", "sdkmessagepairid", "sdkmessagepairid");
                    pairLink.LinkCriteria.AddCondition("sdkmessageid", ConditionOperator.Equal, sdkMessageId);
                    pairLink.LinkCriteria.AddCondition("endpoint", ConditionOperator.Equal, "api/data");
                    respParamQry.AddOrder("position", OrderType.Ascending);

                    var responseParameters = Service.RetrieveMultiple(respParamQry);

                    var stepQry = new QueryExpression("sdkmessageprocessingstep");
                    stepQry.ColumnSet = new ColumnSet("mode", "stage", "plugintypeid");
                    stepQry.Criteria.AddCondition("sdkmessageid", ConditionOperator.Equal, sdkMessageId);
                    stepQry.Criteria.AddCondition("stage", ConditionOperator.NotEqual, 30); // Exclude standard SyncWorkflowExecution plugin
                    var pluginLink = stepQry.AddLink("plugintype", "plugintypeid", "plugintypeid");
                    pluginLink.EntityAlias = "plugin";
                    pluginLink.Columns = new ColumnSet("name");
                    var pluginSteps = Service.RetrieveMultiple(stepQry);

                    var xaml = (string)workflowDetails.GetAttributeValue<AliasedValue>("wf.xaml")?.Value;

                    var action = new CustomAction
                    {
                        WorkflowId = (Guid)workflowDetails.GetAttributeValue<AliasedValue>("wf.workflowid").Value,
                        MessageName = workflowDetails.GetAttributeValue<string>("name"),
                        Name = (string)workflowDetails.GetAttributeValue<AliasedValue>("wf.name").Value,
                        Description = (string)workflowDetails.GetAttributeValue<AliasedValue>("wf.description")?.Value,
                        HasWorkflow = HasWorkflow(xaml),
                        PrimaryEntity = (string)workflowDetails.GetAttributeValue<AliasedValue>("wf.primaryentity")?.Value,
                        RequestParameters = new ParameterCollection<RequestParameter>(requestParameters.Entities
                            .Select(param => new RequestParameter
                            {
                                Name = param.GetAttributeValue<string>("name"),
                                Required = !param.GetAttributeValue<bool>("optional"),
                                Type = Type.GetType(param.GetAttributeValue<string>("clrparser")),
                                BindingInformation = param.GetAttributeValue<string>("parameterbindinginformation"),
                                Description = GetDescription(xaml, param.GetAttributeValue<string>("name"), true)
                            })),
                        ResponseParameters = new ParameterCollection<ResponseParameter>(responseParameters.Entities
                            .Select(param => new ResponseParameter
                            {
                                Name = param.GetAttributeValue<string>("name"),
                                Type = Type.GetType(param.GetAttributeValue<string>("clrformatter")),
                                BindingInformation = param.GetAttributeValue<string>("parameterbindinginformation"),
                                Description = GetDescription(xaml, param.GetAttributeValue<string>("name"), false)
                            })),
                        PluginSteps = pluginSteps.Entities
                            .Select(step => new PluginStep
                            {
                                StepId = step.Id,
                                PluginId = step.GetAttributeValue<EntityReference>("plugintypeid").Id,
                                PluginName = (string) step.GetAttributeValue<AliasedValue>("plugin.name").Value,
                                Sync = step.GetAttributeValue<OptionSetValue>("mode").Value == 0,
                                Stage = step.GetAttributeValue<OptionSetValue>("stage").Value
                            })
                            .ToList()
                    };

                    foreach (var boundParam in action.RequestParameters.Where(p => p.BindingInformation != null))
                    {
                        if (boundParam.BindingInformation == "Bound:TRUE")
                        {
                            boundParam.IsBindingTarget = true;
                            boundParam.BindingTargetType = action.PrimaryEntity;
                        }
                        else if (boundParam.BindingInformation.StartsWith("OTC:") && Int32.TryParse(boundParam.BindingInformation.Substring(4), out var otc))
                        {
                            var metaQry = new RetrieveMetadataChangesRequest
                            {
                                Query = new EntityQueryExpression
                                {
                                    Properties = new MetadataPropertiesExpression
                                    {
                                        PropertyNames =
                                        {
                                            nameof(EntityMetadata.LogicalName)
                                        }
                                    },
                                    Criteria = new MetadataFilterExpression
                                    {
                                        Conditions =
                                        {
                                            new MetadataConditionExpression(nameof(EntityMetadata.ObjectTypeCode), MetadataConditionOperator.Equals, otc)
                                        }
                                    }
                                }
                            };
                            var meta = (RetrieveMetadataChangesResponse) Service.Execute(metaQry);

                            if (meta.EntityMetadata.Count == 1)
                                boundParam.BindingTargetType = meta.EntityMetadata[0].LogicalName;
                        }
                    }

                    // Autoselect the plugin if there is only one to choose from
                    var availablePlugins = action.PluginSteps.Where(step => step.Sync && step.Stage == 40).ToList();
                    if (availablePlugins.Count == 1)
                        action.Plugin = new EntityReference("plugintype", availablePlugins[0].PluginId) { Name = availablePlugins[0].PluginName };

                    // Autoselect the minimum amount of allowed custom processing steps
                    action.AllowedCustomProcessingStepType = action.GetAllowedCustomProcessingStepType()[0];

                    args.Result = action;
                },
                PostWorkCallBack = args =>
                {
                    selectedActionLabel.Text = ((CustomAction)args.Result).MessageName;
                    propertyGrid.SelectedObject = args.Result;
                    convertButton.Enabled = true;
                }
            });
        }

        private bool HasWorkflow(string xaml)
        {
            var xml = new XmlDocument();
            xml.LoadXml(xaml);
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("act", "http://schemas.microsoft.com/netfx/2009/xaml/activities");
            var activity = (XmlElement) xml.SelectSingleNode("/act:Activity", nsmgr);
            var mxswaNamespace = activity.Attributes.Cast<XmlAttribute>().Single(attr => attr.Name.StartsWith("xmlns:") && attr.Value.StartsWith("clr-namespace:Microsoft.Xrm.Sdk.Workflow.Activities;"));
            nsmgr.AddNamespace("mxswa", mxswaNamespace.Value);
            var wf = xml.SelectSingleNode("/act:Activity/mxswa:Workflow", nsmgr);

            return wf.HasChildNodes;
        }

        private string GetDescription(string xaml, string parameter, bool input)
        {
            var xml = new XmlDocument();
            xml.LoadXml(xaml);
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("act", "http://schemas.microsoft.com/netfx/2009/xaml/activities");
            nsmgr.AddNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            var activity = (XmlElement)xml.SelectSingleNode("/act:Activity", nsmgr);
            var mxswNamespace = activity.Attributes.Cast<XmlAttribute>().Single(attr => attr.Name.StartsWith("xmlns:") && attr.Value.StartsWith("clr-namespace:Microsoft.Xrm.Sdk.Workflow;"));
            nsmgr.AddNamespace("mxsw", mxswNamespace.Value);
            var prop = (XmlElement)xml.SelectSingleNode($"/act:Activity/x:Members/x:Property[@Name='{parameter}']/x:Property.Attributes[mxsw:ArgumentDirectionAttribute/@Value='{(input ? Microsoft.Xrm.Sdk.Workflow.ArgumentDirection.Input : Microsoft.Xrm.Sdk.Workflow.ArgumentDirection.Output)}']/mxsw:ArgumentDescriptionAttribute", nsmgr);

            if (prop == null)
                return null;

            return prop.GetAttribute("Value");
        }

        private void convertButton_Click(object sender, EventArgs e)
        {
            var action = (CustomAction)propertyGrid.SelectedObject;
            var solutionId = (Guid)solutionComboBox.SelectedValue;

            if (action.HasWorkflow)
            {
                if (MessageBox.Show("This Custom Action has a workflow component that will be lost during conversion to a Custom API. You will need to implement a plugin to replicate the same functionality as the workflow.\r\n\r\nAre you sure you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
            }

            WorkAsync(new WorkAsyncInfo
            {
                Message = $"Converting {action.MessageName} to Custom API...",
                Work = (bw, args) =>
                {
                    var solutionName = Service.Retrieve("solution", solutionId, new ColumnSet("uniquename")).GetAttributeValue<string>("uniquename");
                    
                    const string customapi = nameof(customapi);
                    const string customapirequestparameter = nameof(customapirequestparameter);
                    const string customapiresponseproperty = nameof(customapiresponseproperty);

                    var entityDetails = (RetrieveMetadataChangesResponse)Service.Execute(new RetrieveMetadataChangesRequest
                    {
                        Query = new EntityQueryExpression
                        {
                            Criteria = new MetadataFilterExpression
                            {
                                Conditions =
                                {
                                    new MetadataConditionExpression(nameof(EntityMetadata.LogicalName), MetadataConditionOperator.In, new object[] { customapi, customapirequestparameter, customapiresponseproperty })
                                }
                            },
                            Properties = new MetadataPropertiesExpression
                            {
                                PropertyNames = { nameof(EntityMetadata.LogicalName), nameof(EntityMetadata.ObjectTypeCode) }
                            }
                        }
                    });
                    var objectTypeCodes = entityDetails.EntityMetadata.ToDictionary(entity => entity.LogicalName, entity => entity.ObjectTypeCode.Value);

                    // TODO: Save the sdkmessageprocessingsteps that are associated with the existing message, including which solutions they were part of and any associated images

                    // TODO: Delete the sdkmessageprocessingsteps. Do this in a transaction so that if one can't be deleted for some reason, we haven't lost any ones that have already been deleted
                    Service.Execute(new ExecuteTransactionRequest
                    {
                    });

                    // Unpublish the custom action
                    Service.Update(new Entity("workflow", action.WorkflowId)
                    {
                        ["statecode"] = new OptionSetValue(0),
                        ["statuscode"] = new OptionSetValue(1)
                    });

                    // Delete the custom action
                    Service.Delete("workflow", action.WorkflowId);

                    // Create the custom API
                    var api = new Entity(customapi)
                    {
                        ["allowedcustomprocessingsteptype"] = new OptionSetValue((int)action.AllowedCustomProcessingStepType),
                        ["bindingtype"] = new OptionSetValue(action.PrimaryEntity == "none" ? 0 : 1),
                        ["boundentitylogicalname"] = action.PrimaryEntity == "none" ? null : action.PrimaryEntity,
                        ["description"] = action.Description ?? action.Name,
                        ["displayname"] = action.Name,
                        ["isfunction"] = action.IsFunction,
                        ["isprivate"] = action.IsPrivate,
                        ["name"] = action.Name,
                        ["plugintypeid"] = action.Plugin,
                        ["uniquename"] = action.MessageName
                    };
                    api.Id = Service.Create(api);
                    Service.Execute(new AddSolutionComponentRequest
                    {
                        ComponentId = api.Id,
                        ComponentType = objectTypeCodes[api.LogicalName],
                        SolutionUniqueName = solutionName
                    });

                    // Add the request parameters
                    foreach (var actionParam in action.RequestParameters)
                    {
                        if (actionParam.IsBindingTarget)
                            continue;

                        var apiParam = new Entity(customapirequestparameter)
                        {
                            ["customapiid"] = api.ToEntityReference(),
                            ["description"] = actionParam.Description,
                            ["displayname"] = actionParam.Name,
                            ["isoptional"] = !actionParam.Required,
                            ["logicalentityname"] = actionParam.BindingTargetType,
                            ["name"] = $"{action.MessageName}.{actionParam.Name}",
                            ["type"] = actionParam.TypeOptionSetValue,
                            ["uniquename"] = actionParam.Name
                        };

                        apiParam.Id = Service.Create(apiParam);
                        Service.Execute(new AddSolutionComponentRequest
                        {
                            ComponentId = apiParam.Id,
                            ComponentType = objectTypeCodes[apiParam.LogicalName],
                            SolutionUniqueName = solutionName
                        });
                    }

                    // Add the response parameters
                    foreach (var actionProp in action.ResponseParameters)
                    {
                        var apiProp = new Entity(customapiresponseproperty)
                        {
                            ["customapiid"] = api.ToEntityReference(),
                            ["description"] = actionProp.Description,
                            ["displayname"] = actionProp.Name,
                            ["logicalentityname"] = actionProp.BindingTargetType,
                            ["name"] = $"{action.MessageName}.{actionProp.Name}",
                            ["type"] = actionProp.TypeOptionSetValue,
                            ["uniquename"] = actionProp.Name
                        };

                        apiProp.Id = Service.Create(apiProp);
                        Service.Execute(new AddSolutionComponentRequest
                        {
                            ComponentId = apiProp.Id,
                            ComponentType = objectTypeCodes[apiProp.LogicalName],
                            SolutionUniqueName = solutionName
                        });
                    }

                    // Remove the post-operation step that has been moved to stage 30
                    if (action.Plugin != null)
                    {
                        var step = action.PluginSteps.First(s => s.PluginId == action.Plugin.Id && s.Stage == 40 && s.Sync);
                        Service.Delete("sdkmessageprocessingstep", step.StepId);
                    }

                    // TODO: Recreate the sdkmessageprocessingsteps
                },
                PostWorkCallBack = args =>
                {
                    if (args.Error != null)
                        MessageBox.Show(args.Error.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Refresh the list of available actions
                    solutionComboBox_SelectedIndexChanged(sender, e);
                }
            });
        }
    }
}
