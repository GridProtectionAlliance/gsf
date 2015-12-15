//******************************************************************************************************
//  MainWindow.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/14/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using GSF.PQDIF.Logical;
using GSF.PQDIF.Physical;

namespace PQDIFExplorer
{
    /// <summary>
    /// The main window of the PQDIF Explorer application.
    /// </summary>
    public partial class MainWindow : Form
    {
        private Dictionary<Guid, Tag> m_tagLookup;
        private List<DetailsWindow> m_detailsWindows;

        /// <summary>
        /// Creates a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        // Opens the given file for exploration.
        private void OpenFile(string fileName)
        {
            Record record;
            ContainerRecord containerRecord;

            // Clear out existing items in the tree view
            ObservationTree.Nodes.Clear();

            // Use the physical parser to more closely display the structure of the file
            using (PhysicalParser parser = new PhysicalParser(fileName))
            {
                parser.Open();

                while (parser.HasNextRecord())
                {
                    // Parse the next record
                    record = parser.NextRecord();

                    // The compression algorithm and compression style are necessary for properly parsing the file
                    // and must be obtained by parsing the logical structure of the container record
                    if (record.Header.TypeOfRecord == RecordType.Container)
                    {
                        containerRecord = ContainerRecord.CreateContainerRecord(record);
                        parser.CompressionAlgorithm = containerRecord.CompressionAlgorithm;
                        parser.CompressionStyle = containerRecord.CompressionStyle;
                    }

                    // Convert this record and all its child elements to a node for
                    // the tree view and add the node to the root level of the tree
                    ObservationTree.Nodes.Add(ToTreeNode(record));
                }
            }
        }

        private TreeNode ToTreeNode(Record record)
        {
            // Use the type of the record to identify it in the tree view
            TreeNode node = new TreeNode(record.Header.TypeOfRecord.ToString());

            // Set the tag of the node so that we can quickly associate
            // a selected node with the record that spawned it
            node.Tag = record;

            if ((object)record.Body != null)
            {
                // Convert each of the elements in the record body's
                // collection into child nodes of the record's tree node
                foreach (Element element in record.Body.Collection.Elements)
                    node.Nodes.Add(ToTreeNode(element));
            }

            // Return the node that represents the record
            return node;
        }

        private TreeNode ToTreeNode(CollectionElement collection)
        {
            TreeNode node;
            Tag tag;

            // Look up the tag of the element to determine its name and
            // use that name, or the tag itself if no name is available,
            // to identify the node in the tree view
            if (m_tagLookup.TryGetValue(collection.TagOfElement, out tag))
                node = new TreeNode(tag.Name);
            else
                node = new TreeNode(collection.TagOfElement.ToString());

            // Set the tag of the node so that we can quickly associate
            // a selected node with the element that spawned it
            node.Tag = collection;

            // Convert each of the elements in the collection
            // into child nodes of this element's tree node
            foreach (Element element in collection.Elements)
                node.Nodes.Add(ToTreeNode(element));

            // Return the node that represents the collection
            return node;
        }
        
        private TreeNode ToTreeNode(Element element)
        {
            CollectionElement collection;
            Tag tag;

            // If the element is a collection element,
            // treat it as a recursive case
            collection = element as CollectionElement;

            if ((object)collection != null)
                return ToTreeNode(collection);

            // Base case: look up the tag and create a leaf node identified
            // by the tag name or the tag itself if no name is available
            if (m_tagLookup.TryGetValue(element.TagOfElement, out tag))
                return new TreeNode(tag.Name) { Tag = element };
            else
                return new TreeNode(element.TagOfElement.ToString()) { Tag = element };
        }

