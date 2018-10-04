using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Pipelines.Save;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitecore.Foundation.Validation.Pipelines.Save
{
    /// <summary>Validates that field names are unique.</summary>
    public class CheckTemplateFieldNames
    {
        /// <summary>Runs the processor.</summary>
        /// <param name="args">The arguments.</param>
        public void Process(SaveArgs args)
        {
            if (!args.HasSheerUI)
                return;    
            Assert.ArgumentNotNull((object)args, nameof(args));
            Assert.IsNotNull((object)args.Items, "args.Items");
            foreach (SaveArgs.SaveItem saveItem in args.Items)
            {
                Item obj = Client.ContentDatabase.GetItem(saveItem.ID, saveItem.Language);
                if (obj != null && obj.Database.Engines.TemplateEngine.IsTemplate(obj))
                {
                    Template template = TemplateManager.GetTemplate(obj.ID, Client.ContentDatabase);
                    List<TemplateField> templateFields = template.GetFields(false).ToList();
                    templateFields.AddRange(GetTemplateFields(saveItem, obj));

                    if (templateFields == null || templateFields.Count == 0)
                        return;

                    List<string> fieldNames = new List<string>();
                    List<string> duplicateFieldNames = new List<string>();
                    foreach (TemplateField templateField in templateFields)
                    {
                        if (fieldNames.Contains(templateField.Name.ToLower()))
                        {
                            duplicateFieldNames.Add(templateField.Name);
                        }
                        fieldNames.Add(templateField.Name.ToLower());
                    }
                    if (duplicateFieldNames.Count != 0)
                    {
                        CheckTemplateFieldNames.ShowDialog(obj, duplicateFieldNames);
                        return;
                    }
                }
            }
        }

        /// <summary>Get the current base template fields.</summary>
        /// <param name="saveItem">The template item being saved.</param>
        /// <param name="Item">The tempate item.</param>
        private static List<TemplateField> GetTemplateFields(SaveArgs.SaveItem saveItem, Item obj)
        {
            List<TemplateField> baseTemplateFields = new List<TemplateField>();
            SaveArgs.SaveField saveField = ((IEnumerable<SaveArgs.SaveField>)saveItem.Fields).FirstOrDefault<SaveArgs.SaveField>((Func<SaveArgs.SaveField, bool>)(field => field.ID == FieldIDs.BaseTemplate));
            string [] currentBaseTemplateIDs = saveField.Value.Split('|');

            List<ID> templateFieldsResolved = new List<ID>();
            foreach (string baseId in currentBaseTemplateIDs)
            {
                ID baseTemplateId = new ID(baseId);
                if (!templateFieldsResolved.Contains(baseTemplateId))
                {
                    Template template = TemplateManager.GetTemplate(baseTemplateId, Client.ContentDatabase);
                    if (template != null)
                        baseTemplateFields.AddRange(template.GetFields(false).ToList());
                    templateFieldsResolved.Add(baseTemplateId);
                }
            }
            return baseTemplateFields;
        }
        /// <summary>Shows the dialog.</summary>
        /// <param name="item">The item.</param>
        /// <param name="fieldNames">The fieldNames.</param>
        private static void ShowDialog(Item item, List<string> fieldNames)
        {
            StringBuilder stringBuilder = new StringBuilder(Translate.Text("This template contains duplicate field names:\n\n", (object)item.DisplayName));
            if (fieldNames.Count > 0)
            {
                stringBuilder.Append("<table style='word-break:break-all;'>");
                stringBuilder.Append("<tbody>");
                foreach (string fieldName in fieldNames)
                {
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        stringBuilder.Append("<tr>");
                        stringBuilder.Append("<td style='width:70px;vertical-align:top;padding-bottom:5px;padding-right:5px;'>");
                        stringBuilder.Append(fieldName);
                        stringBuilder.Append("</td>");
                        stringBuilder.Append("<td style='vertical-align:top;padding-bottom:5px;'>");
                        stringBuilder.Append("</td>");
                        stringBuilder.Append("</tr>");
                    }
                }
                stringBuilder.Append("</tbody></table>");
            }
            stringBuilder.Append("<br />");
            Context.ClientPage.ClientResponse.Alert(stringBuilder.ToString());
        }
    }
}