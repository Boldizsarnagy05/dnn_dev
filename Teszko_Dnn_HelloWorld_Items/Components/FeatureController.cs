/*
' Copyright (c) 2026 Teszko
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

//using System.Xml;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Search;
using System.Collections.Generic;

namespace Teszko.Dnn.Teszko_Dnn_HelloWorld_Items.Components
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Controller class for Teszko_Dnn_HelloWorld_Items
    /// 
    /// The FeatureController class is defined as the BusinessController in the manifest file (.dnn)
    /// DotNetNuke will poll this class to find out which Interfaces the class implements. 
    /// 
    /// The IPortable interface is used to import/export content from a DNN module
    /// 
    /// The ISearchable interface is used by DNN to index the content of a module
    /// 
    /// The IUpgradeable interface allows module developers to execute code during the upgrade 
    /// process for a module.
    /// 
    /// Below you will find stubbed out implementations of each, uncomment and populate with your own data
    /// </summary>
    /// -----------------------------------------------------------------------------

    //uncomment the interfaces to add the support.
    public class FeatureController //: IPortable, ISearchable, IUpgradeable
    {


        #region Optional Interfaces

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ExportModule implements the IPortable ExportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be exported</param>
        /// -----------------------------------------------------------------------------
        //public string ExportModule(int ModuleID)
        //{
        //string strXML = "";

        //List<Teszko_Dnn_HelloWorld_ItemsInfo> colTeszko_Dnn_HelloWorld_Itemss = GetTeszko_Dnn_HelloWorld_Itemss(ModuleID);
        //if (colTeszko_Dnn_HelloWorld_Itemss.Count != 0)
        //{
        //    strXML += "<Teszko_Dnn_HelloWorld_Itemss>";

        //    foreach (Teszko_Dnn_HelloWorld_ItemsInfo objTeszko_Dnn_HelloWorld_Items in colTeszko_Dnn_HelloWorld_Itemss)
        //    {
        //        strXML += "<Teszko_Dnn_HelloWorld_Items>";
        //        strXML += "<content>" + DotNetNuke.Common.Utilities.XmlUtils.XMLEncode(objTeszko_Dnn_HelloWorld_Items.Content) + "</content>";
        //        strXML += "</Teszko_Dnn_HelloWorld_Items>";
        //    }
        //    strXML += "</Teszko_Dnn_HelloWorld_Itemss>";
        //}

        //return strXML;

        //	throw new System.NotImplementedException("The method or operation is not implemented.");
        //}

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ImportModule implements the IPortable ImportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be imported</param>
        /// <param name="Content">The content to be imported</param>
        /// <param name="Version">The version of the module to be imported</param>
        /// <param name="UserId">The Id of the user performing the import</param>
        /// -----------------------------------------------------------------------------
        //public void ImportModule(int ModuleID, string Content, string Version, int UserID)
        //{
        //XmlNode xmlTeszko_Dnn_HelloWorld_Itemss = DotNetNuke.Common.Globals.GetContent(Content, "Teszko_Dnn_HelloWorld_Itemss");
        //foreach (XmlNode xmlTeszko_Dnn_HelloWorld_Items in xmlTeszko_Dnn_HelloWorld_Itemss.SelectNodes("Teszko_Dnn_HelloWorld_Items"))
        //{
        //    Teszko_Dnn_HelloWorld_ItemsInfo objTeszko_Dnn_HelloWorld_Items = new Teszko_Dnn_HelloWorld_ItemsInfo();
        //    objTeszko_Dnn_HelloWorld_Items.ModuleId = ModuleID;
        //    objTeszko_Dnn_HelloWorld_Items.Content = xmlTeszko_Dnn_HelloWorld_Items.SelectSingleNode("content").InnerText;
        //    objTeszko_Dnn_HelloWorld_Items.CreatedByUser = UserID;
        //    AddTeszko_Dnn_HelloWorld_Items(objTeszko_Dnn_HelloWorld_Items);
        //}

        //	throw new System.NotImplementedException("The method or operation is not implemented.");
        //}

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSearchItems implements the ISearchable Interface
        /// </summary>
        /// <param name="ModInfo">The ModuleInfo for the module to be Indexed</param>
        /// -----------------------------------------------------------------------------
        //public DotNetNuke.Services.Search.SearchItemInfoCollection GetSearchItems(DotNetNuke.Entities.Modules.ModuleInfo ModInfo)
        //{
        //SearchItemInfoCollection SearchItemCollection = new SearchItemInfoCollection();

        //List<Teszko_Dnn_HelloWorld_ItemsInfo> colTeszko_Dnn_HelloWorld_Itemss = GetTeszko_Dnn_HelloWorld_Itemss(ModInfo.ModuleID);

        //foreach (Teszko_Dnn_HelloWorld_ItemsInfo objTeszko_Dnn_HelloWorld_Items in colTeszko_Dnn_HelloWorld_Itemss)
        //{
        //    SearchItemInfo SearchItem = new SearchItemInfo(ModInfo.ModuleTitle, objTeszko_Dnn_HelloWorld_Items.Content, objTeszko_Dnn_HelloWorld_Items.CreatedByUser, objTeszko_Dnn_HelloWorld_Items.CreatedDate, ModInfo.ModuleID, objTeszko_Dnn_HelloWorld_Items.ItemId.ToString(), objTeszko_Dnn_HelloWorld_Items.Content, "ItemId=" + objTeszko_Dnn_HelloWorld_Items.ItemId.ToString());
        //    SearchItemCollection.Add(SearchItem);
        //}

        //return SearchItemCollection;

        //	throw new System.NotImplementedException("The method or operation is not implemented.");
        //}

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpgradeModule implements the IUpgradeable Interface
        /// </summary>
        /// <param name="Version">The current version of the module</param>
        /// -----------------------------------------------------------------------------
        //public string UpgradeModule(string Version)
        //{
        //	throw new System.NotImplementedException("The method or operation is not implemented.");
        //}

        #endregion

    }

}
