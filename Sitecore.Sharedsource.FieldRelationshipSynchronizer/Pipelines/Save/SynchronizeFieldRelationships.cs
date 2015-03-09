using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Pipelines.Save;
using Sitecore.Sharedsource.FieldRelationships;

namespace Sitecore.Sharedsource.Pipelines.Save
{
    public class SynchronizeFieldRelationships
    {
        private readonly IEnumerable<FieldRelationship> _relationships;

        private readonly Database _database = Database.GetDatabase("master");
        
        private readonly char[] _delimiters = { '|' };

        public SynchronizeFieldRelationships(IEnumerable<FieldRelationship> relationshipCollection)
        {
            this._relationships = relationshipCollection;
        }

        public void Process(SaveArgs args)
        {
            foreach (var item in args.Items)
            {
                foreach (var field in item.Fields)
                {
                    if (field.OriginalValue == field.Value) continue;

                    this.SyncLeftToRightRelationships(field, item);
                    this.SyncRightToLeftRelationships(field, item);
                }
            }
        }

        protected void SyncLeftToRightRelationships(SaveArgs.SaveField savedField, SaveArgs.SaveItem savedItem)
        {
            var matchingRelationships = this._relationships.Where(x => x.LeftFieldId == savedField.ID);

            foreach (var relationship in matchingRelationships)
            {
                if (relationship.SyncDirection == SyncDirection.Both ||
                    relationship.SyncDirection == SyncDirection.LeftToRight)
                {
                    this.UpdateOtherField(savedItem, savedField, relationship.RightFieldId);
                }
            }
        }

        protected void SyncRightToLeftRelationships(SaveArgs.SaveField savedField, SaveArgs.SaveItem savedItem)
        {
            var matchingRelationships = this._relationships.Where(x => x.RightFieldId == savedField.ID);

            foreach (var relationship in matchingRelationships)
            {
                if (relationship.SyncDirection == SyncDirection.Both ||
                    relationship.SyncDirection == SyncDirection.RightToLeft)
                {
                    this.UpdateOtherField(savedItem, savedField, relationship.LeftFieldId);
                }
            }
        }

        protected void UpdateOtherField(SaveArgs.SaveItem savedItem, SaveArgs.SaveField savedField, ID otherFieldId)
        {
            var originalIds = savedField.OriginalValue.Split(this._delimiters, StringSplitOptions.RemoveEmptyEntries);
            var newIds = savedField.Value.Split(this._delimiters, StringSplitOptions.RemoveEmptyEntries);

            var addedItemIds = newIds.Except(originalIds).Select(ID.Parse);
            this.AddItemIds(addedItemIds, savedItem.ID, otherFieldId);

            var removedItemIds = originalIds.Except(newIds).Select(ID.Parse);
            this.RemoveItemIds(removedItemIds, savedItem.ID, otherFieldId);
        }

        protected void AddItemIds(IEnumerable<ID> otherItemIds, ID savedItemId, ID otherFieldId)
        {
            foreach (var otherItemId in otherItemIds)
            {
                var otherItem = this._database.GetItem(otherItemId);
                otherItem.Fields.ReadAll();

                var otherField = otherItem.Fields.SingleOrDefault(x => x.ID == otherFieldId);
                if (otherField == null) return;

                var ids = otherField.Value
                    .Split(this._delimiters, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
                ids.Add(savedItemId.ToString());
                var updatedValue = string.Join("|", ids.Distinct());

                otherItem.Editing.BeginEdit();
                otherField.Value = updatedValue;
                otherItem.Editing.EndEdit();
            }
        }

        protected void RemoveItemIds(IEnumerable<ID> otherItemIds, ID savedItemId, ID otherFieldId)
        {
            foreach (var otherItemId in otherItemIds)
            {
                var otherItem = this._database.GetItem(otherItemId);
                otherItem.Fields.ReadAll();

                var otherField = otherItem.Fields.SingleOrDefault(x => x.ID == otherFieldId);
                if (otherField == null) return;

                var updatedIds = otherField.Value
                    .Split(this._delimiters, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => x != savedItemId.ToString());
                var updatedValue = string.Join("|", updatedIds.Distinct());

                otherItem.Editing.BeginEdit();
                otherField.Value = updatedValue;
                otherItem.Editing.EndEdit();
            }
        }
    }
}