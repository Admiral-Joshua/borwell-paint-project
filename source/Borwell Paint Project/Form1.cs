using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Borwell_Paint_Project
{
    public partial class Form1 : Form
    {
        int LastMouseX;
        int LastMouseY;
        int lastWidth;
        int lastHeight;
        int paintCoverage = 2;

        bool Changes_Saved = true;

        int scaleFactor = 0;

        // DISCLAIMER 1: After realising I'd need to take more than 2 dimensions as input for the L-Shaped panel I decided to exclude it from the final build,
        //  with the features that I have included in this version it is still possible to define an L-Shaped room simply by using 2 square floor panels, then
        //  removing one of the predefined walls.

        // Variables to store the currently selected room name
        //  This will typically only be set when a user is currently dragging around or resizing an element in designing of their room.
        string SelectedRoom = "";

        decimal Pi = Convert.ToDecimal(3.142);

        string resizeMode = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void RefreshElementList()
        {
            while (RoomOverviewPanel.Controls.Count > 0)
            {
                RoomOverviewPanel.Controls.Remove(RoomOverviewPanel.Controls[0]);
            }
            int floorCounter = 0;
            int wallCounter = 0;
            foreach (Control SelectedElement in roomPlan1.Controls)
            {
                string ElementID = SelectedElement.Name.ToString().Replace("RoomElement", "");
                Control SelectedImage = SelectedElement.Controls[("RoomImage" + ElementID).ToString()];

                GroupBox ElementIcon_Group = new GroupBox
                {
                    Width = 130,
                    Height = 150,
                    Name = "ElementIcon_Group" + ElementID.ToString(),
                };
                PictureBox ElementIcon_Image = new PictureBox
                {
                    Width = 90,
                    Height = 90,
                    Name = "ElementIcon_Image" + ElementID.ToString(),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point(20, 15)
                };
                Label ElementIcon_Info = new Label
                {
                    Width = 118,
                    Height = 35,
                    Location = new Point(6, 108),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Name = "ElementIcon_Info" + ElementID.ToString()
                };
                CheckBox ElementIcon_Paint = new CheckBox
                {
                    Text = "Paint",
                    AutoSize = true,
                    Location = new Point(80, 8),
                    Checked = true,
                    Font = new Font("Franklin Gothic Medium", float.Parse("8.25"), FontStyle.Bold),
                    Name = "PaintCheckbox_" + ElementID.ToString()
                };


                if (SelectedImage.Tag.ToString().ToLower().Contains("square"))
                {
                    floorCounter++;
                    ElementIcon_Image.Image = Properties.Resources.FloorPanel;
                    ElementIcon_Group.Text = "Floor #" + floorCounter.ToString();
                }
                else if (SelectedImage.Tag.ToString().ToLower().Contains("triangle"))
                {
                    floorCounter++;
                    ElementIcon_Image.Image = Properties.Resources.FloorPanel2;
                    ElementIcon_Group.Text = "Floor #" + floorCounter.ToString();
                }
                else if (SelectedImage.Tag.ToString().ToLower().Contains("circle"))
                {
                    floorCounter++;
                    ElementIcon_Image.Image = Properties.Resources.FloorPanel3;
                    ElementIcon_Group.Text = "Floor #" + floorCounter.ToString();
                }
                else if (SelectedImage.Tag.ToString().ToLower().Contains("l-shaped"))
                {
                    floorCounter++;
                    ElementIcon_Image.Image = Properties.Resources.FloorPanel4;
                    ElementIcon_Group.Text = "Floor #" + floorCounter.ToString();
                } else
                {
                    wallCounter++;
                    ElementIcon_Image.Image = Properties.Resources.WallPanel;
                    ElementIcon_Group.Text = "Wall #" + wallCounter.ToString();
                    ElementIcon_Group.Controls.Add(ElementIcon_Paint);
                }
                Control RoomWidthReadout = SelectedElement.Controls["WidthReadout" + ElementID];
                Control RoomHeightReadout = SelectedElement.Controls["HeightReadout" + ElementID];
                if (SelectedImage.Tag.ToString() == "wall")
                {
                    ElementIcon_Info.Text = "Width: " + RoomWidthReadout.Text.ToString() + Environment.NewLine + "Height: " + RoomHeightReadout.Text.ToString();
                } else
                {
                    ElementIcon_Info.Text = "Width: " + RoomWidthReadout.Text.ToString() + Environment.NewLine + "Height: " + RoomHeightReadout.Text.ToString();
                }

                // Adding a Delete Button to each element
                Button DeleteBtn = new Button
                {
                    Location = new Point(0, 15),
                    Text = "X",
                    ForeColor = Color.Red,
                    Width = 20,
                    Height = 24,
                    FlatStyle = FlatStyle.Flat,
                    Name = "RoomIcon_Delete" + ElementID.ToString()
                };

                ElementIcon_Group.Controls.Add(ElementIcon_Image);
                ElementIcon_Group.Controls.Add(ElementIcon_Info);
                ElementIcon_Group.Controls.Add(DeleteBtn);
                RoomOverviewPanel.Controls.Add(ElementIcon_Group);

                ElementIcon_Group.Click += (sender2, e2) => ShowElement(SelectedElement.Name.ToString());
                ElementIcon_Image.Click += (sender2, e2) => ShowElement(SelectedElement.Name.ToString());
                ElementIcon_Info.Click += (sender2, e2) => ShowElement(SelectedElement.Name.ToString());
                ElementIcon_Paint.CheckedChanged += (sender2, e2) => Update_Wall_Area();

                DeleteBtn.Click += (sender2, e2) => Delete_Element(SelectedElement.Name.ToString(), ElementIcon_Group, false);
            }
        }

        public string[] SplitAtLines(string inputString)
        {
            string[] lines = inputString.Split(
                        new[] { Environment.NewLine },
                        StringSplitOptions.None
                    );

            return lines;
        }

        private void Delete_Element(string ElementName, Control RoomGroup, bool Force)
        {
            try
            {
                Control SelectedElement = roomPlan1.Controls[ElementName];
                DialogResult dialogResult = DialogResult.Yes;
                if (Force == false)
                {
                    dialogResult = MessageBox.Show("Are you sure you want to delete '" + RoomGroup.Text.ToString() + "'?", "Are you sure?", MessageBoxButtons.YesNo);
                }
                if (dialogResult == DialogResult.Yes)
                {
                    roomPlan1.Controls.Remove(SelectedElement);
                    SelectedElement.Dispose();
                    if (RoomGroup != null)
                    {
                        RoomOverviewPanel.Controls.Remove(RoomGroup);
                        RoomGroup.Dispose();
                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show("Room Element Deletion failed. Please try again." + Environment.NewLine + Environment.NewLine + "Error Details:" + ex.Message.ToString());
            }

            Update_Floor_Area();
            Update_Wall_Area();
        }

        private void Update_Wall_Area()
        {
            decimal TotalArea = 0;
            decimal TotalArea2 = 0;
            foreach (Control SelectedIcon in RoomOverviewPanel.Controls)
            {
                string ElementID = SelectedIcon.Name.ToString().Replace("ElementIcon_Group", "");
                try {
                    Control PaintCheck = SelectedIcon.Controls["PaintCheckbox_" + ElementID];
                    CheckBox PaintChecked = PaintCheck as CheckBox;
                    string ElementDimensions = SelectedIcon.Controls["ElementIcon_Info" + ElementID.ToString()].Text.ToString();

                    string[] lines = SplitAtLines(ElementDimensions.ToString());
                    float ElementWidth = float.Parse(lines[0].Replace("Width: ", "").Replace("m", ""));
                    float ElementHeight = float.Parse(lines[1].Replace("Height: ", "").Replace("m", ""));

                    if (PaintChecked.Checked == true)
                    {
                        TotalArea += Convert.ToDecimal(ElementWidth * ElementHeight);
                    } else
                    {
                        TotalArea2 += Convert.ToDecimal(ElementWidth * ElementHeight);
                    }

                    paintAreaOutput1.Text = Math.Round(TotalArea, 2).ToString() + "m";
                    WallAreaOutput1.Text = Math.Round(TotalArea + TotalArea2, 2).ToString() + "m";
                } catch
                {
                    // Do Nothing (For now at least) - This will happen as a checkbox is not present in elements that are not walls (e.g. Floor Elements do not have checkboxes).
                    //   As a result, we use try-catch just to stop any error messages from popping up. We don't want all that ruining user experience.
                }
            }
            Calculate_Paint_Required();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            trackBar1.Value = 1;
            scaleFactor = 2;
            scaleOutputLabel.Text = "Scale: " + ConvertToM_CM(scaleFactor).ToString() + " per Pixel";
        }

        private void DisableResize(string ElementName)
        {
            SelectedRoom = "";
            updateResize1.Stop();

            string ElementID = ElementName.ToString().Replace("RoomElement", "");
            Control SelectedElement = roomPlan1.Controls[ElementName];
            Control SelectedImage = SelectedElement.Controls["RoomImage" + ElementID];

            if (checkBox2.Checked == true && SelectedImage.Tag.ToString().Contains("wall"))
            {
                Match_Heights(SelectedElement.Height);
            }

            Changes_Saved = false;
        }

        private void ShowElement(string ElementName)
        {
            foreach (Control element in roomPlan1.Controls)
            {
                if (element.Name.ToString() == ElementName)
                {
                    element.Visible = true;
                } else
                {
                    element.Visible = false;
                }
            }
        }

        int RoomItemNumber = 0;

        private void AddRoomElement(string objectType, string createType)
        {
            Panel roomElement = new Panel()
            {
                Width = 130,
                Height = 130,
                Name = "RoomElement" + RoomItemNumber.ToString(),
                Location = new Point(110, 110)
            };

            Button ResizeWidthBtn = new Button()
            {
                Width = 22,
                Height = 22,
                Name = "RoomResizeW_" + RoomItemNumber.ToString(),
                Font = new Font("Franklin Gothic Medium", 8),
                Text = "↔",
                Location = new Point(roomElement.Width - 22, (roomElement.Height / 2) - 11)
            };
            Button ResizeHeightBtn = new Button()
            {
                Width = 22,
                Height = 22,
                Name = "RoomResizeH_" + RoomItemNumber.ToString(),
                Font = new Font("Franklin Gothic Medium", 8),
                Text = "↨",
                Location = new Point((roomElement.Width / 2) - 22, roomElement.Height - 22)
            };

            TextBox RoomSizeHeight = new TextBox()
            {
                Width = 50,
                Height = 20,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Franklin Gothic Medium", 8),
                Name = "WidthReadout" + RoomItemNumber.ToString()
            };

            TextBox RoomSizeWidth = new TextBox() {
                Width = 50,
                Height = 20,
                TextAlign = HorizontalAlignment.Center,
                Font = new Font("Franklin Gothic Medium", 8),
                Name = "HeightReadout" + RoomItemNumber.ToString()
            };

            PictureBox roomImage = new PictureBox() {
                Width = 130,
                Height = 130,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Name = "RoomImage" + RoomItemNumber.ToString(),
                Location = new Point(0, 0),
                Dock = DockStyle.Fill,
                Tag = objectType.ToLower().Replace("floor:", "")
            };

            // Choosing the Correct Image for the Element
            if (objectType.ToLower().Contains("floor"))
            {
                if (objectType.ToLower().Contains("square"))
                {
                    roomImage.Image = Properties.Resources.FloorPanel;
                    RoomSizeWidth.Location = new Point((roomElement.Width / 2) - 25, 0);
                    RoomSizeHeight.Location = new Point(0, (roomElement.Height / 2) - 10);
                }
                else if (objectType.ToLower().Contains("triangle"))
                {
                    roomImage.Image = Properties.Resources.FloorPanel2;
                    RoomSizeWidth.Location = new Point((roomElement.Width / 2), roomElement.Height - 20);
                    RoomSizeHeight.Location = new Point(roomElement.Width / 2 - 50, roomElement.Height / 2 - 10);
                }
                else if (objectType.ToLower().Contains("circle"))
                {
                    roomImage.Image = Properties.Resources.FloorPanel3;
                    RoomSizeWidth.Location = new Point(roomElement.Width / 2 - 50, roomElement.Height / 2 - 10);
                    RoomSizeHeight.Location = new Point(roomElement.Width / 2 - 50, roomElement.Height / 2 - 10);
                }
                else if (objectType.ToLower().Contains("l-shape"))
                {
                    roomImage.Image = Properties.Resources.FloorPanel4;
                    RoomSizeWidth.Location = new Point(roomElement.Width / 2 - 50, 0);
                    RoomSizeHeight.Location = new Point(0, roomElement.Height / 2 - 10);
                }
            } else
            {
                // If not a floor element, assumed a wall (to ensure nothing is left unassigned)
                roomImage.Image = Properties.Resources.WallPanel;
                RoomSizeWidth.Location = new Point((roomElement.Width / 2) - 25, 0);
                RoomSizeHeight.Location = new Point(0, (roomElement.Height / 2) - 10);
            }

            roomImage.Cursor = Cursors.SizeAll;

            roomPlan1.Controls.Add(roomElement);

            // Loading all our defined controls into the GroupBox now before loading it into the panel.
            roomElement.Controls.Add(RoomSizeWidth);
            roomElement.Controls.Add(RoomSizeHeight);
            roomElement.Controls.Add(ResizeWidthBtn);
            roomElement.Controls.Add(ResizeHeightBtn);
            roomElement.Controls.Add(roomImage);

            roomImage.BringToFront();
            RoomSizeHeight.BringToFront();
            RoomSizeWidth.BringToFront();
            ResizeWidthBtn.BringToFront();
            ResizeHeightBtn.BringToFront();
            roomElement.BringToFront();


            //            RefreshElementList();
            //UpdateDimensions(roomImage.Name.ToString(), roomElement.Name.ToString());

            roomElement.Refresh();
            roomPlan1.Refresh();

            // Adding the appropriate Event Handlers so that rooms, once added can be resized
            //  as well as moved around with the mouse later on.
            roomImage.MouseDown += (sender2, e2) => EnableResize(roomElement.Name, "floor", "move");
            roomImage.MouseUp += (sender2, e2) => DisableResize(roomElement.Name.ToString());

            ResizeWidthBtn.MouseDown += (sender2, e2) => EnableResize(roomElement.Name.ToString(), "floor", "x-mode");
            ResizeWidthBtn.MouseUp += (sender2, e2) => DisableResize(roomElement.Name.ToString());

            ResizeHeightBtn.MouseDown += (sender2, e2) => EnableResize(roomElement.Name.ToString(), "floor", "y-mode");
            ResizeHeightBtn.MouseUp += (sender2, e2) => DisableResize(roomElement.Name.ToString());

            RefreshElementList();
            UpdateDimensions("LOL, DON'T NEED IT", roomElement.Name.ToString());

            RoomItemNumber++;

            if (createType == "normal") {
                DialogResult MsgboxResult = MessageBox.Show("Would you like the program to automatically add the necessary walls to your new object?", "Optional Extra", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (MsgboxResult == DialogResult.Yes) { 
                    if (objectType.ToString().ToLower().Contains("square"))
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            AddRoomElement("wall", "load");
                        }
                    }
                    else if (objectType.ToString().ToLower().Contains("triangle"))
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            AddRoomElement("wall", "load");

                        }
                    }
                    else if (objectType.ToString().ToLower().Contains("circle"))
                    {
                        AddRoomElement("wall", "load");
                    }
                }
            }

            Changes_Saved = false;
            ShowElement(roomElement.Name.ToString());
        }

        private void Match_Heights(int TargetHeight)
        {
            // This function will, when enabled will match the heights of all other walls with the currently selected.
            foreach (Control SelectedElement in roomPlan1.Controls)
            {
                string ElementID = SelectedElement.Name.ToString().Replace("RoomElement", "");
                Control SelectedImage = SelectedElement.Controls["RoomImage" + ElementID];

                // I did originally store this code inside the ResizeLoop / Timer Object, but it was proving to be quite
                //  -inefficient when you started to scale up and have a lot more rooms. So I moved it to here so it runs
                //  only after you have finished resizing your element.
                if (SelectedImage.Tag.ToString().Contains("wall") && SelectedElement.Height != TargetHeight)
                {
                    SelectedElement.Height = TargetHeight;
                    UpdateDimensions(SelectedImage.Name.ToString(), SelectedElement.Name.ToString());
                }
            }
        }

        public void UpdateDimensions(string roomImageName, string roomGroupboxName)
        {
            //ElementGroup.Name = "InfoReadout_" + ElementID;
            //ElementInfo.Name = "InfoText_" + ElementID;

            Control selectedRoomElement = roomPlan1.Controls[roomGroupboxName.ToString()];
            string ElementID = selectedRoomElement.Name.Replace("RoomElement", "");

            Control roomElementHeightTextbox = selectedRoomElement.Controls[("WidthReadout" + ElementID.ToString())];
            Control roomElementWidthTextbox = selectedRoomElement.Controls[("HeightReadout" + ElementID.ToString())];

            string DimensionsX = ConvertToPureM(selectedRoomElement.Height * scaleFactor).ToString() + "m";
            string DimensionsY = ConvertToPureM(selectedRoomElement.Width * scaleFactor).ToString() + "m";

            roomElementWidthTextbox.Text = DimensionsX;
            roomElementHeightTextbox.Text = DimensionsY;


            // Reloading Current Dimensions into the Element Info Panel at Bottom of Screen
            //RoomElement.Name.ToString() + "_InfoReadout";
            Control SelectedInfoPanel = RoomOverviewPanel.Controls[("ElementIcon_Group" + ElementID.ToString())];
            Control SelectedInfoText = SelectedInfoPanel.Controls[("ElementIcon_Info" + ElementID.ToString())];
            SelectedInfoText.Text = "Width: " + DimensionsY.ToString() + Environment.NewLine + "Height: " + DimensionsX.ToString();

            Control ResizeBtnW = selectedRoomElement.Controls["RoomResizeW_" + ElementID];
            Control ResizeBtnH = selectedRoomElement.Controls["RoomResizeH_" + ElementID];
            Control WidthReadout = selectedRoomElement.Controls["WidthReadout" + ElementID];
            Control HeightReadout = selectedRoomElement.Controls["HeightReadout" + ElementID];

            Control roomImage = selectedRoomElement.Controls["RoomImage" + ElementID];
            ResizeBtnW.Location = new Point(selectedRoomElement.Width - 22, (selectedRoomElement.Height / 2) - 11);
            ResizeBtnH.Location = new Point((selectedRoomElement.Width / 2) - 22, selectedRoomElement.Height - 22);

            if (roomImage.Tag.ToString().ToLower().Contains("square") || roomImage.Tag.ToString().ToLower().Contains("wall"))
            {
                WidthReadout.Location = new Point((selectedRoomElement.Width / 2) - 25, 0);
                HeightReadout.Location = new Point(0, (selectedRoomElement.Height / 2) - 10);
            }
            else if (roomImage.Tag.ToString().ToLower().Contains("triangle"))
            {
                WidthReadout.Location = new Point((selectedRoomElement.Width / 2) + 1, selectedRoomElement.Height - 20);
                HeightReadout.Location = new Point(selectedRoomElement.Width / 2 - 25, selectedRoomElement.Height / 2);
            }
            else if (roomImage.Tag.ToString().ToLower().Contains("circle"))
            {
                WidthReadout.Location = new Point(selectedRoomElement.Width / 2 - 25, selectedRoomElement.Height / 2 - 10);
                HeightReadout.Location = new Point(selectedRoomElement.Width / 2 - 25, selectedRoomElement.Height / 2 - 10);
            }
            else if (roomImage.Tag.ToString().ToLower().Contains("l-shape"))
            { 
                WidthReadout.Location = new Point(selectedRoomElement.Width / 2 - 50, 0);
                HeightReadout.Location = new Point(0, selectedRoomElement.Height / 2 - 10);
            }

            Update_Floor_Area();
            Update_Wall_Area();
            Update_Volume();
            Calculate_Paint_Required();
        }

        private void EnableResize(string selectedElementName, string roomContainer, string resizeType)
        {
            try {
                string selectedElementID = selectedElementName.Replace("RoomElement", "");
                Control SelectedRoomElement = null;
                Control selectedRoomImage = null;
                if (roomContainer == "floor")
                {
                    SelectedRoomElement = roomPlan1.Controls[selectedElementName];
                    selectedRoomImage = SelectedRoomElement.Controls["RoomImage" + selectedElementID.ToString()];
                }

                LastMouseX = Cursor.Position.X;
                LastMouseY = Cursor.Position.Y;
                lastWidth = selectedRoomImage.Width;
                lastHeight = selectedRoomImage.Height;

                SelectedRoom = selectedElementName;

                if (resizeType == "x-mode")
                {
                    resizeMode = "X_Plane";
                }
                else if (resizeType == "y-mode")
                {
                    resizeMode = "Y_Plane";
                }
                else if (resizeType == "move")
                {
                    resizeMode = "Move";
                }

                updateResize1.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Select '" + selectedElementName + "'" + Environment.NewLine + ex.Message);
            }
        }

        private void Update_Volume()
        {
            decimal FloorSurfaceArea = Convert.ToDecimal(surfaceAreaOutput.Text.Replace("m", "").Replace(" ", ""));
            int loopCounter = 0;
            foreach (Control SelectedIcon in RoomOverviewPanel.Controls)
            {
                loopCounter++;
                if (SelectedIcon.Text.ToLower().Contains("wall"))
                {
                    string ElementID = SelectedIcon.Name.ToString().Replace("ElementIcon_Group", "");
                    Control ElementInfo = SelectedIcon.Controls["ElementIcon_Info" + ElementID];
                    string[] lines = SplitAtLines(ElementInfo.Text.ToString());
                    decimal roomHeight = Convert.ToDecimal(lines[1].ToString().Replace("m", "").Replace(" ", "").Replace("Height:", ""));
                    decimal TotalVolume = FloorSurfaceArea * roomHeight;
                    roomVolumeOutput.Text = Math.Round(TotalVolume, 2).ToString() + "m";
                    break;
                }
                if (loopCounter <= 0)
                {
                    // Do Nothing (yet) - This will only occur if no walls exist in the current room.
                    roomVolumeOutput.Text = "0m";
                }
            }
        }

        private void UpdateDrag1_Tick(object sender, EventArgs e)
        {
            if (SelectedRoom != "")
            {
                Control SelectedElement = roomPlan1.Controls[SelectedRoom];
                string ElementID = SelectedRoom.Replace("RoomElement", "");
                Control SelectedImage = SelectedElement.Controls[("RoomImage" + ElementID)];

                if (resizeMode == "X_Plane")
                {
                    if (Cursor.Position.X > LastMouseX || Cursor.Position.X < LastMouseX)
                    {
                        int DeltaX = Cursor.Position.X - LastMouseX;

                        SelectedElement.Width = lastWidth + DeltaX;

                        if (SelectedImage.Tag.ToString() == "circle")
                        {
                            SelectedElement.Height = lastHeight + DeltaX;
                        }
                    }
                    LastMouseX = Cursor.Position.X;
                    UpdateDimensions(SelectedImage.Name.ToString(), SelectedElement.Name.ToString());
                }
                if (resizeMode == "Y_Plane")
                {
                    if (Cursor.Position.Y < LastMouseY || Cursor.Position.Y > LastMouseY)
                    {
                        int DeltaY = Cursor.Position.Y - LastMouseY;

                        SelectedElement.Height = lastHeight + DeltaY;

                        if (SelectedImage.Tag.ToString() == "circle")
                        {
                            SelectedElement.Width = lastWidth + DeltaY;
                        }

                    }
                    LastMouseY = Cursor.Position.Y;
                    

                    UpdateDimensions(SelectedImage.Name.ToString(), SelectedElement.Name.ToString());
                }

                lastWidth = SelectedElement.Width;
                lastHeight = SelectedElement.Height;

                if (resizeMode == "Move")
                {
                    if (Cursor.Position.X < LastMouseX || Cursor.Position.X > LastMouseX)
                    {
                        int DeltaX = Cursor.Position.X - LastMouseX;
                        SelectedElement.Location = new Point(SelectedElement.Location.X + DeltaX, SelectedElement.Location.Y);
                    }
                    if (Cursor.Position.Y < LastMouseY || Cursor.Position.Y > LastMouseY)
                    {
                        int DeltaY = Cursor.Position.Y - LastMouseY;
                        SelectedElement.Location = new Point(SelectedElement.Location.X, SelectedElement.Location.Y + DeltaY);
                    }
                    LastMouseX = Cursor.Position.X;
                    LastMouseY = Cursor.Position.Y;
                }
            }
        }

        private void Update_Floor_Area()
        {
            decimal TotalArea = 0;

            foreach (Control Element in roomPlan1.Controls)
            {
                string ElementID = Element.Name.ToString().Replace("RoomElement", "");
                Control RoomIcon = Element.Controls[("RoomImage" + ElementID).ToString()];

                if (Element.Name.ToString().Contains("RoomElement"))
                {
                    string TargetName = "RoomImage" + ElementID.ToString();

                    decimal ElementWidth = ConvertToPureM(RoomIcon.Width * scaleFactor);
                    decimal ElementHeight = ConvertToPureM(RoomIcon.Height * scaleFactor);

                    if (RoomIcon.Tag.ToString() == "square")
                    {
                        TotalArea += ElementWidth * ElementHeight;
                    }
                    if (RoomIcon.Tag.ToString() == "triangle")
                    {
                        TotalArea += ((ElementWidth * ElementHeight) / 2);
                    }
                    if (RoomIcon.Tag.ToString() == "circle")
                    {
                        TotalArea += ((ElementWidth * ElementWidth) * Pi);
                    }
                }
            }
            surfaceAreaOutput.Text = Math.Round(TotalArea, 2).ToString() + " m";
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value == 1)
            {
                scaleFactor = 2;
            }
            else if (trackBar1.Value == 2)
            {
                scaleFactor = 4;
            }
            else if (trackBar1.Value == 3)
            {
                scaleFactor = 6;
            }
            else if (trackBar1.Value == 4)
            {
                scaleFactor = 8;
            }
            else if (trackBar1.Value == 5)
            {
                scaleFactor = 10;
            }
            else if (trackBar1.Value == 6)
            {
                scaleFactor = 12;
            }
            else if (trackBar1.Value == 7)
            {
                scaleFactor = 14;
            }
            else if (trackBar1.Value == 8)
            {
                scaleFactor = 16;
            }
            else if (trackBar1.Value == 9)
            {
                scaleFactor = 20;
            }
            scaleOutputLabel.Text = "Scale: " + ConvertToM_CM(scaleFactor).ToString() + " per Pixel";

            List<string> RoomElements = new List<string>();

            int loopCounter = 0;

            while (loopCounter < roomPlan1.Controls.Count)
            {
                if (roomPlan1.Controls[loopCounter].Name.ToString().Contains("RoomElement"))
                {
                    string ElementID = roomPlan1.Controls[loopCounter].Name.ToString().Replace("RoomElement", "");
                    UpdateDimensions("RoomImage" + ElementID, "RoomElement" + ElementID);
                }
                loopCounter++;
            }

        }

        public string ConvertToM_CM(int inputCM)
        {
            int wholeMetres = inputCM / 100;
            int leftOverCentimetres = inputCM % 100;

            string returnString = "";

            if (wholeMetres > 0)
            {
                returnString = wholeMetres.ToString() + "m";
            }
            if (leftOverCentimetres > 0)
            {
                returnString += " & " + leftOverCentimetres.ToString() + "cm";
            }
            return returnString;
        }
        
        public decimal ConvertToCM(decimal InputM)
        {
            decimal pureCenti = InputM * 100;
            return pureCenti;
        }

        public decimal ConvertToPureM(decimal inputCM)
        {
            decimal pureMetres = inputCM / 100;
            return pureMetres;
        }

        private void FloorPlanButton_Click(object sender, EventArgs e)
        {
            if (roomPlan1.Height == 435)
            {
                roomPlan1.Height = 36;
            }
            else if (roomPlan1.Height == 36)
            {
                roomPlan1.Height = 435;
            }
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
            AddRoomElement("floor:square", "normal");
        }

        private void PictureBox4_Click(object sender, EventArgs e)
        {
            AddRoomElement("floor:triangle", "normal");
        }

        private void PictureBox3_Click(object sender, EventArgs e)
        {
            AddRoomElement("floor:circle", "normal");
        }

        private void PictureBox5_Click(object sender, EventArgs e)
        {
            AddRoomElement("floor:l-shape", "normal");
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == false)
            {
                DialogResult msgboxResult = MessageBox.Show("Are you sure you want to disable Wall Height Lock?" + Environment.NewLine + "The application currently does not support Volume calculations without this feature being on.", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (msgboxResult == DialogResult.No)
                {
                    checkBox2.Checked = true;
                }
            }
        }

        private void Calculate_Paint_Required()
        {
            decimal areaToCover = Convert.ToDecimal(paintAreaOutput1.Text.Replace("m", "").Replace(" ", ""));
            decimal PaintRequired = areaToCover / paintCoverage;
            paintRequiredOutput.Text = Math.Round(PaintRequired, 2).ToString() + "Litres";
        }

        private void TrackBar2_Scroll(object sender, EventArgs e)
        {
            paintCoverage = trackBar2.Value;
            Calculate_Paint_Required();
            coverageOutput.Text = paintCoverage.ToString() + "m";
        }

        public void Save_Project(string TargetPath)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(TargetPath))
            {
                foreach (Control RoomElement in roomPlan1.Controls)
                {
                    string ElementID = RoomElement.Name.ToString().Replace("RoomElement", "");
                    Control RoomImage = RoomElement.Controls["RoomImage" + ElementID];
                    
                    if (RoomElement.Visible == true)
                    {
                        file.WriteLine("ELEMENT:" + RoomElement.Name.ToString().Replace(ElementID, "") + "$#$" + RoomImage.Tag.ToString() + "$#$width:" + RoomImage.Width + "$#$height:" + RoomImage.Height + "$#$SELECTED$#$");
                    } else
                    {
                        file.WriteLine("ELEMENT:" + RoomElement.Name.ToString().Replace(ElementID, "") + "$#$" + RoomImage.Tag.ToString() + "$#$width:" + RoomImage.Width + "$#$height:" + RoomImage.Height + "$#$");
                    }
                }
                file.Close();
                Changes_Saved = true;
            }
        }
        public void Clear_Project()
        {
            if (roomPlan1.Controls.Count > 0 && Changes_Saved == false) {
                DialogResult MsgResult = MessageBox.Show("You have unsaved changes in your room design. Would you like to save changes?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (MsgResult == DialogResult.Yes)
                {
                    saveFileDialog1.ShowDialog();
                }
            }

            while (roomPlan1.Controls.Count > 0)
            {
                Control RoomElement = roomPlan1.Controls[0];
                if (RoomElement.Name.ToString().Contains("RoomElement"))
                {
                    Delete_Element(RoomElement.Name.ToString(), null, true);
                }
            }
            while (RoomOverviewPanel.Controls.Count > 0)
            {
                RoomOverviewPanel.Controls.Remove(RoomOverviewPanel.Controls[0]);
            }

            surfaceAreaOutput.Text = "0m";
            WallAreaOutput1.Text = "0m";
            paintAreaOutput1.Text = "0m";
            roomVolumeOutput.Text = "0m";
            paintRequiredOutput.Text = "0.0 Litres";

            //if (exitAfter)
            //{
            //    Environment.Exit(0);
            //}
        }
        private void Load_Project(string TargetPath)
        {
            Clear_Project();
            using (System.IO.StreamReader file =
                new System.IO.StreamReader(TargetPath))
            {
                string LineRead = file.ReadLine();

                while (LineRead != "" || LineRead != null)
                {
                    if (LineRead == null)
                    {
                        break;
                    }
                    if (LineRead.Contains("ELEMENT"))
                    {
                        LineRead = LineRead.Replace("ELEMENT:", "");
                        string TempStorage = "";

                        string ElementName = "";
                        string ElementType = "";
                        string ElementWidth = "";
                        string ElementHeight = "";
                        string SelectedElement = "";

                        int loopCounter = 0;

                        foreach (char Character in LineRead)
                        {
                            TempStorage += Character.ToString();
                            if (TempStorage.Contains("$#$"))
                            {
                                TempStorage = TempStorage.Replace("$#$", "");
                                if (loopCounter == 0)
                                {
                                    ElementName = TempStorage;
                                } else if (loopCounter == 1)
                                {
                                    ElementType = TempStorage;
                                } else if (loopCounter == 2)
                                {
                                    ElementWidth = TempStorage.Replace("width:", "");
                                }
                                else if (loopCounter == 3)
                                {
                                    ElementHeight = TempStorage.Replace("height:", "");
                                }
                                else if (loopCounter == 4)
                                {
                                    SelectedElement = TempStorage;
                                }
                                TempStorage = "";
                                loopCounter++;
                            }
                        }
                        if (ElementType.Contains("square"))
                        {
                            AddRoomElement("floor:square", "load");
                        }
                        else if (ElementType.Contains("triangle"))
                        {
                            AddRoomElement("floor:traingle", "load");
                        } else if (ElementType.Contains("circle"))
                        {
                            AddRoomElement("floor:circle", "load");
                        } else if (ElementType.Contains("l-shape"))
                        {
                            AddRoomElement("floor:l-shape", "load");
                        } else if (ElementType.Contains("wall"))
                        {
                            AddRoomElement("wall", "load");
                        }

                        if (SelectedElement == "SELECTED")
                        {
                            ShowElement("RoomElement" + (RoomItemNumber - 1).ToString());
                        }

                        Control NewElement = roomPlan1.Controls["RoomElement" + (RoomItemNumber - 1).ToString()];
                        NewElement.Width = Convert.ToInt16(ElementWidth);
                        NewElement.Height = Convert.ToInt16(ElementHeight);
                        UpdateDimensions("", NewElement.Name.ToString());
                    }

                    LineRead = file.ReadLine();
                }
            }
            Changes_Saved = true;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear_Project();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Save_Project(saveFileDialog1.FileName.ToString());
            this.Text = "Borwell Paint Project - Editing: " + System.IO.Path.GetFileName(saveFileDialog1.FileName).ToString();
        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            Load_Project(openFileDialog1.FileName.ToString());
            this.Text = "Borwell Paint Project - Editing: " + openFileDialog1.SafeFileName.ToString();
        }

        private void PictureBox7_Click(object sender, EventArgs e)
        {
            AddRoomElement("wall", "normal");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (roomPlan1.Controls.Count > 0)
            {
                Clear_Project();
            }
        }
    }
}
