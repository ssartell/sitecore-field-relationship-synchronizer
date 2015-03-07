# Sitecore Field Relationship Synchronizer
Maintains relationships between treelist fields on different items.

Add the following xml to the config in the SaveUI pipeline before the WorkflowSaveCommand processor:

```
<processor mode="on" type="Sitecore.Sharedsource.Pipelines.Save.SynchronizeFieldRelationships, FieldRelationshipSynchronizer">
    <param type="Sitecore.Sharedsource.FieldRelationships.ConfigRelationshipCollection, FieldRelationshipSynchronizer">
        <fieldRelationships hint="raw:AddFieldRelationship">
            <!-- leftFieldId: field guid -->
            <!-- rightFieldId: field guid -->
            <!-- syncDirection: 0=None, 1=LeftToRight, 2=RightToLeft, 3=Both -->
            <fieldRelationship leftFieldId="{5B310D21-A2DB-4278-BA46-36AFA17D47CF}" rightFieldId="{B41C5FDC-010D-4D0E-9694-F888CBAEA5E5}" syncDirection="3"/>
            <!-- add more fieldRelationships here-->
        </fieldRelationships>
    </param>
</processor>
```