        // Gets detailed information about the given record.
        private string GetDetails(Record record)
        {
            StringBuilder details = new StringBuilder();

            // Display the fields in the record's header
            details.AppendLine($"  Signature: {record.Header.RecordSignature}");
            details.AppendLine($"       Type: {record.Header.TypeOfRecord} ({record.Header.RecordTypeTag})");
            details.AppendLine($"Header Size: {record.Header.HeaderSize}");
            details.AppendLine($"  Body Size: {record.Header.BodySize}");
            details.AppendLine($"   Checksum: {record.Header.Checksum}");

            return details.ToString();
        }

        // Gets detailed information about the given element.
        private string GetDetails(Element element)
        {
            StringBuilder details = new StringBuilder();
            Tag tag;

            // Display the value if the element is a vector or a scalar
            if (element is ScalarElement)
                details.AppendLine($"        Value: {ValueAsString((ScalarElement)element)}").AppendLine();
            else if (element is VectorElement)
                details.AppendLine($"        Value: {ValueAsString((VectorElement)element)}").AppendLine();

            // Display the tag of the element and the actual type of the
            // element and its value as defined by the data in the file
            details.AppendLine($"          Tag: {element.TagOfElement}");
            details.AppendLine($" Element type: {element.TypeOfElement}");
            details.AppendLine($"Physical type: {element.TypeOfValue}");

            // Look up the element's tag to display detailed information about the element as defined
            // by its tag as well as the expected type of the element and its value based on the tag
            if (m_tagLookup.TryGetValue(element.TagOfElement, out tag))
            {
                details.AppendLine();
                details.AppendLine($"-- Tag details --");
                details.AppendLine($"           Name: {tag.Name}");
                details.AppendLine($"  Standard Name: {tag.StandardName}");
                details.AppendLine($"    Description: {tag.Description}");
                details.AppendLine($"   Element type: {tag.ElementType}");
                details.AppendLine($"  Physical type: {tag.PhysicalType}");
                details.AppendLine($"       Required: {(tag.Required ? "Yes" : "No")}");
            }

            return details.ToString();
        }

        // Converts the value of the given element to a string.
        private string ValueAsString(ScalarElement element)
        {
            // The values of many elements can be displayed in a more readable
            // format based on its definition in PQDIF's logical structure
            if (element.TagOfElement == ContainerRecord.CompressionAlgorithmTag && element.TypeOfValue == PhysicalType.UnsignedInteger4)
                return $"{(CompressionAlgorithm)element.GetUInt4()} ({element.GetUInt4()})";

            if (element.TagOfElement == ContainerRecord.CompressionStyleTag && element.TypeOfValue == PhysicalType.UnsignedInteger4)
                return $"{(CompressionStyle)element.GetUInt4()} ({element.GetUInt4()})";

            if (element.TagOfElement == DataSourceRecord.EquipmentIDTag && element.TypeOfValue == PhysicalType.Guid)
                return $"{Equipment.ToString(element.GetGuid())} ({element.GetGuid()})";

            if (element.TagOfElement == DataSourceRecord.VendorIDTag && element.TypeOfValue == PhysicalType.Guid)
                return $"{Vendor.ToString(element.GetGuid())} ({element.GetGuid()})";

            if (element.TagOfElement == ChannelDefinition.QuantityTypeIDTag && element.TypeOfValue == PhysicalType.Guid)
                return $"{QuantityType.ToString(element.GetGuid())} ({element.GetGuid()})";

            if (element.TagOfElement == ChannelDefinition.QuantityMeasuredIDTag && element.TypeOfValue == PhysicalType.UnsignedInteger4)
                return $"{(QuantityMeasured)element.GetUInt4()} ({element.GetUInt4()})";

            if (element.TagOfElement == SeriesDefinition.QuantityCharacteristicIDTag && element.TypeOfValue == PhysicalType.Guid)
                return $"{QuantityCharacteristic.ToString(element.GetGuid())} ({element.GetGuid()})";

            if (element.TagOfElement == ChannelDefinition.PhaseIDTag && element.TypeOfValue == PhysicalType.UnsignedInteger4)
                return $"{(Phase)element.GetUInt4()} ({element.GetUInt4()})";

            if (element.TagOfElement == SeriesDefinition.ValueTypeIDTag && element.TypeOfValue == PhysicalType.Guid)
                return $"{SeriesValueType.ToString(element.GetGuid())} ({element.GetGuid()})";

            if (element.TagOfElement == SeriesDefinition.QuantityUnitsIDTag && element.TypeOfValue == PhysicalType.UnsignedInteger4)
                return $"{(QuantityUnits)element.GetUInt4()} ({element.GetUInt4()})";

            if (element.TagOfElement == SeriesDefinition.StorageMethodIDTag && element.TypeOfValue == PhysicalType.UnsignedInteger4)
                return $"{(StorageMethods)element.GetUInt4()} ({element.GetUInt4()})";

            // Use a specific date-time format for timestamps
            if (element.TypeOfValue == PhysicalType.Timestamp)
                return element.GetTimestamp().ToString("yyyy-MM-dd HH:mm:ss.fffffff");

            // If the tag could not be recognized as one that can be displayed
            // in a more readable form, this method will parse the type as appropriate
            // based on the physical type defined in the file and convert it to a
            // string using that type's implementation of the ToString() method
            return element.Get().ToString();
        }

