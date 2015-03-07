# Sitecore Field Relationship Synchronizer
Maintains relationships between fields on different items.

```
<processor mode="on" type="Sitecore.Sharedsource.Pipelines.Save.SynchronizeFieldRelationships, FieldRelationshipSynchronizer">
    <param type="Sitecore.Sharedsource.FieldRelationships.ConfigRelationshipCollection, FieldRelationshipSynchronizer">
        <fieldRelationships hint="raw:AddFieldRelationship">
            <fieldRelationship leftFieldId="{5B310D21-A2DB-4278-BA46-36AFA17D47CF}" rightFieldId="{B41C5FDC-010D-4D0E-9694-F888CBAEA5E5}" syncDirection="3"/>
        </fieldRelationships>
    </param>
</processor>
```