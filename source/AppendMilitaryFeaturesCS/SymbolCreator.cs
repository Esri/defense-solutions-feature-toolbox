using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;

namespace AppendMilitaryFeatures
{
    public class SymbolCreator
    {
        public SymbolCreator()
        {
            // build dictionary for symbol ID code position 11
            Position11ToMarkerSymbolNames.Add("A", new List<string>() { "Headquarters" });
            Position11ToMarkerSymbolNames.Add("B", new List<string>() { "Headquarters","Task Force" });
            Position11ToMarkerSymbolNames.Add("C", new List<string>() { "Headquarters","Feint/Dummy" });
            Position11ToMarkerSymbolNames.Add("D", new List<string>() { "Headquarters","Feint/Dummy","Task Force" });
            Position11ToMarkerSymbolNames.Add("E", new List<string>() { "Task Force" });
            Position11ToMarkerSymbolNames.Add("F", new List<string>() { "Feint/Dummy" });
            Position11ToMarkerSymbolNames.Add("G", new List<string>() { "Feint/Dummy", "Task Force" });
            Position11ToMarkerSymbolNames.Add("H", new List<string>() { "Installation" });
        }

        public bool Initialized
        {
            get { return initialized; }
        }
        bool initialized = false;

        private Random r = new Random();
        public string GetRandomSIC()
        {
            return SIC2SymbolNameDictionary.ElementAt(r.Next(SIC2SymbolNameDictionary.Keys.Count - 1)).Key;
        }

        /// <summary>
        /// The Initialize method will load the style files and lookup tables
        /// </summary>
        public void Initialize()
        {
           if (!initialized)
                LoadStyleFiles();

           initialized = true;
        }

        public string GetRuleNameFromSidc(string sidc)
        {
            string baseName = GetGenericSymbolName(sidc);
            if (string.IsNullOrEmpty(baseName) || (sidc.Length < 10))
                return string.Empty;

            // Tactical Graphics / METOC don't care about the rest
            if ((sidc[0] == 'G') || (sidc[0] == 'W'))
                return baseName;

            StringBuilder buildName = new StringBuilder(baseName);

            // TODO: Damaged/Destroyed

            if (HasValidEchelon(sidc))
            {
                string echelonModifierName = GetModifierMarkerSymbolName(sidc);
                if (!string.IsNullOrEmpty(echelonModifierName))
                    buildName.Append("/" + echelonModifierName);
            }

            if (sidc[0] != 'E') // none of these for EMS
            {
                // position 11
                if (HasValidPosition11(sidc))
                {
                    var list = GetPosition11MarkerSymbolNames(sidc);
                    if (list != null && list.Count > 0)
                    {
                        foreach (var name in list)
                        {
                            if (!string.IsNullOrEmpty(name))
                                buildName.Append("/" + name);
                        }
                    }
                }

                // has both position 11 and 12
                if (HasValidPosition11and12(sidc))
                {
                    string pos11and12ModifierName = GetPosition11and12MarkerSymbolName(sidc);
                    if (!string.IsNullOrEmpty(pos11and12ModifierName))
                        buildName.Append("/" + pos11and12ModifierName);
                }
            }

            return buildName.ToString();
        }

        /// <summary>
        /// this method will create the multi layer marker symbol with the generic marker symbol and modifier marker symbols form the Symbol ID Code
        /// </summary>
        /// <param name="sic"></param>
        /// <returns></returns>
        public IMarkerSymbol GetMarkerSymbolFromSIC(string sic)
        {
            if (string.IsNullOrEmpty(sic))
                return null;

            IMarkerSymbol result = null;
            IMarkerSymbol ms = GetGenericMarkerSymbolFromSIC(sic);

            if ((sic[0] == 'G') || (sic[0] == 'W'))
                return ms;

            // create the graphic element for the marker symbol and add it to the map
            if (ms != null)
            {
                // workaround for the Marker symbol problem where the symbol size randomly loads at size 8 instead of 25
                if (ms.Size < 25)
                {
                    ms.Size = 25;
                }
                IMultiLayerMarkerSymbol mlms = new MultiLayerMarkerSymbolClass();
                mlms.ClearLayers();

                // workaround for the Marker symbol problem where the symbol size randomly loads at size 8 instead of 25
                //if (mlms.Size != 25)
                //{
                //    mlms.Size = 25;
                //}

                // moved the addition of generic frame symbol to the end so that it draws on top

                // Position 4 if it is D, C, X, or F
                if (IsValidSicBase(sic, @"^.{3}[DCXF].{11}$"))
                {
                    IMarkerSymbol StrengthSymbol = null;
                    string strengthName = "";

                    // get the name of the symbol
                    switch (sic[3])
                    {
                        case 'D':
                            strengthName = "Damaged";
                            break;
                        case 'C':
                            strengthName = "Fully Capable";
                            break;
                        case 'X':
                            strengthName = "Destroyed";
                            break;
                        case 'F':
                            strengthName = "Full to Capacity";
                            break;
                        default:
                            break;
                    }

                    // get the marker symbol for that name
                    if (!String.IsNullOrEmpty(strengthName))
                    {
                        StrengthSymbol = GetMarkerSymbolByName(strengthName);

                        if (StrengthSymbol != null)
                        {
                            if (!AddSymbolToMultiLayerSymbol(mlms, StrengthSymbol))
                            {
                                LogError(String.Format("Error while adding strength modifier layer to MultiLayerMarkerSymbol, SIC : {0}", sic));
                            }
                        }
                    }
                }

                // echelon
                if (HasValidEchelon(sic))
                {
                    IMarkerSymbol EchelonModifierSymbol = GetMarkerSymbolByName(GetModifierMarkerSymbolName(sic));
                    if (EchelonModifierSymbol != null)
                    {
                        if (!AddSymbolToMultiLayerSymbol(mlms, EchelonModifierSymbol))
                        {
                            LogError(String.Format("Error while adding echelon layer to MultiLayerMarkerSymbol, SIC : {0}", sic));
                        }
                    }
                }

                // position 11

                if (HasValidPosition11(sic))
                {
                    var list = GetPosition11MarkerSymbolNames(sic);
                    if (list != null && list.Count > 0)
                    {
                        foreach (var name in list)
                        {
                            IMarkerSymbol s = GetMarkerSymbolByName(name);
                            if (s != null)
                            {
                                if (!AddSymbolToMultiLayerSymbol(mlms, s))
                                {
                                    LogError(String.Format("Error while adding pos 11 modifier layer to MultiLayerMarkerSymbol, SIC : {0}", sic));
                                }
                            }
                        }
                    }
                }

                // position 11 and 12

                if (HasValidPosition11and12(sic))
                {
                    IMarkerSymbol s = GetMarkerSymbolByName(GetPosition11and12MarkerSymbolName(sic));
                    if (s != null)
                    {
                        if (!AddSymbolToMultiLayerSymbol(mlms, s))
                        {
                            LogError(String.Format("Error while adding pos 11/12 modifier layer to MultiLayerMarkerSymbol, SIC : {0}", sic));
                        }
                    }
                }

                // add generic frame marker symbol
                if (!AddSymbolToMultiLayerSymbol(mlms, ms))
                {
                    LogError(String.Format("Error when adding generic SIC marker symbol to the main multilayer marker symbol, {0}", sic));
                }

                IMapLevel mapLevel = mlms as IMapLevel;
                IMapLevel levelma = ms as IMapLevel;

                // is symbol ID code anticipated/planned (A is position 4) or assumed friend, exercise, suspect, pending, etc (in position 2)
                if (IsValidSicBase(sic, "^.[PASGM].{13}$") || IsValidSicBase(sic, "^.{3}[A].{11}$"))
                {
                    // load dashed frame and add to layer

                    string dashedSIC = "";

                    if (IsValidSicBase(sic, @".{4}[EIU].{10}") == true)
                    {
                        dashedSIC += sic.Substring(0, 5);
                    }
                    else
                    {
                        dashedSIC += sic.Substring(0, 4) + "-";
                    }

                    dashedSIC += "----------";

                    if (SIC2SymbolNameDictionary.ContainsKey(dashedSIC))
                    {
                        IMarkerSymbol dashedms = GetMarkerSymbolByName(SIC2SymbolNameDictionary[dashedSIC]);

                        if (!AddSymbolToMultiLayerSymbol(mlms, dashedms))
                        {
                            LogError(String.Format("Error while adding dashed layer to MultiLayerMarkerSymbol, SIC : {0}", dashedSIC));
                        }
                    }
                    else
                    {
                        LogError(String.Format("Error, SIC to SymbolName dictionary does not contain key, {0}", dashedSIC));
                    }
                }

                result = mlms as IMarkerSymbol;
            }
            else
            {
                LogError(String.Format("Generic MarkerSymbol returned null when loading for sic : {0}", sic));
            }

            return result;
        }
        
