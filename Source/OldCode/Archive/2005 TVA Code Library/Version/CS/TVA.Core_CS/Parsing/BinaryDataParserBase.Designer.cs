using System;
using System.Collections.Generic;
using TVA.Collections;

namespace TVA
{
    namespace Parsing
    {
        public partial class BinaryDataParserBase<TIdentifier, TOutput> : System.ComponentModel.Component
        {
            [System.Diagnostics.DebuggerNonUserCode()]
            public BinaryDataParserBase(System.ComponentModel.IContainer container)
                : this()
            {
                //Required for Windows.Forms Class Composition Designer support
                if (container != null) container.Add(this);
            }

            [System.Diagnostics.DebuggerNonUserCode()]
            public BinaryDataParserBase()
            {
                //This call is required by the Component Designer.
                InitializeComponent();

                m_idPropertyName = "ClassID";
                m_optimizeParsing = true;
                m_settingsCategoryName = this.GetType().Name;
                m_outputTypes = new Dictionary<TIdentifier, TypeInfo>();
                m_unparsedDataReuseCount = new Dictionary<Guid, int>();
                m_dataQueue = ProcessQueue<IdentifiableItem<Guid, byte[]>>.CreateRealTimeQueue(ParseData);
            }

            //Component overrides dispose to clean up the component list.
            [System.Diagnostics.DebuggerNonUserCode()]
            protected override void Dispose(bool disposing)
            {
                try
                {
                    Stop(); // Stop the binary data parser.
                    SaveSettings(); // Saves settings to the config file.
                    if (disposing && (components != null))
                    {
                        components.Dispose();
                    }
                }
                finally
                {
                    base.Dispose(disposing);
                }
            }

            //Required by the Component Designer
            private System.ComponentModel.Container components = null;

            //NOTE: The following procedure is required by the Component Designer
            //It can be modified using the Component Designer.
            //Do not modify it using the code editor.
            [System.Diagnostics.DebuggerStepThrough()]
            private void InitializeComponent()
            {
                components = new System.ComponentModel.Container();
            }
        }
    }
}
