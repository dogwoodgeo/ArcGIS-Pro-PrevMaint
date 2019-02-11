using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Editing.Attributes;

namespace PreventiveMaintenance
{
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("PreventiveMaintenance_Module"));
            }
        }

        public static void SewerLinesStatus()
        {
            try
            {
                var map = MapView.Active.Map;
                map.SetSelection(null);// Clears all current selections
                var mhExists = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(m => m.Name == "Manholes");
                var sewerExists = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().Any(s => s.Name == "Sewer Lines");

                // Check for the SEWER LINES Layer and MANHOLES layers in the map.
                if (!sewerExists)
                {
                    MessageBox.Show("Sewer Lines are missing from map.\n 'Sewer Lines' " +
                        "layers must be named exactly as such for trace to work",
                        "WARNING!");
                }

                else
                {
                    //Make manholes the only selectabe layer in map.
                    var layers = map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
                    foreach (var layer in layers)
                    {

                        if (layer.Name == "Sewer Lines")
                        {
                            layer.SetSelectable(true);
                        }
                        else
                        {
                            layer.SetSelectable(false);
                        }
                    }
                }
            }

            catch (Exception)
            {
                string caption = "WARNING!";
                string message = "Process failed. \n\nSave and restart ArcGIS Pro and try process again.\n\n" +
                    "If problem persist, contact your local GIS nerd.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);
            }
        }

        public static string GetCompkey()
        {
            string compkey;
            
            
            try
            {
                //ActiveMapView.SelectFeatures(geometry, SelectionCombinationMethod.New);

                var map = MapView.Active.Map;
                var mhLayer = map.GetLayersAsFlattenedList().OfType<FeatureLayer>().FirstOrDefault((m => m.Name == "Sewer Lines"));

                // Get the currently selected features in the map
                var selectedFeatures = map.GetSelection();
                var selectCount = mhLayer.SelectionCount;

                if (selectCount == 0)
                {
                    MessageBox.Show("No Sewer Line was selected.\n\nTry selection again.", "WARNING!");
                }
                else if (selectCount > 1)
                {
                    MessageBox.Show("More than one Sewer Line was selected.\n\n" +
                        "Try selecting Sewer Line again.\n\nZooming in may help.", "WARNING!");
                }
                else
                {
                    // get the first layer and its corresponding selected feature OIDs
                    var firstSelectionSet = selectedFeatures.First();

                    // create an instance of the inspector class
                    var inspector = new Inspector();

                    // load the selected features into the inspector using a list of object IDs
                    inspector.Load(firstSelectionSet.Key, firstSelectionSet.Value);

                    //get the value of
                     return compkey = inspector["COMPKEY"].ToString();
                    
                }
  
            }

            catch (Exception)
            {
                string caption = "WARNING!";
                string message = "Precess failed! \n\nSave and restart ArcGIS Pro and try process again.\n\n" +
                    "If problem persist, contact your local GIS nerd.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);

                return null;
            }

            return null;

        }

        //public static StandaloneTable PrevMaintTable(string slComp)
        //{
            //if (!string.IsNullOrEmpty(slComp))
            //{
            //    Uri path = new Uri("O:\\SHARE\\405 - INFORMATION SERVICES\\GIS_Layers\\GISVIEWER.SDE@SQL0.sde");

            //    // Set up Geodatabase Object)
            //    using (Geodatabase geodatabase = new Geodatabase(new DatabaseConnectionFile(path)))
            //    {
            //        string queryString = $"COMPKEY = {slComp}";
            //        QueryDef queryDef = new QueryDef()
            //        {
            //            Tables = "SDE.sewerman.tblV8PMTOOL",
            //            WhereClause = queryString,
            //        };

            //        QueryTableDescription queryTableDescription = new QueryTableDescription(queryDef)
            //        {
            //            MakeCopy = true,
            //            Name = $"Preventive Maintenance: {slComp}.",
            //            PrimaryKeys = geodatabase.GetSQLSyntax().QualifyColumnName("SDE.sewerman.tblV8PMTOOL", "COMPKEY")
            //        };

            //        var queryTable = geodatabase.OpenQueryTable(queryTableDescription);

            //        int count = queryTable.GetCount();
            //        if (count == 0)
            //        {
            //            MessageBox.Show("Sewer line selected has no preventive maintenance scheduled.");
            //            return null;
            //        }

            //        else
            //        {
            //            ////Used since query table doesn't display
            //            //MessageBox.Show($"Row count: {count}");
            //            // Create a standalone table from the queryTable Table
            //            IStandaloneTableFactory tableFactory = StandaloneTableFactory.Instance;
            //            StandaloneTable standaloneTable = tableFactory.CreateStandaloneTable(queryTable, MapView.Active.Map);
            //            return standaloneTable;
            //            // Open the standalone table into a window
            //            //FrameworkApplication.Panes.OpenTablePane(standaloneTable, TableViewMode.eAllRecords);
            //        }
                    
            //    }
                
            //};

        //    return null;
        //}

        //protected static ITablePane OpenTPane(StandaloneTable standaloneTable)
        //{
        //    return FrameworkApplication.Panes.OpenTablePane(standaloneTable, TableViewMode.eAllRecords);
        //}
        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }



        #endregion Overrides

    }
}