        /// <summary>
        /// Property that provides the current symbol count
        /// </summary>
        public int SymbolCount
        {
            get
            {
                return ID2StyleGalleryItemDictionary.Count; 
            }
        }

        /// <summary>
        /// this method will only load the base/generic marker symbol from the Symbol ID Code
        /// </summary>
        /// <param name="sic"></param>
        /// <returns></returns>
        public IMarkerSymbol GetGenericMarkerSymbolFromSIC(string sic)
        {
            string genericName = GetGenericSymbolName(sic);

            return GetMarkerSymbolByName(genericName);
        }
        /// <summary>
        /// Method provides a way to get the Representation Rule ID from the symbol ID code
        /// </summary>
        /// <param name="sic"></param>
        /// <returns></returns>
        public int GetRuleIDFromSIC(string sic)
        {
            if (!IsValidSic(sic))
            {
                return 0;
            }

            string maskedSIC = GetMaskedTGSIC(sic);
            if (!SIC2RuleID.ContainsKey(maskedSIC))
            {
                LogError(String.Format("Error when looking for tactical graphic RuleID for SIC, {0}", sic));
                return 0;
            }

            return SIC2RuleID[GetMaskedTGSIC(sic)];
        }
        /// <summary>
        /// Method masks out a symbol ID code if needed for proper lookup
        /// </summary>
        /// <param name="sic"></param>
        /// <returns></returns>
        private string GetMaskedTGSIC(string sic)
        {
            // mask if needed
            string maskedsic = "";
            maskedsic += sic[0];
            if (sic[0] == 'G')
            {
                maskedsic += "-";
                maskedsic += sic.Substring(2, 8);
                maskedsic += "----X";
            }
            else if (sic[0] == 'W')
            {
                maskedsic += sic.Substring(1, 1);
                maskedsic += "--";
                maskedsic += sic.Substring(4, 11);
            }
            else
            {
                maskedsic += sic.Substring(1, 14);
            }

            return maskedsic.Replace('*','-');
        }

        /// <summary>
        /// This method will return the Marker Symbol Name from a symbol ID code
        /// </summary>
        /// <param name="sic"></param>
        /// <returns></returns>
        public string GetGenericSymbolName(string sic)
        {
            try
            {
                if (sic.Length < 10)
                    return string.Empty;

                // added to support the old standard installations symbol ID code
                string temp = (sic.Substring(0, 10) + ((IsValidSicInstallation(sic) == true) ? "H----" : "-----")).ToUpper();
                StringBuilder sb;
                if (temp[0] == 'G')
                {
                    // Style file Tactical Graphics have multiple conventions: 1:"G-XX", 2:"G<Afilliation>XX", 3:"GFXX"
                    // must check for all 3

                    // 1: "-" for affiliation
                    sb = new StringBuilder(GetMaskedTGSIC(temp));

                    // 2: use affilation
                    if (!SIC2SymbolNameDictionary.ContainsKey(sb.ToString()))
                        sb[1] = temp[1];

                    // 3: use "F" affilitation (some markers only have "F" version)
                    if (!SIC2SymbolNameDictionary.ContainsKey(sb.ToString()))
                        sb[1] = 'F';
                }
                else
                {
                    // need to check for assumed friend and anticipated/planned, change to load the generic frame as if it were friendly and/or present

                    sb = new StringBuilder(temp);

                    sb.Remove(1, 1);

                    if (temp[0] == 'E' && temp[2] == 'N') // Natural Event special case, second pos needs to be a '-'
                    {
                        sb.Insert(1, "-");
                    }
                    else
                    {
                        switch (temp[1])
                        {
                            case 'P':
                            case 'G':
                            case 'W':
                                sb.Insert(1, "U");
                                break;
                            case 'A':
                            case 'D':
                            case 'M':
                                sb.Insert(1, "F");
                                break;
                            case 'S':
                                sb.Insert(1, "H");
                                break;
                            case 'L':
                                sb.Insert(1, "N");
                                break;
                            default:
                                sb.Insert(1, temp[1]);
                                break;
                        }
                    }

                    // Force pos 4 to P (if not 'W'/METOC)
                    if (temp[3] != 'W')
                    {
                        sb.Remove(3, 1);
                        sb.Insert(3, "P");
                    }
                }

                if (!SIC2SymbolNameDictionary.ContainsKey(sb.ToString()))
                {
                    LogError(String.Format("Error, SIC to SymbolName dictionary does not contain a key for {0}", sb.ToString()));
                    return "";
                }

                return SIC2SymbolNameDictionary[sb.ToString()]; 
            }
            catch(Exception ex)
            {
                LogError(String.Format("Error on lookup for symbol ID code, ",sic));
                LogError(ex.ToString());
                return "";
            }
        }

