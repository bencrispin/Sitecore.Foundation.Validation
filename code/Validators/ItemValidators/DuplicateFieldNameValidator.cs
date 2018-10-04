using Sitecore.Collections;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Data.Validators;
using Sitecore.Globalization;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sitecore.Foundation.Validation.Validators.ItemValidators
{
    /// <summary>
    /// Implements an item validator that checks that no duplicate names appears on the same level.
    /// </summary>
    [Serializable]
    public class DuplicateFieldNameValidator : StandardValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Data.Validators.ItemValidators.DuplicateNameValidator" /> class.
        /// </summary>
        public DuplicateFieldNameValidator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitecore.Data.Validators.ItemValidators.DuplicateNameValidator" /> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The context.</param>
        public DuplicateFieldNameValidator(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }

        /// <summary>Gets the name.</summary>
        /// <value>The serialization name.</value>
        public override string Name
        {
            get
            {
                return "Item Contains Multiple Fields With The Same Name";
            }
        }

        /// <summary>
        /// Check a template items fields
        /// </summary>
        /// <returns>The result of the evaluation.</returns>
        protected ValidatorResult CheckDuplicateFieldNames(TemplateField[] templateFields)
        {
            if (templateFields == null || templateFields.Length == 0)
                return ValidatorResult.Valid;
            List<string> fieldNames = new List<string>();
            foreach (TemplateField templateField in templateFields)
            {
                if (fieldNames.Contains(templateField.Name.ToLower()))
                {
                    string alertText = Translate.Text("The field name \"{0}\" is defined on this item multiple times.", templateField.Name);
                    this.Text = this.GetText(alertText, Array.Empty<string>());
                    return this.GetFailedResult(ValidatorResult.Error);
                }
                fieldNames.Add(templateField.Name.ToLower());
            }
            return ValidatorResult.Valid;
        }

        /// <summary>
        /// Checks an item for duplicate field names
        /// </summary>
        /// <returns>The result of the evaluation.</returns>
        protected ValidatorResult CheckDuplicateFieldNames(FieldCollection fields)
        {
            if (fields == null)
                return ValidatorResult.Valid;
            List<string> fieldNames = new List<string>();
            foreach (Field field in fields)
            {
                if (fieldNames.Contains(field.Name.ToLower()))
                {
                    string alertText = Translate.Text("The field name \"{0}\" is defined on this item multiple times.", field.Name);
                    this.Text = this.GetText(alertText, Array.Empty<string>());                    
                    return this.GetFailedResult(ValidatorResult.Error);
                }
                fieldNames.Add(field.Name.ToLower());
            }
            return ValidatorResult.Valid;
        }

        /// <summary>
        /// When overridden in a derived class, this method contains the code to determine whether the value in the input control is valid.
        /// </summary>
        /// <returns>The result of the evaluation.</returns>
        protected override ValidatorResult Evaluate()
        {
            Item obj = this.GetItem();
            if (obj == null)
                return ValidatorResult.Valid;
            else if (obj.TemplateID == TemplateIDs.Template)
            {
                Template template = TemplateManager.GetTemplate(obj.ID, Sitecore.Data.Database.GetDatabase("master"));
                TemplateField[] allFields = template.GetFields(true);
                return CheckDuplicateFieldNames(allFields);
            }

            obj.Fields.ReadAll();
            return CheckDuplicateFieldNames(obj.Fields);
        }

        /// <summary>Gets the max validator result.</summary>
        /// <remarks>
        /// This is used when saving and the validator uses a thread. If the Max Validator Result
        /// is Error or below, the validator does not have to be evaluated before saving.
        /// If the Max Validator Result is CriticalError or FatalError, the validator must have
        /// been evaluated before saving.
        /// </remarks>
        /// <returns>The max validator result.</returns>
        protected override ValidatorResult GetMaxValidatorResult()
        {
            return this.GetFailedResult(ValidatorResult.Error);
        }
    }
}