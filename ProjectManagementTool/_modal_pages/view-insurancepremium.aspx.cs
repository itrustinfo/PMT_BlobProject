﻿using Newtonsoft.Json;
using ProjectManager.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ProjectManagementTool._modal_pages
{
    public partial class view_insurancepremium : System.Web.UI.Page
    {
        DBGetData getdata = new DBGetData();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null)
            {
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "CLOSE", "<script language='javascript'>parent.location.href=parent.location.href;</script>");
            }
            else
            {
                if (!IsPostBack)
                {
                    if (Request.QueryString["InsuranceUID"] != null)
                    {
                        BindPremium(Request.QueryString["InsuranceUID"]);
                    }
                }
            }
        }
        private void BindPremium(string InsuranceUID)
        {
            DataSet ds = getdata.GetInsurancePremiumSelect_by_InsuranceUID(new Guid(InsuranceUID));
            GrdPremium.DataSource = ds;
            GrdPremium.DataBind();
        }
        protected void GrdPremium_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string UID = e.CommandArgument.ToString();
            if (e.CommandName == "download")
            {
                byte[] bytes;
                DataSet ds = getdata.GetInsurancePremiumSelect_by_PremiumUID(new Guid(UID));
                if (ds.Tables[0].Rows.Count > 0)
                {
                    string path = Server.MapPath(ds.Tables[0].Rows[0]["Premium_Receipt"].ToString());

                    string filepath = Server.MapPath("~/_PreviewLoad/" + Path.GetFileName(path));

                    bytes = getdata.GetInsurancePremiumBlobData_PremiumUID(new Guid(UID));

                    BinaryWriter Writer = null;
                    Writer = new BinaryWriter(File.OpenWrite(filepath));

                    // Writer raw data                
                    Writer.Write(bytes);
                    Writer.Flush();
                    Writer.Close();

                    string getExtension = System.IO.Path.GetExtension(filepath);
                    string outPath = filepath.Replace(getExtension, "") + "_download" + getExtension;
                    getdata.DecryptFile(filepath, outPath);

                    System.IO.FileInfo file = new System.IO.FileInfo(outPath);

                    if (file.Exists)
                    {

                        Response.Clear();

                        Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(path));

                        Response.AddHeader("Content-Length", file.Length.ToString());

                        Response.ContentType = "application/octet-stream";

                        Response.WriteFile(file.FullName);

                        Response.End();

                    }

                    else
                    {
                        Page.ClientScript.RegisterStartupScript(Page.GetType(), "CLOSE", "<script language='javascript'>alert('File not found.');</script>");
                    }
                }
            }

            if(e.CommandName=="delete")
            {
                int cnt = getdata.InsurancePremium_Delete(new Guid(UID), new Guid(Session["UserUID"].ToString()));
                if (cnt > 0)
                {
                    if (WebConfigurationManager.AppSettings["Dbsync"] == "Yes")
                    {
                        string WebAPIURL = "";
                        DataSet copysite = getdata.GetDataCopySiteDetails_by_ProjectUID(new Guid(Request.QueryString["ProjectUID"]));
                        if (copysite.Tables[0].Rows.Count > 0)
                        {
                            WebAPIURL = copysite.Tables[0].Rows[0]["DataCopySiteURL"].ToString();
                            WebAPIURL = WebAPIURL + "Activity/InsurancePremiumDelete";
                            string postData = "PremiumUID=" + UID + "&UserUID=" + Session["UserUID"].ToString();
                            string sReturnStatus = getdata.webPostMethod(postData, WebAPIURL);
                            if (!sReturnStatus.StartsWith("Error:"))
                            {
                                dynamic DynamicData = JsonConvert.DeserializeObject(sReturnStatus);
                                string RetStatus = DynamicData.Status;
                                if (!RetStatus.StartsWith("Error:"))
                                {
                                    int rCnt = getdata.ServerFlagsUpdate(UID.ToString(), 2, "Insurance_Premiums", "Y", "PremiumUID");
                                    if (rCnt > 0)
                                    {
                                    }
                                }
                                else
                                {
                                    string ErrorMessage = DynamicData.Message;
                                    getdata.WebAPIStatusInsert(Guid.NewGuid(), WebAPIURL, postData, ErrorMessage, "Failure", "Insurance Premium Delete", "InsurancePremiumDelete", new Guid(UID));
                                    //Page.ClientScript.RegisterStartupScript(Page.GetType(), "CLOSE", "<script language='javascript'>alert('Error: DBSync =" + ErrorMessage + "');</script>");
                                }
                            }
                            else
                            {
                                getdata.WebAPIStatusInsert(Guid.NewGuid(), WebAPIURL, postData, sReturnStatus, "Failure", "Insurance Premium Delete", "InsurancePremiumDelete", new Guid(UID));
                            }
                        }
                    }
                    BindPremium(Request.QueryString["InsuranceUID"]);
                }
            }
        }

        protected void GrdPremium_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

        }

        protected void GrdPremium_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells[6].Text == "&nbsp;")
                {
                    LinkButton lnk = (LinkButton)e.Row.FindControl("lnkdownload");
                    lnk.Enabled = false;
                    lnk.Text = "No File";
                }
            }
        }
    }
}