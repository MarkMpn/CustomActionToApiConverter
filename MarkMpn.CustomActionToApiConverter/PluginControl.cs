using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using McTools.Xrm.Connection;
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
                        lvi.Checked = true;
                    }
                }
            });
        }

        private void customActionListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = null;

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
                    workflowLink.Columns = new ColumnSet("name", "description", "primaryentity");
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

                    var action = new CustomAction
                    {
                        MessageName = workflowDetails.GetAttributeValue<string>("name"),
                        Name = (string)workflowDetails.GetAttributeValue<AliasedValue>("wf.name").Value,
                        Description = (string)workflowDetails.GetAttributeValue<AliasedValue>("wf.description")?.Value,
                        PrimaryEntity = (string)workflowDetails.GetAttributeValue<AliasedValue>("wf.primaryentity")?.Value,
                        RequestParameters = new ParameterCollection<RequestParameter>(requestParameters.Entities
                            .Select(param => new RequestParameter
                            {
                                Name = param.GetAttributeValue<string>("name"),
                                Required = !param.GetAttributeValue<bool>("optional"),
                                Type = Type.GetType(param.GetAttributeValue<string>("clrparser")),
                                BindingInformation = param.GetAttributeValue<string>("parameterbindinginformation")
                            })),
                        ResponseParameters = new ParameterCollection<ResponseParameter>(responseParameters.Entities
                            .Select(param => new ResponseParameter
                            {
                                Name = param.GetAttributeValue<string>("name"),
                                Type = Type.GetType(param.GetAttributeValue<string>("clrformatter")), // TODO: Handle special cases using "formatter" field instead
                                BindingInformation = param.GetAttributeValue<string>("parameterbindinginformation")
                            }))
                    };

                    foreach (var boundParam in action.RequestParameters.Where(p => p.BindingInformation != null))
                    {
                        if (boundParam.BindingInformation == "Bound:TRUE")
                        {
                            boundParam.IsBindingTarget = true;
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

                    args.Result = action;
                },
                PostWorkCallBack = args =>
                {
                    propertyGrid.SelectedObject = args.Result;
                }
            });
        }
    }
}
