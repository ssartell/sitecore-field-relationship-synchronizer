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
                    this.SyncOtherField(savedItem, savedField, relationship.RightFieldId);
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
                    this.SyncOtherField(savedItem, savedField, relationship.LeftFieldId);
                }
            }
        }

        protected void SyncOtherField(SaveArgs.SaveItem savedItem, SaveArgs.SaveField savedField, ID otherFieldId)
        {
            var originalIds = savedField.OriginalValue.Split(this._delimiters, StringSplitOptions.RemoveEmptyEntries);
            var newIds = savedField.Value.Split(this._delimiters, StringSplitOptions.RemoveEmptyEntries);

            var removedItemIds = originalIds.Except(newIds).Select(ID.Parse);
            this.ModifyItems(removedItemIds, otherFieldId, ids => ids.Where(x => x != savedItem.ID.ToString()));

            var addedItemIds = newIds.Except(originalIds).Select(ID.Parse);
            this.ModifyItems(addedItemIds, otherFieldId, ids => ids.Concat(new[] { savedItem.ID.ToString() }));            
        }

        protected void ModifyItems(IEnumerable<ID> itemIdsToModify, ID fieldIdToModify, Func<IEnumerable<string>, IEnumerable<string>> modifyValue)
        {
            foreach (var itemId in itemIdsToModify)
            {
                var otherItem = this._database.GetItem(itemId);
                otherItem.Fields.ReadAll();

                var otherField = otherItem.Fields.SingleOrDefault(x => x.ID == fieldIdToModify);
                if (otherField == null) return;

                var ids = otherField.Value
                    .Split(this._delimiters, StringSplitOptions.RemoveEmptyEntries)
                    .AsEnumerable();
                ids = modifyValue(ids);
                var updatedValue = string.Join("|", ids.Distinct());

                otherItem.Editing.BeginEdit();
                otherField.Value = updatedValue;
                otherItem.Editing.EndEdit();
            }
        }
    }
}