        /// <summary>
        /// This method takes a marker symbol and adds it to the multilayer marker symbol, checks to make sure it's not a multilayer symbol first
        /// </summary>
        /// <param name="mlms"></param>
        /// <param name="ms"></param>
        /// <returns></returns>
        private bool AddSymbolToMultiLayerSymbol(IMultiLayerMarkerSymbol mlms, IMarkerSymbol ms)
        {
            ((ISymbol)ms).ResetDC();

            IMultiLayerMarkerSymbol tempmlms = ms as IMultiLayerMarkerSymbol;

            try
            {
                if (tempmlms != null && tempmlms.LayerCount > 0)
                {
                    for (int layerindex = 0; layerindex < tempmlms.LayerCount; layerindex++)
                    {
                        mlms.AddLayer(tempmlms.get_Layer(layerindex));
                    }
                }
                else
                {
                    mlms.AddLayer(ms);
                }
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// This mehtod will get the modifer marker symbol name from a symbol ID code
        /// </summary>
        /// <param name="sic"></param>
        /// <returns></returns>
        public int GetEchelonOrdinal(string sic)
        {
            string temp = sic.ToUpper();

            if (System.Text.RegularExpressions.Regex.IsMatch(temp, @"[SO][A-Z0-9\-]{10}[A-N][A-Z0-9\-]{3}"))
            {
                return Echelon2Ordinal[temp[11].ToString()];
            }

            return 0;
        }

        public string GetCountryCode(string sic)
        {
            if (sic.Length < 14)
                return "--";

            char cc1 = sic[12];
            char cc2 = sic[13];

            return cc1.ToString() + cc2.ToString();
        }

        /// <summary>
        /// This mehtod will get the modifer marker symbol name from a symbol ID code
        /// </summary>
        /// <param name="sic"></param>
        /// <returns></returns>
        private string GetModifierMarkerSymbolName(string sic)
        {
            // do expression, then dictionary lookup

            string temp = sic.ToUpper();

            if (System.Text.RegularExpressions.Regex.IsMatch(temp, @"[SO][A-Z0-9\-]{10}[A-N][A-Z0-9\-]{3}"))
            {
                return Echelon2MarkerSymbolName[temp[11].ToString()];
            }

            return "Not defined";
        }

        private List<string> GetPosition11MarkerSymbolNames(string sic)
        {
            if (HasValidPosition11(sic))
            {
                // we added other symbols (HQ) that need another check here first before the generic modifers are returned

                if (IsValidSicBase(sic, @"[SO].{9}[B-D].{4}") == true)
                {
                    string temp = "";

                    // pos 1-3
                    if (sic[0] == 'S')
                    {
                        temp += sic.Substring(0, 3);
                    }
                    else
                    {
                        temp += sic.Substring(0, 2) + "-";
                    }

                    // pos 4
                    temp += "-";

                    // pos 5
                    if (IsValidSicBase(sic, @".{4}[EIU].{10}") == true)
                    {
                        temp += sic[4];
                    }
                    else
                    {
                        temp += "-";
                    }

                    // pos 6-10, 11, 12-15
                    temp += "-----" + sic[10] + "----";

                    if (!SIC2SymbolNameDictionary.ContainsKey(temp))
                    {
                        LogError(String.Format("Error, SIC to symbol dictionary does not contain key, {0}", temp));
                        return new List<string>();
                    }

                    return new List<string>() { SIC2SymbolNameDictionary[temp] };
                }

                // return the list of modifiers based on position 11 only
                return Position11ToMarkerSymbolNames[sic[10].ToString().ToUpper()];
            }

            return null;
        }

        private string GetPosition11and12MarkerSymbolName(string sic)
        {
            if (HasValidPosition11and12(sic))
            {
                return Position11and12ToMarkerSymbolName[sic[11].ToString().ToUpper()];
            }

            return "Not defined";
        }

        public bool HasValidEchelon(string sic)
        {
            return IsValidSicBase(sic, @".{11}[A-N].{3}");
        }

        private bool HasValidPosition11(string sic)
        {
            return IsValidSicBase(sic, @".{10}[A-H].{4}");
        }

        private bool HasValidPosition11and12(string sic)
        {
            return IsValidSicBase(sic, @".{10}[M][O-Y].{3}");
        }

        /// <summary>
        /// This method will load a Marker Symbol from the currently loaded style files by the symbol name
        /// </summary>
        /// <param name="markerSymbolName"></param>
        /// <returns></returns>
        public IMarkerSymbol GetMarkerSymbolByName(string markerSymbolName)
        {
            IMarkerSymbol ms = null;

            if (ID2StyleGalleryItemDictionary.ContainsKey(markerSymbolName))
            {
                ms = ID2StyleGalleryItemDictionary[markerSymbolName].Item as IMarkerSymbol;
            }
            else
            {
                LogError(String.Format("Marker symbol name, {0}, not found in symbol name 2 marker symbol dictionary!",markerSymbolName));
            }

            return ms;
        }

        #region Symbol ID code validation methods

        private const string ValidSicExpression = @"^[SGWIOE][PUAFNSHGWMDLJKO\-][PAGSUFXTMOEVLIRNZ\-][APCDXF\-][A-Z0-9\-]{6}[A-Z\-]{2}[A-Z0-9\-]{2}[AECGNSX\-]$";

        // simple SIC checker
        public bool IsValidSic(string sicString)
        {
            return IsValidSicBase(sicString, @"^.{15}$");
        }

        public bool IsValidSicFriendly(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[F].{13}$");
        }
        public bool IsValidSicHostile(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[H].{13}$");
        }
        public bool IsValidSicNeutral(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[N].{13}$");
        }
        public bool IsValidSicUnknown(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[U].{13}$");
        }
        public bool IsValidSicPending(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[P].{13}$");
        }
        public bool IsValidSicAssumedFriend(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[A].{13}$");
        }
        public bool IsValidSicSuspect(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[S].{13}$");
        }
        public bool IsValidSicExercisePending(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[G].{13}$");
        }
        public bool IsValidSicExerciseUnknown(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[W].{13}$");
        }
        public bool IsValidSicExerciseAssumedFriend(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[M].{13}$");
        }
        public bool IsValidSicExerciseFriend(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[D].{13}$");
        }
        public bool IsValidSicExerciseNeutral(string sicString)
        {
            return IsValidSicBase(sicString, @"^.[L].{13}$");
        }

        public bool IsValidSicUnit(string sicString)
        {
            return IsValidSicBase(sicString, @"^[S][FHNUPASGWMDL][AFGPSUXZ][APCDXF](?(?<=..G.)(U)|(.)).{10}$");
        }

        public bool IsValidSicEquipment(string sicString)
        {
            return IsValidSicBase(sicString, @"^[S][FHNUPASGWMDL][PAGSU][APCDXF](?(?<=..G.)(E)|(.)).{10}$");
        }

        public bool IsValidSicInstallation(string sicString)
        {
            return IsValidSicBase(sicString, @"^[S].[G].[I].{10}$");
        }
        public bool IsValidSicMETOCPoint(string sicString)
        {
            return IsValidSicBase(sicString, @"^[W].{9}[P][-][-].{2}$");
        }
        public bool IsValidSicMETOCLine(string sicString)
        {
            return IsValidSicBase(sicString, @"^[W].{9}[-][L][-].{2}$");
        }
        public bool IsValidSicMETOCArea(string sicString)
        {
            return IsValidSicBase(sicString, @"^[W].{9}[-][-][A].{2}$");
        }

        public bool IsValidSicSIGINT(string sicString)
        {
            return IsValidSicBase(sicString, @"^[I].{14}$");
        }

        public bool IsValidSicMOPoint(string sicString)
        {
            if (!IsValidSic(sicString))
            {
                return false;
            }
            // position 3 is T
            if (IsValidSicBase(sicString, @"^[G].[T][ASPK][DIN].{10}$")) 
            {
                return true;
            }

            // position 3 is G

            if (IsValidSicBase(sicString, @"^[G].[G][ASPK][GADOS][P].{9}$"))  
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[G][ASPK][P][N].{9}$"))  
            {
                return true;
            }

            // position 3 is M
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][O][A][O].{8}$"))  
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][O][BM].{9}$"))  
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][O][F][S].{8}$"))  
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][S][EFSU].{9}$"))  
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][N][ZFED].{9}$"))  
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK].[H][T].{8}$"))  
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK].[C][P].{8}$"))  
            {
                return true;
            }
            // position 3 is F
            if (IsValidSicBase(sicString, @"^[G].[F][ASPK][P].{10}$"))  
            {
                return true;
            }
            // position 3 is S
            if (IsValidSicBase(sicString, @"^[G].[S][ASPK][P].{10}$"))  
            {
                return true;
            }
            // position 3 is O
            if (IsValidSicBase(sicString, @"^[G].[O][ASPK][ESF].{10}$"))  
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[O][ASPK][H][MIO].{9}$"))  
            {
                return true;
            }

            return false;
        }
        public bool IsValidSicMOLine(string sicString)
        {
            if (!IsValidSic(sicString))
            {
                return false;
            }
            // position 3 is T
            if (IsValidSicBase(sicString, @"^[G].[T][ASPK][^DIN].{10}$")) 
            {
                return true;
            }
            // position 3 is G
            if (IsValidSicBase(sicString, @"^[G].[G][ASPK][GADOS][L].{9}$")) 
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[G][ASPK][P][DAF].{9}$")) 
            {
                return true;
            }
            // position 3 is M
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][O][G][L].{8}$")) 
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][O][A][DRW].{8}$")) 
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][O][SEWTR].{9}$")) 
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][O][F][G].{8}$")) 
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK].[H][O].{8}$")) 
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][S][LW].{9}$")) 
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK].[C][ABFEDLR].{8}$")) 
            {
                return true;
            }
            // position 3 is F
            if (IsValidSicBase(sicString, @"^[G].[F][ASPK][L].{10}$")) 
            {
                return true;
            }
            // position 3 is S
            if (IsValidSicBase(sicString, @"^[G].[S][ASPK][L].{10}$")) 
            {
                return true;
            }
            // position 3 is O
            if (IsValidSicBase(sicString, @"^[G].[O][ASPK][B].{10}$")) 
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[O][ASPK][H][N].{9}$")) 
            {
                return true;
            }

            return false;
        }
        public bool IsValidSicMOArea(string sicString)
        {
            if (!IsValidSic(sicString))
            {
                return false;
            }
            // position 3 is G
            if (IsValidSicBase(sicString, @"^[G].[G][ASPK][GADOS][A].{9}$"))
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[G][ASPK][P][MYN].{9}$"))
            {
                return true;
            }
            // position 3 is M
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][O][G][BZFR].{8}$"))
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][O][U].{9}$"))
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][O][F][DA].{8}$"))
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][S][P].{9}$"))
            {
                return true;
            }
            if (IsValidSicBase(sicString, @"^[G].[M][ASPK][N][RBCL].{9}$"))
            {
                return true;
            }
            // position 3 is F
            if (IsValidSicBase(sicString, @"^[G].[F][ASPK][A].{10}$"))
            {
                return true;
            }
            // position 3 is S
            if (IsValidSicBase(sicString, @"^[G].[S][ASPK][A].{10}$"))
            {
                return true;
            }

            return false;
        }

        public bool IsValidSicSOViolentActivities(string sicString)
        {
            return IsValidSicBase(sicString, @"^[O].[V].{12}$");
        }
        public bool IsValidSicSOLocations(string sicString)
        {
            return IsValidSicBase(sicString, @"^[O].[L].{12}$");
        }
        public bool IsValidSicSOOperations(string sicString)
        {
            return IsValidSicBase(sicString, @"^[O].[O].{12}$");
        }
        public bool IsValidSicSOItems(string sicString)
        {
            return IsValidSicBase(sicString, @"^[O].[I].{12}$");
        }
        public bool IsValidSicSOIndividual(string sicString)
        {
            return IsValidSicBase(sicString, @"^[O].[P].{12}$");
        }
        public bool IsValidSicSONon(string sicString)
        {
            return IsValidSicBase(sicString, @"^[O].[G].{12}$");
        }
        public bool IsValidSicSORape(string sicString)
        {
            return IsValidSicBase(sicString, @"^[O].[R].{12}$");
        }

        public bool IsValidSicEMIncident(string sicString)
        {
            return IsValidSicBase(sicString, @"^[E].[I].{12}$");
        }
        public bool IsValidSicEMNaturalEvents(string sicString)
        {
            return IsValidSicBase(sicString, @"^[E].[N].{12}$");
        }
        public bool IsValidSicEMOperations(string sicString)
        {
            return IsValidSicBase(sicString, @"^[E].[O].{12}$");
        }
        public bool IsValidSicEMInfrastructure(string sicString)
        {
            return IsValidSicBase(sicString, @"^[E].[F].{12}$");
        }

        private bool IsValidSicBase(string sicString, string expression)
        {
            if ((String.IsNullOrEmpty(sicString)) || (sicString.Length != 15) || String.IsNullOrEmpty(expression))
            {
                return false;
            }
            else
            {
                string temp = sicString.ToUpper();

                return (System.Text.RegularExpressions.Regex.IsMatch(temp, expression) &&
                    System.Text.RegularExpressions.Regex.IsMatch(temp, ValidSicExpression));
            }
        }

        #endregion Symbol ID code validation methods

        private void ProcessStyleGalleryItems(IStyleGallery styleGallery, string styleClass)
        {
            if (styleGallery == null)
            {
                Console.WriteLine("ERROR: Style Gallery NULL");
                return;
            }

            IEnumStyleGalleryItem ge = styleGallery.get_Items(styleClass, "", "");

            if (ge == null)
            {
                Console.WriteLine("ERROR: No Items for Style Class:" + styleClass);
                return;
            }

            int count = 0;
            IStyleGalleryItem item = null;
            while ((item = ge.Next()) != null)
            {
                try
                {
                    if (!ID2StyleGalleryItemDictionary.ContainsKey(item.Name))
                    {
                        // SKIP APP6's for now
                        if (item.Name.Contains("APP6"))
                            continue;

                        IStyleGalleryItem2 item2 = item as IStyleGalleryItem2;
                        if (item2 != null)
                        {
                            // split the tags into list
                            var tagList = item2.Tags.Split(new char[] { ';' });

                            // reverse the list so we can easily locate the symbol ID code tag
                            var tagListReversed = tagList.Reverse();

                            // IMPORTANT: assumes SIC is always first in reversed list (last in original list)
                            string lastTag = tagListReversed.ElementAt(0);
                            string sidc = lastTag.ToUpper().Trim().Replace('*', '-');

                            if (!string.IsNullOrEmpty(sidc)) // (sidc))
                            {
                                if (!SIC2SymbolNameDictionary.ContainsKey(sidc))
                                {
                                    string itemName = item.Name;
                                    // if (styleClass.StartsWith("Rep"))
                                    if ((sidc[0] == 'G') || (sidc[0] == 'W'))
                                    {
                                        int len = itemName.Length;
                                        if ((len > 2) && (itemName.EndsWith("F") 
                                            || itemName.EndsWith("H") || itemName.EndsWith("N") 
                                            || itemName.EndsWith("U")))
                                            itemName = itemName.Remove(len - 2);
                                    }

                                    count++;
                                    System.Diagnostics.Trace.WriteLine("#" + count + ", Item Name: " + itemName + ", SIDC: " + sidc);
                                    if (!ID2StyleGalleryItemDictionary.ContainsKey(itemName))
                                        ID2StyleGalleryItemDictionary.Add(itemName, item);
                                    SIC2SymbolNameDictionary.Add(sidc, itemName);
                                }
                            }
                            else
                            {
                                LogError(String.Format("*** Failed to add a valid SIC, {0}, {1}, {2}, {3}, {4}", sidc.Length, sidc, IsValidSic(sidc) ? "valid" : "INVALID", SIC2SymbolNameDictionary.ContainsKey(sidc) ? "DUPLICATE" : "New", item.Name));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(String.Format("Error loading marker symbol from style item, {0}", item.Name));
                    LogError(ex.ToString());
                    Console.WriteLine(ex);
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(ge);
            ge = null;

        }

        /// <summary>
        /// This method loads the configured style files
        /// </summary>
        private void LoadStyleFiles()
        {
            IStyleGallery styleGallery = new StyleGalleryClass();
            IStyleGalleryStorage styleGalleryStorage = styleGallery as IStyleGalleryStorage;

            try
            {
                if (styleGalleryStorage != null)
                {
                    //Console.WriteLine(String.Format("Default folder : {0}, File Count : {1}", StyleGalleryStorage.DefaultStylePath, StyleGalleryStorage.FileCount));

                    //Console.WriteLine(String.Format("Class Count : {0}", StyleGallery.ClassCount));
                    int fileCount = styleGalleryStorage.FileCount;
                    List<string> files = new List<string>();
                    for (int i = 0; i < fileCount; i++)
                    {
                        string file = styleGalleryStorage.get_File(i);
                        files.Add(file);
                        //Console.WriteLine("Adding file: " + file);
                    }

                    foreach (string path in files)
                    {
                        styleGalleryStorage.RemoveFile(path);
                    }

                    // WORKAROUND: looks like doing all styles at once causing issues so doing them one at a time
                    List<string> styleFiles = new List<string>() {"C2 Military Operations.style", "C2 UEI Air Track.style", "C2 UEI Ground Track Equipment.style",
                    "C2 UEI Ground Track Installations.style", "C2 UEI Ground Track Units.style", "C2 UEI Sea Surface Track.style","C2 UEI Space Track.style",
                    "C2 UEI Special Operations Track.style","C2 UEI Subsurface Track.style","Military Emergency Management.style","Military METOC.style",
                    "Signals Intelligence.style","Stability Operations.style"};

                    // Simple Test (for quicker loading)
                    // List<string> styleFiles = new List<string>() {"C2 UEI Ground Track Units.style"};

                    foreach (string styleFile in styleFiles)
                    {
                        string stylepath = styleGalleryStorage.DefaultStylePath + styleFile;
                        // ex: string stylepath = @"C:\Program Files (x86)\ArcGIS\Desktop10.1\Styles\" + styleFile;
                        styleGalleryStorage.AddFile(stylepath);

                        System.Diagnostics.Trace.WriteLine("Style" + styleFile);

                        // IMPORTANT: Note: preloading all the gallery items for faster lookup

                        try
                        {
                            ProcessStyleGalleryItems(styleGallery, "Marker Symbols");
                            ProcessStyleGalleryItems(styleGallery, "Representation Rules");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        styleGalleryStorage.RemoveFile(stylepath);
                    }
                }
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2);
            }

            // release objects
            int refsLeft = 0;
            do
            {
                refsLeft = System.Runtime.InteropServices.Marshal.ReleaseComObject(styleGallery);
            }
            while (refsLeft > 0);

            styleGallery = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void LogError(string msg)
        {
            // Console.WriteLine(msg);

            // Log to file:
            //using (TextWriter w = File.AppendText("AppendMilitaryFeaturesLog.txt"))
            //{
            //    w.WriteLine(msg);                
            //}
        }

        public void Cleanup()
        {
            ID2StyleGalleryItemDictionary.Clear();
            SIC2SymbolNameDictionary.Clear();
        }

        private Dictionary<string, string> SIC2SymbolNameDictionary = new Dictionary<string, string>();
        private Dictionary<string, IStyleGalleryItem> ID2StyleGalleryItemDictionary = new Dictionary<string, IStyleGalleryItem>();
        private Dictionary<string, string> Echelon2MarkerSymbolName = new Dictionary<string, string>() 
            {{"A","Team/Crew"},
            {"B","Squad"},
            {"C","Section"},
            {"D","Platoon/Detachment"},
            {"E","Company/Battery/Troop"},
            {"F","Battalion/Squadron"},
            {"G","Regiment/Group"},
            {"H","Brigade"},
            {"I","Division"},
            {"J","Corps"},
            {"K","Army"},
            {"L","Army Group/Front"},
            {"M","Region"},
            {"N","Command"}};

        private Dictionary<string, int> Echelon2Ordinal = new Dictionary<string, int>() {{"A",0},
                                                                                        {"B",1},
                                                                                        {"C",11},
                                                                                        {"D",111},
                                                                                        {"E",2},
                                                                                        {"F",22},
                                                                                        {"G",222},
                                                                                        {"H",3},
                                                                                        {"I",33},
                                                                                        {"J",333},
                                                                                        {"K",3333},
                                                                                        {"L",33333},
                                                                                        {"M",333333},
                                                                                        {"N",44}};

        private Dictionary<string, string> Position11and12ToMarkerSymbolName = new Dictionary<string, string>()
            {{"O","Wheeled (Limited Cross Country)"},
                {"P","Wheeled (Cross Country)"},
                {"Q","Tracked"},
                {"R","Wheeled and Tracked"},
                {"S","Towed"},
                {"T","Railway"},
                {"U","Over Snow"},
                {"V","Sled"},
                {"W","Pack Animals"},
                {"X","Barge"},
                {"Y","Amphibious"}};

        private Dictionary<string, List<string>> Position11ToMarkerSymbolNames = new Dictionary<string, List<string>>();

        private Dictionary<string, int> SIC2RuleID = new Dictionary<string, int>()
        {
            {"WA--BAIF----A--",1},
            {"WA--BAMV----A--",2},
            {"WA--BATB----A--",3},
            {"WA--BAI-----A--",4},
            {"WA--BALPNC--A--",5},
            {"WA--BALPC---A--",6},
            {"WA--BAFP----A--",7},
            {"WA--BAT-----A--",8},
            {"WA--BAFG----A--",9},
            {"WA--BAD-----A--",10},
            {"WA--BAFF----A--",11},
            {"WO--HDDA----A--",13},
            {"WO--HCI-----A--",14},
            {"WO--HCB-----A--",15},
            {"WO--HCW-----A--",16},
            {"WO--HCF-----A--",17},
            {"WO--HPBA----A--",18},
            {"WO--HPFF----A--",19},
            {"WO--HPMD----A--",20},
            {"WO--HPMO----A--",21},
            {"WO--HABP----A--",22},
            {"WO--HHD-----A--",23},
            {"WO--HHDF----A--",24},
            {"WO--HHDK----A--",25},
            {"WO--HHDD----A--",26},
            {"WO--OBVA----A--",27},
            {"WO--OBVB----A--",28},
            {"WO--OBVC----A--",29},
            {"WO--OBVD----A--",30},
            {"WO--OBVE----A--",31},
            {"WO--OBVF----A--",32},
            {"WO--OBVG----A--",33},
            {"WO--OBVH----A--",34},
            {"WO--OBVI----A--",35},
            {"WO--BSF-----A--",36},
            {"WO--BSG-----A--",37},
            {"WO--BSM-----A--",38},
            {"WO--BST-----A--",39},
            {"WO--GMSR----A--",40},
            {"WO--GMSC----A--",41},
            {"WO--GMSSVS--A--",42},
            {"WO--GMSSC---A--",43},
            {"WO--GMSSM---A--",44},
            {"WO--GMSSF---A--",45},
            {"WO--GMSSVF--A--",46},
            {"WO--GMSIVF--A--",47},
            {"WO--GMSIF---A--",48},
            {"WO--GMSIM---A--",49},
            {"WO--GMSIC---A--",50},
            {"WO--GMSB----A--",51},
            {"WO--GMS-CO--A--",52},
            {"WO--GMS-PH--A--",53},
            {"WO--GMS-SH--A--",54},
            {"WO--GML-----A--",55},
            {"WO--GMN-----A--",56},
            {"WO--GMRS----A--",57},
            {"WO--GMRM----A--",58},
            {"WO--GMRR----A--",59},
            {"WO--GMCL----A--",60},
            {"WO--GMCM----A--",61},
            {"WO--GMCH----A--",62},
            {"WO--GMIBA---A--",63},
            {"WO--GMIBB---A--",64},
            {"WO--GMIBC---A--",65},
            {"WO--GMIBD---A--",66},
            {"WO--GMIBE---A--",67},
            {"WO--GMBCA---A--",68},
            {"WO--GMBCB---A--",69},
            {"WO--GMBCC---A--",70},
            {"WO--GMBTA---A--",71},
            {"WO--GMBTB---A--",72},
            {"WO--GMBTC---A--",73},
            {"WO--GMBTD---A--",74},
            {"WO--GMBTE---A--",75},
            {"WO--GMBTF---A--",76},
            {"WO--GMBTG---A--",77},
            {"WO--GMBTH---A--",78},
            {"WO--GMBTI---A--",79},
            {"WO--L-MA----A--",80},
            {"WO--L-SA----A--",81},
            {"WO--L-TA----A--",82},
            {"WO--L-O-----A--",83},
            {"WO--MCC-----A--",84},
            {"WO--MOA-----A--",85},
            {"G-GPGAG-------X",13}, // General 
            {"G-GPGAA-------X",10}, // Assembly
            {"G-GPGAE-------X",12}, // Engagement
            {"G-GPGAF-------X",51}, // Fortified
            {"G-GPGAD-------X",11}, // Drop Zone
            {"G-GPGAX-------X",50}, // Extraction Zone (EZ)
            {"G-GPGAL-------X",22}, // Landing Zone (LZ)
            {"G-GPGAP-------X",18}, // Pickup Zone (PZ)
            {"G-GPGAY-------X",52}, // Limited Access Area
            {"G-GPGAZ-------X",53}, // Airfield Zone
            {"G-GPAAR-------X",19}, // Restricted Operations Zone (ROZ)
            {"G-GPAAF-------X",20}, // Short-range air Defense Engagement zone
            {"G-GPAAH-------X",14}, // High Density airspace control zone
            {"G-GPAAM-------X",15}, // Missile Engagement Zone
            {"G-GPAAML------X",17}, // Missile Engagement Zone, low altitude mez
            {"G-GPAAMH------X",16}, // Missile Engagement Zone, high altitude mez
            {"G-GPAAW-------X",21}, // Weapons Free Zone
            {"G-GPPM--------X",77}, // Decoy Mined Area
            {"G-GPPY--------X",78}, // Decoy Mined Area, Fenced
            {"G-GPPC--------X",79}, // Dummy Minefield (Dynamic)
            {"G-GPDAB-------X",9}, // Battle Position
            {"G-GPDABP------X",81}, // Battle Position, prepared but not occupied
            {"G-GPDAE-------X",12}, // Engagement Area
            {"G-GPOAA-------X",5}, // Assault Position
            {"G-GPOAK-------X",6}, // Attack Position
            {"G-GPOAO-------X",7}, // Objective
            {"G-GPOAP-------X",73}, // Penetration Box
            {"G-GPSAO-------X",74}, // Area of Operations
            {"G-GPSAA-------X",75}, // Airhead
            {"G-GPSAE-------X",54}, // Encirclement
            {"G-GPSAN-------X",25}, // Named area of interest
            {"G-GPSAT-------X",26}, // Targeted area of interest
            {"G-MPOGB-------X",38}, // Belt
            {"G-MPOGZ-------X",39}, // General obstacle zone
            {"G-MPOGF-------X",76}, // Obstacle free area
            {"G-MPOGR-------X",82}, // Obstacle restricted area
            {"G-MPOFD-------X",83}, // mine field Dynamic Depiction
            {"G-MPOFA-------X",84}, // Mined Area
            {"G-MPOU--------X",40}, // Unexploded ordnance area
            {"G-MPSP--------X",24}, // Strong point
            {"G-MPNR--------X",43}, // Radioactive area
            {"G-MPNB--------X",41}, // Radiological and nuclear BIO contaminated area
            {"G-MPNC--------X",42}, // Radiological and nuclear CML contaminated area
            {"G-MPNL--------X",85}, // Dose rate contour lines
            {"G-FPAT--------X",30}, // fire support, area target
            {"G-FPATG-------X",31}, // area target, series or group of targets
            {"G-FPATS-------X",58}, // smoke
            {"G-FPATB-------X",59}, // bomb area
            {"G-FPACAI------X",27}, // airspace coordination area
            {"G-FPACSI------X",60}, // fire support area, irregular
            {"G-FPACFI------X",28}, // free fire
            {"G-FPACNI------X",29}, // no fire area
            {"G-FPACRI------X",33}, // restrictive fire area
            {"G-FPACEI------X",61}, // sensor zone
            {"G-FPACDI------X",62}, // dead space area
            {"G-FPACZI------X",63}, // zone of responsibility
            {"G-FPACBI------X",64}, // target build up area
            {"G-FPACVI------X",65}, // target value area
            {"G-FPACT-------X",66}, // terminally guided munition footprint (TGMF)
            {"G-FPAZII------X",67}, // artillery target intelligence zone
            {"G-FPAZXI------X",68}, // call for fire zone
            {"G-FPAZCI------X",69}, // censor zone
            {"G-FPAZFI------X",70}, // critical friendly zone
            {"G-F-AKBI------X",71}, // kill box, blue
            {"G-F-AKPI------X",72}, // kill box, purple
            {"G-SPAD--------X",55}, // detainee holding area
            {"G-SPAE--------X",56}, // epow holding area
            {"G-SPAR--------X",34}, // forward arming and refueling area
            {"G-SPAH--------X",57}, // refugee holding area
            {"G-SPASB-------X",35}, // support area, brigade
            {"G-SPASD-------X",36}, // Division support area
            {"G-SPASR-------X",37}, // regimental support area
            // point 1 = pt2, point 2 = pt1, point 3 = pt3
            {"G-GPPD--------X",128}, // dummy deception
            {"G-GPDLP-------X",129}, // principal direction of fire
            // 3 points, 1 and 2 are the ends, the 3rd is the offset
            {"G-GPOLI-------X",140}, // Infiltration Lane
            {"G-MPORP-------X",113}, // obstacles, roadblock planned
            {"G-MPORS-------X",114}, // obstacles, roadblock explosives
            {"G-MPORA-------X",115}, // obstacles, roadblock explosives armed but passable
            {"G-MPORC-------X",116}, // same as above, complete
            {"G-MPOT--------X",119}, // trip wire
            {"G-MPBCE-------X",55}, // Crossing site ford easy
            {"G-MPBCD-------X",56}, // Crossing site ford difficult
            // 4 points,from point = between pt1, pt2, to point = between pt3, pt4
            {"G-GPOAS-------X",145}, // support by fire position
            // 4 points, from point = between pt1, pt3, to point = between pt2, pt4
            {"G-MPOFG-------X",118}, // Minefield Gap
            {"G-MPBCA-------X",120}, // Assault crossing
            {"G-MPBCB-------X",70}, // Crossing bridge gap
            // 3 points, from point is between pt2,pt3, to point is pt1, *** Marker offset
            {"G-GPOAF-------X",144}, // attack by fire position
            // 3 points, from point = between pt2, pt3, to point = pt1
            {"G-GPSLA-------X",136}, // Ambush
            // 2 point flipped
            {"G-TPF---------X",84}, // Task, Fix
            {"G-TPA---------X",89}, // Task, Follow and Assume
            {"G-TPAS--------X",90}, // Task, Follow and Support
            {"G-TPE---------X",85}, // Task, Isolate
            {"G-TPO---------X",91}, // Task, Occupy
            {"G-TPQ---------X",92}, // Task, Retain
            {"G-TPS---------X",93}, // Task, Secure
            {"G-GPPF--------X",127}, // Deception, Direction of attack for feint
            {"G-GPOLKA------X",2}, // Direction of Attack, Aviation
            {"G-GPOLKGM-----X",3}, // Ground, Main Attack
            {"G-GPOLKGS-----X",4}, // Ground, Supporting Attack
            {"G-MPOEF-------X",57}, // Obstacle Effect, Fix
            {"G-SPLCM-------X",58}, // Moving Convoy
            {"G-SPLCH-------X",59}, // Halted Convoy
            // 2 point, not flipped
            {"G-GPDLF-------X",10},
            {"G-MPBCF-------X",122}, // Crossing site/water crossing, ferry
            {"G-MPBCL-------X",23}, // Crossing site/water crossing, lane
            {"G-MPBCR-------X",121}, // crossing site/water crossing, raft site
            {"G-MPSW--------X",103}, // Foxhole, emplacement or weapon site
            {"G-FPLT--------X",16}, // Linear Target
            {"G-FPLTS-------X",123}, // Linear Smoke Target
            {"G-FPLTF-------X",124}, // Linear Target, Final protective fire (FPF)
            {"G-OPHN--------X",126}, // Hazard, Navigational
            {"G-OPB---------X",131}, // Bearing Line
            {"G-OPBE--------X",132}, // Bearing Line, Electronic
            {"G-OPBA--------X",133}, // Bearing Line, Acoustic
            {"G-OPBT--------X",134}, // Bearing Line, Torpedo
            {"G-OPBO--------X",135}, // Bearing Line, Electro-optical intercept
            // 2 point or more
            {"G-GPGLB-------X",29}, // General, Lines, Boundaries
            {"G-GPGLF-------X",22}, // Forward line of own troops
            {"G-GPGLC-------X",24}, // Line of contact
            {"G-GPGLP-------X",9}, // Phase Line
            {"G-GPGLL-------X",97}, // Light line
            {"G-GPOLF-------X",5}, // Final coordination line
            {"G-GPOLL-------X",6}, // Limit of advance
            {"G-GPOLT-------X",7}, // Line of Departure
            {"G-GPOLC-------X",8}, // Line of Departure/Line of Contact
            {"G-GPOLP-------X",141}, // Probable line of deployment
            {"G-GPSLR-------X",139}, // Release line
            {"G-MPOGL-------X",67}, // General Obstacle Line
            {"G-MPOS--------X",111}, // Obstacle Abatis
            {"G-MPOADU------X",100}, // Antitank ditch, under construction
            {"G-MPOADC------X",19}, // Antitank ditch, complete
            {"G-MPOAR-------X",101}, // Antitank ditch, with mines
            {"G-MPOAW-------X",112}, // Antitank wall
            {"G-MPOWU-------X",28}, // Wire Obstacle, unspecified
            {"G-MPOWS-------X",104}, // wire obstacle, single fence
            {"G-MPOWD-------X",105}, // wire obstacle, double fence
            {"G-MPOWA-------X",106}, // wire obstacle, double apron fence
            {"G-MPOWL-------X",107}, // wire obstacle, low wire fence
            {"G-MPOWH-------X",108}, // wire obstacle, high wire fence
            {"G-MPOWCS------X",109}, // Concertina, single
            {"G-MPOWCD------X",110}, // Concertina, double strand
            {"G-MPOWCT------X",27}, // Concertina, triple strand
            {"G-MPOHO-------X",117}, // Overhead wire
            {"G-MPSL--------X",102}, // Fortified Line
            {"G-FPLCF-------X",14}, // Fire support coordination line
            {"G-FPLCC-------X",12}, // Coordinated fire line
            {"G-FPLCN-------X",17}, // No fire line
            {"G-FPLCR-------X",18}, // Restricted Fire Line
            {"G-SPLRM-------X",25}, // Main Supply Route
            {"G-SPLRA-------X",60}, // Alternate Supply Route
            {"G-SPLRO-------X",61}, // One-way traffic
            {"G-SPLRT-------X",63}, // Alternating traffic
            {"G-SPLRW-------X",62}, // Two-way traffic
            // *******************************************METOC**********************************************
            {"WA--PFC----L---",1}, // Cold Front
            {"WA--PFCU---L---",2}, // upper cold front
            {"WA--PFC-FG-L---",3}, // cold frontogenesis
            {"WA--PFC-FY-L---",4}, // cold frontolysis
            {"WA--PFW----L---",5}, // warm front
            {"WA--PFWU---L---",6}, // upper warm front
            {"WA--PFW-FG-L---",7}, // warm frontogenesis
            {"WA--PFW-FY-L---",8}, // warm frontolysis
            {"WA--PFO----L---",9}, // occluded front
            {"WA--PFOU---L---",10}, // upper occluded front
            {"WA--PFO-FY-L---",11}, // occluded frontolysis
            {"WA--PFS----L---",12}, // stationary front
            {"WA--PFSU---L---",13}, // Upper Stationary Front
            {"WA--PFS-FG-L---",14}, // Stationary Frontogenesis
            {"WA--PFS-FY-L---",15}, // Stationary Frontolysis
            {"WA--PXT----L---",16}, // Trough axis
            {"WA--PXR----L---",17}, // Ridge axis
            {"WA--PXSQ---L---",18}, // Severe Squall Line
            {"WA--PXIL---L---",19}, // Instability Line
            {"WA--PXSH---L---",20}, // Shear Line
            {"WA--PXITCZ-L---",21}, // Inter-Tropical convergance Zone
            {"WA--PXCV---L---",22}, // Convergence Line
            {"WA--PXITD--L---",23}, // Inter-Tropical Discontinuity
            {"WA--IPIB---L---",28}, // ISOBAR - surface
            {"WA--IPCO---L---",29}, // Contour - upper air
            {"WA--IPIS---L---",30}, // Isotherm
            {"WA--IPIT---L---",31}, // Isotach
            {"WA--IPID---L---",32}, // Isodrosotherm
            {"WA--IPTH---L---",33}, // Isopleths thickness
            {"WA--IPFF---L---",34}, // freeform
            {"WO--IDID---L---",36}, // ice drift direction
            {"WO--ILOV---L---",37}, // limit of visual observation
            {"WO--ILUC---L---",38}, // limit of undercast
            {"WO--ILOR---L---",39}, // limit of radar observation
            {"WO--ILIEO--L---",40}, // observed ice edge or boundary
            {"WO--ILIEE--L---",41}, // estimated ice edge or boundary
            {"WO--ILIER--L---",42}, // ice edge or boundary from radar
            {"WO--IOC----L---",43}, // cracks
            {"WO--IOCS---L---",44}, // cracks at a specific location
            {"WO--IOL----L---",45}, // lead
            {"WO--IOLF---L---",46}, // frozen lead
            {"WO--HDDL---L---",47}, // depth curve
            {"WO--HDDC---L---",48}, // depth contour
            {"WO--HCC----L---",49}, // coastline
            {"WO--HCF----L---",50}, // foreshore
            {"WO--HPBA---L---",51}, // anchorage
            {"WO--HPBP---L---",52}, // pier/wharf/quay
            {"WO--HPMRA--L---",54}, // ramp, above water
            {"WO--HPMRB--L---",55}, // ramp, below water
            {"WO--HPSPA--L---",56}, // breakwater, groin, jetty above water
            {"WO--HPSPB--L---",57}, // below water
            {"WO--HPSPS--L---",58}, // seawall
            {"WO--HALLA--L---",59}, // leading line
            {"WO--HHDB---L---",60}, // breakers                     *********** Reef missing?
            {"WO--TCCCFE-L---",61}, // current flow
            {"WO--TCCCFF-L---",62}, // current flow flood
            {"WO--L-ML---L---",63}, // maritime limit boundary
            {"WO--L-RA---L---",64}, // restricted area
            {"WO--MCA----L---",65}, // submarine cable
            {"WO--MCD----L---",66}, // canal
            {"WO--MPA----L---",67}, // pipeline/pipe
            {"G-GPSLH-------X",138}, // holding line
            {"G-GPSLB-------X",137}, // Bridgehead
        // Type one is a tactical line graphic where there is 3 anchor points, drawn in arcmap with a two point line where
        // from point = pt3, to point = between pt1 and pt2, calculate width from pt1 and pt2
        // width will determine some layer effects, marker size/offset, line offset
            {"G-TPB---------X",80}, // Task, Block, *** Marker size
            {"G-TPH---------X",86}, // Task, Breach, *** Marker size, Marker +offset, Marker -offset, Line +offset, Line -offset
            {"G-TPY---------X",87}, // Task, Bypass, *** Marker size, Marker +offset, Marker -offset, Line +offset, Line -offset
            {"G-TPC---------X",81}, // Task, Canalize, *** Marker size, Marker +offset, Marker -offset, Line +offset, Line -offset
            {"G-TPX---------X",88}, // Task, Clear, *** Marker size, Marker +offset, Marker -offset, Line +offset, Line -offset
            {"G-TPJ---------X",82}, // Task, Contain, *** Marker size
            {"G-TPP---------X",76}, // Task, Penetrate, *** Marker size
            {"G-MPOEB-------X",26}, // Obstacle, effect block
            {"G-MPBDE-------X",21}, // Obstacle, bypass easy
            {"G-MPBDD-------X",53}, // Obstacle, bypass difficult
            {"G-MPBDI-------X",54}, // Obstacle, bypass impossible
        // Type two is a tactical line graphic where there is 3 anchor points, drawn in arcmap with a two point line where
        // from point = pt2, to point = pt1, width from pt2 and pt3
            {"G-TPL---------X",73}, // Task, Delay, *** Marker size
            {"G-TPM---------X",78}, // Task, Retirement, *** Marker size
            {"G-TPW---------X",79}, // Task, Withdraw, *** Marker size
            {"G-TPWP--------X",95}, // Task, Withdraw under pressure, *** Marker size

            {"G-GPALC-------X", 49}, // Air Coridor
            {"G-GPALM-------X", 51}, // MRR
            {"G-GPOLAA------X", 37}, // AOA Airborne


        };
    }

}
