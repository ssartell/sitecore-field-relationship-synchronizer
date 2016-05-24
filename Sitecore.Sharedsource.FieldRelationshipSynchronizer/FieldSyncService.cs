using System;
using System.Collections.Generic;
using System.Linq;
using Rightpoint.Sitecore.FieldSync.Config;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Events;
using Sitecore.Pipelines.Save;

namespace Rightpoint.Sitecore.FieldSync
{
    public class FieldSyncService
    {
        protected readonly IEnumerable<FieldRelationship> Relationships;

        protected readonly Database Database = Database.GetDatabase("master");

        protected readonly char[] Delimiters = { '|' };

        public FieldSyncService(IEnumerable<FieldRelationship> relationships)
        {
            this.Relationships = relationships;
        }

        public void SaveUiPipelineSync(SaveArgs args)
        {
            foreach (var item in args.Items)
            {
                foreach (var field in item.Fields)
                {
                    this.Sync(item.ID, field.ID, field.OriginalValue, field.Value);
                }
            }
        }

        public void SavingEventSync(SitecoreEventArgs args)
        {
            var updatedItem = args.Parameters[0] as Item;
            if (updatedItem == null) return;

            var existingItem = updatedItem.Database.GetItem(updatedItem.ID, updatedItem.Language, updatedItem.Version);
            if (existingItem == null) return;

            foreach (Field field in updatedItem.Fields)
            {
                this.Sync(updatedItem.ID, field.ID, existingItem.Fields[field.ID]?.Value, updatedItem.Fields[field.ID]?.Value);
            }
        }

        public void Sync(ID savedItemId, ID savedFieldId, string originalValue, string newValue)
        {
            if (string.Equals(originalValue, newValue)) return;

            this.SyncLeftToRightRelationships(savedItemId, savedFieldId, originalValue, newValue);
            this.SyncRightToLeftRelationships(savedItemId, savedFieldId, originalValue, newValue);
        }

        protected void SyncLeftToRightRelationships(ID savedItemId, ID savedFieldId, string originalValue, string newValue)
        {
            var matchingRelationships = this.Relationships.Where(x => x.LeftFieldId == savedFieldId);

            foreach (var relationship in matchingRelationships)
            {
                if (relationship.SyncDirection == SyncDirection.Both ||
                    relationship.SyncDirection == SyncDirection.LeftToRight)
                {
                    this.SyncOtherField(savedItemId, relationship.RightFieldId, originalValue, newValue);
                }
            }
        }

        protected void SyncRightToLeftRelationships(ID savedItemId, ID savedFieldId, string originalValue, string newValue)
        {
            var matchingRelationships = this.Relationships.Where(x => x.RightFieldId == savedFieldId);

            foreach (var relationship in matchingRelationships)
            {
                if (relationship.SyncDirection == SyncDirection.Both ||
                    relationship.SyncDirection == SyncDirection.RightToLeft)
                {
                    this.SyncOtherField(savedItemId, relationship.LeftFieldId, originalValue, newValue);
                }
            }
        }

        protected void SyncOtherField(ID savedItemId, ID otherFieldId, string originalValue, string newValue)
        {
            var originalIds = originalValue.Split(this.Delimiters, StringSplitOptions.RemoveEmptyEntries);
            var newIds = newValue.Split(this.Delimiters, StringSplitOptions.RemoveEmptyEntries);

            var removedItemIds = originalIds.Except(newIds).Select(ID.Parse);
            this.ModifyItems(removedItemIds, otherFieldId, ids => ids.Where(x => x != savedItemId.ToString()));

            var addedItemIds = newIds.Except(originalIds).Select(ID.Parse);
            this.ModifyItems(addedItemIds, otherFieldId, ids => ids.Concat(new[] { savedItemId.ToString() }));
        }

        protected void ModifyItems(IEnumerable<ID> itemIdsToModify, ID fieldIdToModify, Func<IEnumerable<string>, IEnumerable<string>> modifyValue)
        {
            foreach (var itemId in itemIdsToModify)
            {
                var otherItem = this.Database.GetItem(itemId);
                otherItem.Fields.ReadAll();

                var otherField = otherItem.Fields.SingleOrDefault(x => x.ID == fieldIdToModify);
                if (otherField == null) return;

                var ids = otherField.Value
                    .Split(this.Delimiters, StringSplitOptions.RemoveEmptyEntries)
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