        // Converts the value of the given element to a string.
        private string ValueAsString(VectorElement element)
        {
            IEnumerable<string> values;
            string join;

            // The physical types Char1 and Char2 indicate the value is a string
            if (element.TypeOfValue == PhysicalType.Char1)
                return Encoding.ASCII.GetString(element.GetValues()).Trim((char)0);

            if (element.TypeOfValue == PhysicalType.Char2)
                return Encoding.Unicode.GetString(element.GetValues()).Trim((char)0);

            if (element.TypeOfValue == PhysicalType.Timestamp)
            {
                // Use a specific date-time format for timestamps
                values = Enumerable.Range(0, element.Size)
                    .Select(index => element.GetTimestamp(index).ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
            }
            else
            {
                // Other types of values must be parsed as appropriate
                // based on the physical type as defined in the file
                values = Enumerable.Range(0, element.Size)
                    .Select(index => element.Get(index).ToString());
            }

            // Join the values in the collection
            // to a single, comma-separated string
            join = string.Join(", ", values);

            // Wrap the string in curly braces and return
            return $"{{ {join} }}";
        }

        // Fixes the size of the tree view and
        // details text box after the form is resized.
        private void FixSize()
        {
            const int pad = 15;

            int x = ClientRectangle.X + pad;
            int y = ClientRectangle.Y + MenuBar.Height + pad;
            int width = 300;
            int height = ClientRectangle.Height - y - pad;

            ObservationTree.SetBounds(x, y, width, height);

            x += width + pad;
            width = ClientRectangle.Width - x - pad;

            DetailsTextBox.SetBounds(x, y, width, height);
        }

        // Fixes the scroll bars in the details view
        // based on whether the text fits inside the text box.
        private void FixScrollBars()
        {
            Size textSize = TextRenderer.MeasureText(DetailsTextBox.Text, DetailsTextBox.Font);
            bool showVertical = DetailsTextBox.ClientSize.Height < textSize.Height + Convert.ToInt32(DetailsTextBox.Font.Size);
            bool showHorizontal = DetailsTextBox.ClientSize.Width < textSize.Width;

            if (showVertical && showHorizontal)
                DetailsTextBox.ScrollBars = ScrollBars.Both;
            else if (showVertical)
                DetailsTextBox.ScrollBars = ScrollBars.Vertical;
            else if (showHorizontal)
                DetailsTextBox.ScrollBars = ScrollBars.Horizontal;
            else
                DetailsTextBox.ScrollBars = ScrollBars.None;
        }

        // Cancels the next expand or collapse operation in the tree view.
        // This is used to suppress the behavior where double-clicking
        // causes a node to expand or collapse.
        private void CancelExpandCollapseOnce()
        {
            TreeViewCancelEventHandler expandCollapseHandler = null;

            expandCollapseHandler = (expandSender, args) =>
            {
                args.Cancel = true;
                ObservationTree.BeforeExpand -= expandCollapseHandler;
                ObservationTree.BeforeCollapse -= expandCollapseHandler;
            };

            ObservationTree.BeforeExpand += expandCollapseHandler;
            ObservationTree.BeforeCollapse += expandCollapseHandler;
        }

        // Handler called when the window loads.
        private void MainWindow_Load(object sender, EventArgs e)
        {
            m_detailsWindows = new List<DetailsWindow>();

            // Look up tag definitions in the TagDefinitions.xml file
            m_tagLookup = PQDIFExplorer.Tag.GenerateTags(XDocument.Load("TagDefinitions.xml"))
                .ToDictionary(tag => tag.ID);

            // Fix the size of the contents of the form
            FixSize();
        }

        // Handler called when the user selects the option to open a PQDIF file.
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.DefaultExt = ".pqd";
                openFileDialog.Filter = "PQDIF Files|*.pqd|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    OpenFile(openFileDialog.FileName);
            }
        }

