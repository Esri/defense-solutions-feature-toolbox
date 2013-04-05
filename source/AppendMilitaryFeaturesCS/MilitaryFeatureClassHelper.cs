using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.Diagnostics;
using ESRI.ArcGIS.esriSystem;

namespace AppendMilitaryFeatures
{
    class MilitaryFeatureClassHelper
    {
        public bool Initialized
        {
            get { return initialized; }
        }
        private bool initialized = false;

        // *2* Different SIC/SIDC Field Names (?/!)
        public const string SIDC_FIELD_NAME1 = "sic";
        public const string SIDC_FIELD_NAME2 = "sidc";

        public const string UNIQUE_ID_FIELD_NAME = "uniquedesignation";
        public const string ECHELON_FIELD = "echelon";
        public const string COUNTRY_FIELD = "countrycode";

        // *2* Different Rule Field Names (?/!)
        public const string RULE_FIELD_NAME1 = "ruleid";
        public const string RULE_FIELD_NAME2 = "symbolrule";

        /// <summary>
        /// Defaults to location relative to application's bin path, set to alternate location if desired 
        /// </summary>
        public string FullWorkspacePath
        {
            get { return System.IO.Path.Combine(workspacePath, workspaceName); }
            set 
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(value);

                string folder = fileInfo.DirectoryName;
                string name   = fileInfo.Name;

                WorkspacePath = folder;
                WorkspaceName = name;
            }
        }

        /// <summary>
        /// Defaults to location relative to application's bin path, set to alternate location if desired 
        /// </summary>
        public string WorkspacePath
        {
            get { return workspacePath; }
            set { workspacePath = value; }
        }
        private string workspacePath = System.Reflection.Assembly.GetExecutingAssembly().Location + @"\..\..\..\..\";

        /// <summary>
        /// Defaults to location relative to application's bin path, set to alternate location if desired 
        /// </summary>
        public string WorkspaceName
        {
            get { return workspaceName; }
            set { workspaceName = value; }
        }
        private string workspaceName = "Default.gdb";

        /// <summary>
        /// The workspace used for all operations. 
        /// Set to null, if a new workspace is desired.
        /// </summary>
        public IFeatureWorkspace Workspace
        {
            get
            {
                if (workspace == null)
                {
                    workspace = getWorkspace();

                    // if it is still null, that is bad, something went wrong
                    if (workspace == null)
                    {
                        Trace.WriteLine("Error getting Workspace");
                    }
                }

                return workspace;
            }

            set
            {
                workspace = value;
            }
        }
        private IFeatureWorkspace workspace = null;

        IRepresentationWorkspaceExtension RepresentationWorkspaceExtension
        {
            get
            {
                if (representationWorkspaceExtension == null)
                {
                    representationWorkspaceExtension = getRepWorkspaceExtension();

                    // if it is still null, that is bad, something went wrong
                    if (representationWorkspaceExtension == null)
                    {
                        Trace.WriteLine("Error getting Workspace Extension");
                    }
                }

                return representationWorkspaceExtension;
            }
        }
        private IRepresentationWorkspaceExtension representationWorkspaceExtension = null;

        public IRepresentationClass GetRepresentationClassForFeatureClass(IFeatureClass featureClass)
        {
            if (RepresentationWorkspaceExtension == null)
                return null;

            IRepresentationClass repClass = null;

            IEnumDatasetName datasetNames = RepresentationWorkspaceExtension.get_FeatureClassRepresentationNames(featureClass);
            datasetNames.Reset();
            IDatasetName dsName;
            while ((dsName = datasetNames.Next()) != null)
            {
                string repName = dsName.Name;
                repClass = RepresentationWorkspaceExtension.OpenRepresentationClass(repName);
                // TODO: only gets first Rep Class set / assumes only one name/set
                break;
            }

            return repClass;
        }

