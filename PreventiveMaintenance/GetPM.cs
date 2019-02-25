using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace PreventiveMaintenance
{
    internal class GetPM : MapTool
    {   
        string slComp;
        
        
        
        public GetPM()
        {  
            IsSketchTool = true;
            SketchType = SketchGeometryType.Point;
            SketchOutputMode = SketchOutputMode.Screen; 
        }


        protected override Task OnToolActivateAsync(bool active)
        {
            return QueuedTask.Run(() =>
            {
                Module1.SewerLinesStatus();
                
            });
            //return base.OnToolActivateAsync(active);
        }


        protected override async Task<bool> OnSketchCompleteAsync(Geometry geometry)
        {
            try
            {        
                var standaloneTable =  await QueuedTask.Run(() =>
                {
                    ActiveMapView.SelectFeatures(geometry, SelectionCombinationMethod.New);
                    slComp = Module1.GetCompkey();
                    if (!string.IsNullOrEmpty(slComp))
                    {
                        Uri path = new Uri("O:\\SHARE\\405 - INFORMATION SERVICES\\GIS_Layers\\GISVIEWER.SDE@SQL0.sde");

                        // Set up Geodatabase Object)
                        using (Geodatabase geodatabase = new Geodatabase(new DatabaseConnectionFile(path)))
                        {
                            string queryString = $"COMPKEY = {slComp}";
                            QueryDef queryDef = new QueryDef()
                            {
                                Tables = "SDE.sewerman.tblV8PMTOOL",
                                WhereClause = queryString,
                            };

                            QueryTableDescription queryTableDescription = new QueryTableDescription(queryDef)
                            {
                                MakeCopy = true,
                                Name = $"Preventive Maintenance: {slComp}",
                                PrimaryKeys = geodatabase.GetSQLSyntax().QualifyColumnName("SDE.sewerman.tblV8PMTOOL", "COMPKEY")
                            };

                            var queryTable = geodatabase.OpenQueryTable(queryTableDescription);

                            int count = queryTable.GetCount();
                            if (count == 0)
                            {
                                MessageBox.Show("Sewer line selected has no preventive maintenance scheduled.");
                            
                            }

                            else
                            {
                                // Create a standalone table from the queryTable Table
                                IStandaloneTableFactory tableFactory = StandaloneTableFactory.Instance;
                                StandaloneTable pmTable = tableFactory.CreateStandaloneTable(queryTable, MapView.Active.Map);

                                return pmTable;
                            }                     
                        }
                    };
                    //return base.OnSketchCompleteAsync(geometry);
                    return null;
                });
                // Open the standalone table pane
                FrameworkApplication.Panes.OpenTablePane(standaloneTable, TableViewMode.eAllRecords);
                //return true;
            }
           
            catch (Exception)
            {
                string caption = "Error Occured";
                string message = "Process failed! \nSave and restart ArcGIS Pro and try process again.\n" +
                    "If problem persist, contact your local GIS nerd.";

                //Using the ArcGIS Pro SDK MessageBox class
                MessageBox.Show(message, caption);
            }
            return true;


        }
    }   
}