        // Handler called after the user selects a node in the tree view.
        private void ObservationTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            object tag;
            Record record;
            Element element;

            string details;

            if ((object)e.Node == null)
                return;

            tag = e.Node.Tag;
            record = tag as Record;
            element = tag as Element;

            // Get details about the record or
            // element selected in the tree view
            if ((object)record != null)
                details = GetDetails(record);
            else if ((object)element != null)
                details = GetDetails(element);
            else
                return;

            // Updates the details text box with
            // information about the selected item
            DetailsTextBox.Clear();
            DetailsTextBox.AppendText(details);
        }

        // Handler called when the user clicks on the tree view.
        private void ObservationTree_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode node;

            object tag;
            Record record;
            Element element;

            string details;
            
            // Only handle mouse down events here if the user
            // double-clicks with the left mouse button
            if (e.Button != MouseButtons.Left || e.Clicks != 2)
                return;

            // Figure out which node the user double-clicked on
            node = ObservationTree.GetNodeAt(ObservationTree.PointToClient(MousePosition));

            if ((object)node == null)
                return;

            tag = node.Tag;
            record = tag as Record;
            element = tag as Element;

            // Get details about the record or
            // element the user double-clicked on
            if ((object)record != null)
                details = GetDetails(record);
            else if ((object)element != null)
                details = GetDetails(element);
            else
                return;

            // Creates a new window in which to display the details
            BeginInvoke(new Action(() =>
            {
                DetailsWindow detailsWindow = new DetailsWindow();
                detailsWindow.SetText(details);
                detailsWindow.Show();
                detailsWindow.FormClosing += (obj, args) => m_detailsWindows.Remove(detailsWindow);
                m_detailsWindows.Add(detailsWindow);
            }));

            // Cancel expand or collapse once to suppress the
            // default behavior of expand/collapse on double-click
            CancelExpandCollapseOnce();
        }

        // Handler called when the user resizes the form.
        private void MainWindow_Resize(object sender, EventArgs e)
        {
            FixSize();
        }

        // Handler called when the text is changed in the details text box.
        private void DetailsTextBox_TextChanged(object sender, EventArgs e)
        {
            FixScrollBars();
        }

        // Handler called when the details text box is resized.
        private void DetailsTextBox_Resize(object sender, EventArgs e)
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(FixScrollBars));
        }

        // Handler called when the user selects the Exit button in the toolbar menu.
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Handler called when the main window is closing so we can clean up
        // any details windows that were spawned while the application was running.
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<DetailsWindow> detailsWindows = new List<DetailsWindow>(m_detailsWindows);

            foreach (DetailsWindow detailsWindow in detailsWindows)
                detailsWindow.Close();
        }
    }
}
