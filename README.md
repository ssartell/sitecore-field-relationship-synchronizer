# Sitecore Field Relationship Synchronizer
Maintains relationships between items by automatically synchronizing treelist or multilist fields on save via a pipeline processor. Droplinks can also be synchronized but may result in an invalid state if multiple items try to relate to a single item. Relationships can be defined as one-way or two-way.

Example Scenario
--------------
Let's say we had templates for students and courses that had a many-to-many relationship. Each student has a treelist of courses and each course has a treelist of students. This project can keep both sides synchronized such that adding or removing a course from a student will automatically update all affected courses by modifying their student treelists.

XML Configuration
--------------
To configure relationships, patch the following xml to the config in the `SaveUI` pipeline (or whatever pipeline suits your needs) before the `WorkflowSaveCommand` processor:

```XML
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/">
  <sitecore>
    <processors>
      <saveUI>
        <processor mode="on" type="Rightpoint.Sitecore.FieldSync.Pipelines.SaveUI.FieldSync, Rightpoint.Sitecore.FieldSync">
          <param type="Rightpoint.Sitecore.FieldSync.FieldRelationships.ConfigRelationshipCollection, Rightpoint.Sitecore.FieldSync">
            <fieldRelationships hint="raw:AddFieldRelationship">
              <!-- leftFieldId: field guid from template -->
              <!-- rightFieldId: field guid from template -->
              <!-- syncDirection: 0=None, 1=LeftToRight, 2=RightToLeft, 3=Both -->

              <!-- example -->
              <fieldRelationship leftFieldId="{93F250AA-2C83-4A1F-9EA0-A06352E987C2}" rightFieldId="{E9FE63B2-07C4-47C1-885E-7554DE9B5D67}" syncDirection="3"/>
            </fieldRelationships>
          </param>
        </processor>
      </saveUI>
    </processors>
  </sitecore>
</configuration>
```

Custom Code Configuration
--------------
If you'd prefer to use a custom configuration instead of xml, implement `IEnumerable<FieldRelationship>` and replace the `ConfigRelationshipCollection` param in the above xml with your own implementation. For example, your implementation could read relationships stored as items in the content tree.