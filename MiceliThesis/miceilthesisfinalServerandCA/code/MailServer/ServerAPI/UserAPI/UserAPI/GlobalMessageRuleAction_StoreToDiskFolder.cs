using System;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.MailServer.API.UserAPI
{
    /// <summary>
    /// The GlobalMessageRuleAction_StoreToDiskFolder object represents global message rule "Store To Disk Folder" action in LumiSoft Mail Server virtual server.
    /// </summary>
    public class GlobalMessageRuleAction_StoreToDiskFolder : GlobalMessageRuleActionBase
    {
        private string m_Folder = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="rule">Onwer rule that ows this action.</param>
        /// <param name="owner">Owner GlobalMessageRuleActionCollection that owns this action.</param>
        /// <param name="id">Action ID.</param>
        /// <param name="description">Action description.</param>
        /// <param name="actionData">Action data.</param>
        internal GlobalMessageRuleAction_StoreToDiskFolder(GlobalMessageRule rule,GlobalMessageRuleActionCollection owner,string id,string description,byte[] actionData) : base(rule,owner,id,description)
        {
            /*  Action data structure:
                    <ActionData>
                        <Folder></Folder>
                    </ActionData>
            */
                        
            XmlTable table = new XmlTable("ActionData");
            table.Parse(actionData);
            m_Folder = table.GetValue("Folder");            
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="rule">Onwer rule that ows this action.</param>
        /// <param name="owner">Owner GlobalMessageRuleActionCollection that owns this action.</param>
        /// <param name="id">Action ID.</param>
        /// <param name="description">Action description.</param>
        /// <param name="folder">Disk folder where to store message.</param>
        internal GlobalMessageRuleAction_StoreToDiskFolder(GlobalMessageRule rule,GlobalMessageRuleActionCollection owner,string id,string description,string folder) : base(rule,owner,id,description)
        {
            m_Folder = folder;
        }


        #region method Serialize

        /// <summary>
        /// Serialices action object.
        /// </summary>
        /// <returns>Returns serialized action data.</returns>
        internal override byte[] Serialize()
        {
            /*  Action data structure:
                    <ActionData>
                        <Folder></Folder>
                    </ActionData>
            */

            XmlTable table = new XmlTable("ActionData");
            table.Add("Folder",m_Folder);

            return table.ToByteData();
        }

        #endregion


        #region Properties Impelementation

        /// <summary>
        /// Get global message rule action type.
        /// </summary>
        public override GlobalMessageRuleAction_enum ActionType
        {
           get{ return GlobalMessageRuleAction_enum.StoreToDiskFolder; }
        }

        /// <summary>
        /// Gets if this object has changes what isn't stored to mail server by calling Commit().
        /// </summary>
        public bool HasChanges
        {
            get{ return m_ValuesChanged; }
        }

        /// <summary>
        /// Gets or sets disk folder where to store message.
        /// </summary>
        public string Folder
        {
            get{ return m_Folder; }

            set{
                if(m_Folder != value){
                    m_Folder = value;

                    m_ValuesChanged = true;
                }
            }
        }

        #endregion

    }
}
