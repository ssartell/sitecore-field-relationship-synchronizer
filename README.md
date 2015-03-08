# Sitecore Field Relationship Synchronizer
Maintains relationships between items by automatically synchronizing treelist or multilist fields on save.

Example Scenario
--------------
Let's say we had templates for students and courses. Each student has a treelist of courses and each course has a treelist of students. This project can keep both sides synchronized such that adding or removing a course from a student will automatically update all affected courses by modifying their student treelists.

XML Configuration
--------------
To configure relationships, patch the following xml to the config in the `SaveUI` pipeline (or whatever pipeline suits your needs) before the `WorkflowSaveCommand` processor:

```XML
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

Custom Code Configuration
--------------
If you'd prefer to implement your own configuration, implement `Sitecore.Sharedsource.FieldRelationships.IRelationshipCollection` and replace the `ConfigRelationshipCollection` param in the above xml with your own implementation. For example, your implementation could read relationships stored as items in the content tree.