using System;
using Sitecore.Data;

namespace Rightpoint.Sitecore.FieldSync.Config
{
    public class FieldRelationship
    {
        public FieldRelationship(ID leftFieldId, ID rightFieldId, SyncDirection syncDirection)
        {
            this.LeftFieldId = leftFieldId;
            this.RightFieldId = rightFieldId;
            this.SyncDirection = syncDirection;
        }

        public FieldRelationship(string leftFieldId, string rightFieldId, SyncDirection syncDirection)
            : this(ID.Parse(leftFieldId), ID.Parse(rightFieldId), syncDirection)
        {
        }

        public FieldRelationship(string leftFieldId, string rightFieldId, string syncDirection)
            : this(leftFieldId, rightFieldId, (SyncDirection)Enum.Parse(typeof(SyncDirection), syncDirection))
        {
        }

        public ID LeftFieldId { get; protected set; }

        public ID RightFieldId { get; protected set; }

        public SyncDirection SyncDirection { get; set; }
    }
}