        /// <summary>
        /// Open a Feature Class from a Fully Qualified Name
        /// </summary>
        public IFeatureClass GetFeatureClassByName(string fullPathToFeatureClassName)
        {
            IFeatureClass foundFeatureClass = null;

            try
            {
                // get the Workspace (only works with FGDB's):
                string[] separator = new string[]{".gdb"};
                string workspacePath = fullPathToFeatureClassName.Split(separator, StringSplitOptions.None)[0];
                workspacePath += ".gdb";

                string datasetNameAndFeatureClass = fullPathToFeatureClassName.Split(separator, StringSplitOptions.None)[1];
 
                this.FullWorkspacePath = workspacePath;
                IFeatureWorkspace ws = getWorkspace();

                if (!initialized)
                    return null;

                char[] fileSeparator = new char[]{'\\'};
                string featureDatasetName = datasetNameAndFeatureClass.Split(fileSeparator)[1];
                string featureClassName = datasetNameAndFeatureClass.Split(fileSeparator)[2];

                foundFeatureClass = GetFeatureClassByName(featureDatasetName, featureClassName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return foundFeatureClass;
        }

        /// <summary>
        /// Open a Feature Class from DatasetName, FeatureClass name
        /// Assumes the Workspace has been proeviously set
        /// </summary>
        public IFeatureClass GetFeatureClassByName(string featureDatasetName, string featureClassName)
        {
            IFeatureClass foundFeatureClass = null;

            if (!initialized)
                return null;

            try
            {
                if (Workspace == null)
                    return null;

                IFeatureDataset inputFeatureDataset = Workspace.OpenFeatureDataset(featureDatasetName);

                if (inputFeatureDataset == null)
                    return null;

                IEnumDataset eds = inputFeatureDataset.Subsets;
                eds.Reset();
                IDataset ds;
                while ((ds = eds.Next()) != null)
                {
                    if (ds.Name == featureClassName)
                    {
                        IFeatureClass inputFeatureClass = ds as IFeatureClass;
                        if (inputFeatureClass != null)
                        {
                            foundFeatureClass = inputFeatureClass;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return foundFeatureClass;
        }

        public bool IsFeatureClassLockable(IFeatureClass checkFeatureClass)
        {
            IObjectClass objectClass = checkFeatureClass as IObjectClass;
            if (objectClass == null)
                return false;

            ISchemaLock schemaLock = (ISchemaLock)objectClass;
            if (schemaLock == null)
                return false;

            // Get an enumerator over the current schema locks.
            IEnumSchemaLockInfo enumSchemaLockInfo = null;
            schemaLock.GetCurrentSchemaLocks(out enumSchemaLockInfo);

            // Iterate through the locks.
            ISchemaLockInfo schemaLockInfo = null;
            int lockCount = 0;
            while ((schemaLockInfo = enumSchemaLockInfo.Next()) != null)
            {
                lockCount++;
                Trace.WriteLine(string.Format("{0} : {1} : {2}", schemaLockInfo.TableName,
                    schemaLockInfo.UserName, schemaLockInfo.SchemaLockType));
            }

            // Note: 1 sharedLock for this process is normal, so we test for > 1 lock
            return (lockCount < 2);
        }

        public bool ChangeSchemaLockFeatureClass(IFeatureClass checkFeatureClass, bool exclusiveLock)
        {
            bool success = false;

            try
            {
                IObjectClass objectClass = checkFeatureClass as IObjectClass;
                if (objectClass == null)
                    return false;

                ISchemaLock schemaLock = (ISchemaLock)objectClass;
                if (schemaLock == null)
                    return false;

                if (exclusiveLock)
                    schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);
                else
                    schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);

                success = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return success;
        }

        private IFeatureWorkspace getWorkspace()
        {
            IFeatureWorkspace featureWorkspace = null;

            try
            {
                if (!System.IO.Directory.Exists(workspacePath))
                {
                    Trace.WriteLine("Could not open workspace, path doesn't exist: " + workspacePath);
                    return featureWorkspace; // = null
                }

                string fgdbPath = workspacePath;
                string fgdbName = workspaceName;

                string fullWorkspacePath = System.IO.Path.Combine(workspacePath, workspaceName);

                if (!System.IO.Directory.Exists(fullWorkspacePath))
                {
                    Trace.WriteLine("Could not open workspace, file doesn't exist: " + workspaceName);
                    return featureWorkspace; // = null
                }
                else
                {
                    IWorkspaceFactory workspaceFactory = System.Activator.CreateInstance(System.Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory")) as IWorkspaceFactory;
                    featureWorkspace = workspaceFactory.OpenFromFile(fullWorkspacePath, 0) as IFeatureWorkspace;
                    initialized = true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                featureWorkspace = null;
            }

            return featureWorkspace;
        }

        private IRepresentationWorkspaceExtension getRepWorkspaceExtension()
        {
            IFeatureWorkspace ws = getWorkspace();
            IWorkspaceExtensionManager extManager;
            extManager = ws as IWorkspaceExtensionManager;
            UID pUID = new UID();
            pUID.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            return extManager.FindExtension(pUID) as IRepresentationWorkspaceExtension;
        }

    }
}
