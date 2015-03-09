using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Sitecore.Xml;

namespace Sitecore.Sharedsource.FieldRelationships
{
    public class ConfigRelationshipCollection : IEnumerable<FieldRelationship>
    {
        public ConfigRelationshipCollection()
        {
            this.FieldRelationships = new List<FieldRelationship>();
        }

        public List<FieldRelationship> FieldRelationships { get; private set; }

        public IEnumerator<FieldRelationship> GetEnumerator()
        {
            return this.FieldRelationships.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.FieldRelationships.GetEnumerator();
        }

        public void AddFieldRelationship(string key, XmlNode node)
        {
            this.AddFieldRelationship(node);
        }

        public void AddFieldRelationship(XmlNode node)
        {
            var leftFieldId = XmlUtil.GetAttribute("leftFieldId", node);
            var rightFieldId = XmlUtil.GetAttribute("rightFieldId", node);
            var syncDirection = XmlUtil.GetAttribute("syncDirection", node);

            this.FieldRelationships.Add(new FieldRelationship(leftFieldId, rightFieldId, syncDirection));
        }
    }
}