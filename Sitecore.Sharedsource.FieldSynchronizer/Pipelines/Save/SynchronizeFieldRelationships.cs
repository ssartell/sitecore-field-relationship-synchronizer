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

        public SynchronizeFieldRelationships(IRelationshipCollection relationshipCollection)
        {
            this._relationships = relationshipCollection;
        }

        private readonly Database _database = Database.GetDatabase("master");
        private readonly char[] _delimiters = { '|' };

        public void Process(SaveArgs args)
        {
            foreach (var item in args.Items)
            {
                foreach (var field in item.Fields)
                {
                    if (field.OriginalValue == field.Value) return;

                    this.SyncLeftToRight(field, item);
                    this.SyncRightToLeft(field, item);
                }
            }
        }

        protected void SyncLeftToRight(SaveArgs.SaveField leftField, SaveArgs.SaveItem item)
        {
            var relevantRelationships = this._relationships.Where(x => x.LeftFieldId == leftField.ID);

            foreach (var relationship in relevantRelationships)
            {
                if (relationship.SyncDirection == SyncDirection.Both ||
                    relationship.SyncDirection == SyncDirection.LeftToRight)
                {
                    this.UpdateUnmodifiedField(item, leftField, relationship.RightFieldId);
                }
            }
        }

        protected void SyncRightToLeft(SaveArgs.SaveField rightField, SaveArgs.SaveItem item)
        {
            var relevantRelationships = this._relationships.Where(x => x.RightFieldId == rightField.ID);

            foreach (var relationship in relevantRelationships)
            {
                if (relationship.SyncDirection == SyncDirection.Both ||
                    relationship.SyncDirection == SyncDirection.RightToLeft)
                {
                    this.UpdateUnmodifiedField(item, rightField, relationship.LeftFieldId);
                }
            }
        }

        protected void UpdateUnmodifiedField(SaveArgs.SaveItem modifiedItem, SaveArgs.SaveField modifiedField, ID unmodifiedFieldId)
        {
            var unmodifiedField = this._database.GetItem(unmodifiedFieldId);

            if (unmodifiedField == null) return;

            var originalIds = modifiedField.OriginalValue.Split(this._delimiters, StringSplitOptions.RemoveEmptyEntries);
            var newIds = modifiedField.Value.Split(this._delimiters, StringSplitOptions.RemoveEmptyEntries);

            var addedIds = newIds.Except(originalIds).Select(ID.Parse);
            var removedIds = originalIds.Except(newIds).Select(ID.Parse);

            this.Add(addedIds, modifiedItem.ID, unmodifiedFieldId);
            this.Remove(removedIds, modifiedItem.ID, unmodifiedFieldId);
        }

        protected void Add(IEnumerable<ID> itemIds, ID itemIdToAdd, ID unmodifiedFieldId)
        {
            foreach (var id in itemIds)
            {
                var unmodifiedItem = this._database.GetItem(id);
                var field = unmodifiedItem.Fields.SingleOrDefault(x => x.ID == unmodifiedFieldId);

                if (field == null) return;

                var ids = field.Value.Split(this._delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();
                ids.Add(itemIdToAdd.ToString());
                var updatedValue = string.Join("|", ids);

                unmodifiedItem.Editing.BeginEdit();
                field.Value = updatedValue;
                unmodifiedItem.Editing.EndEdit();
            }
        }

        protected void Remove(IEnumerable<ID> itemIds, ID itemIdToRemove, ID unmodifiedFieldId)
        {
            foreach (var id in itemIds)
            {
                var unmodifiedItem = this._database.GetItem(id);
                var field = unmodifiedItem.Fields.SingleOrDefault(x => x.ID == unmodifiedFieldId);

                if (field == null) return;

                var updatedIds = field.Value.Split(this._delimiters, StringSplitOptions.RemoveEmptyEntries).Where(x => x != itemIdToRemove.ToString());
                var updatedValue = string.Join("|", updatedIds);

                unmodifiedItem.Editing.BeginEdit();
                field.Value = updatedValue;
                unmodifiedItem.Editing.EndEdit();
            }
        }
    }
}