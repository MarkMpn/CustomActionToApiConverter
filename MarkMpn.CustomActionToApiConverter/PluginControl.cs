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
                    sdkMessageLink.Columns = new ColumnSet("name");
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
    }
}
