using ProjectManager.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ProjectManagementTool._content_pages.Convert_to_blob
{
    public partial class _default : System.Web.UI.Page
    {
        DBGetData getdt = new DBGetData();
        TaskUpdate gettk = new TaskUpdate();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Username"] == null)
            {
                Response.Redirect("~/Login.aspx");
            }
            else
            {
                if (!IsPostBack)
                {
                    BindProject();
                    LblProgress.Visible = false;
                }

            }
        }

        private void BindProject()
        {
            DataTable ds = new DataTable();
            if (Session["TypeOfUser"].ToString() == "U" || Session["TypeOfUser"].ToString() == "MD" || Session["TypeOfUser"].ToString() == "VP")
            {
                ds = gettk.GetProjects();
            }
            else if (Session["TypeOfUser"].ToString() == "PA")
            {
                //ds = gettk.GetProjects_by_UserUID(new Guid(Session["UserUID"].ToString()));
                ds = gettk.GetAssignedProjects_by_UserUID(new Guid(Session["UserUID"].ToString()));
            }
            else
            {
                //ds = gettk.GetProjects();
                ds = gettk.GetAssignedProjects_by_UserUID(new Guid(Session["UserUID"].ToString()));
            }
            DDlProject.DataTextField = "ProjectName";
            DDlProject.DataValueField = "ProjectUID";
            DDlProject.DataSource = ds;
            DDlProject.DataBind();

        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            LblProgress.Visible = true;
            LblMessage.Visible = false;
            int TotalDocuments = 0, SuccessFullyConverted = 0, Errored = 0, StatusInsert = 0, StatusError = 0, VersionSuccess = 0, VersionError = 0, FileNotFound = 0;
            DataSet ds = getdt.GetAllDocumentsby_ProjectUID(new Guid(DDlProject.SelectedValue));
            if (ds.Tables[0].Rows.Count > 0)
            {
                TotalDocuments = ds.Tables[0].Rows.Count;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Guid ActualDocumentUID = new Guid(ds.Tables[0].Rows[i]["ActualDocumentUID"].ToString());
                    string path = Server.MapPath(ds.Tables[0].Rows[i]["ActualDocument_Path"].ToString());
                    try
                    {
                        if (File.Exists(path))
                        {
                            byte[] filetobytes = DBGetData.FileToByteArray(path);

                            int aDoc = getdt.ActualDocumentBlobInsertorUpdate(Guid.NewGuid(), ActualDocumentUID, filetobytes);
                            if (aDoc > 0)
                            {
                                //Insert into Logs table
                                SuccessFullyConverted += 1;
                                int alog = getdt.DocumenttoBlobLog_Insert(Guid.NewGuid(), ActualDocumentUID, "ActualDocuments", "Success", path);
                                //Insert into DocumentStatus Blob Table
                                DataSet dsstatus = getdt.getActualDocumentStatusList(ActualDocumentUID);
                                if (dsstatus.Tables[0].Rows.Count > 0)
                                {
                                    for (int j = 0; j < dsstatus.Tables[0].Rows.Count; j++)
                                    {
                                        try
                                        {
                                            byte[] Reviewfiletobytes = null;
                                            byte[] Coverfilebytes = null;
                                            string coverLetterPath = dsstatus.Tables[0].Rows[j]["CoverLetterFile"].ToString();
                                            string RevieFilePath = dsstatus.Tables[0].Rows[j]["LinkToReviewFile"].ToString();

                                            if (!string.IsNullOrEmpty(dsstatus.Tables[0].Rows[j]["LinkToReviewFile"].ToString()))
                                            {
                                                Reviewfiletobytes = DBGetData.FileToByteArray(Server.MapPath(RevieFilePath));
                                            }
                                            if (!string.IsNullOrEmpty(dsstatus.Tables[0].Rows[j]["CoverLetterFile"].ToString()))
                                            {
                                                Coverfilebytes = DBGetData.FileToByteArray(Server.MapPath(coverLetterPath));
                                            }
                                            int statuccount = getdt.DocumentStatusBlob_InsertorUpdate(Guid.NewGuid(), new Guid(dsstatus.Tables[0].Rows[j]["StatusUID"].ToString()), new Guid(dsstatus.Tables[0].Rows[j]["DocumentUID"].ToString()), Coverfilebytes, Reviewfiletobytes);
                                            if (statuccount > 0)
                                            {
                                                StatusInsert += 1;
                                                int log = getdt.DocumenttoBlobLog_Insert(Guid.NewGuid(), new Guid(dsstatus.Tables[0].Rows[j]["StatusUID"].ToString()), "DocumentStatus", "Success", coverLetterPath);
                                            }
                                            else
                                            {
                                                StatusError += 1;
                                                int log = getdt.DocumenttoBlobLog_Insert(Guid.NewGuid(), new Guid(dsstatus.Tables[0].Rows[j]["StatusUID"].ToString()), "DocumentStatus", "Error", coverLetterPath);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            StatusError += 1;
                                        }

                                        //Insert into DocumentVersionBlob Table
                                        byte[] VersionDoc = null;
                                        byte[] CoverFileDoc = null;
                                        DataSet dsversion = getdt.getDocumentVersions_by_StatusUID(new Guid(dsstatus.Tables[0].Rows[j]["StatusUID"].ToString()));
                                        for (int k = 0; k < dsversion.Tables[0].Rows.Count; k++)
                                        {
                                            try
                                            {

                                                string Versonpath = "";
                                                if (!string.IsNullOrEmpty(dsversion.Tables[0].Rows[k]["Doc_FileName"].ToString()))
                                                {
                                                    VersionDoc = DBGetData.FileToByteArray(Server.MapPath(dsversion.Tables[0].Rows[k]["Doc_FileName"].ToString()));
                                                }

                                                if (!string.IsNullOrEmpty(dsversion.Tables[0].Rows[k]["Doc_CoverLetter"].ToString()))
                                                {
                                                    CoverFileDoc = DBGetData.FileToByteArray(Server.MapPath(dsversion.Tables[0].Rows[k]["Doc_CoverLetter"].ToString()));
                                                }

                                                int versionCnt = getdt.DocumentVersionBlob_insertorUpdate(Guid.NewGuid(), new Guid(dsversion.Tables[0].Rows[k]["DocVersion_UID"].ToString()), ActualDocumentUID, CoverFileDoc, VersionDoc);
                                                if (versionCnt > 0)
                                                {
                                                    VersionSuccess += 1;
                                                    int log = getdt.DocumenttoBlobLog_Insert(Guid.NewGuid(), new Guid(dsversion.Tables[0].Rows[k]["DocVersion_UID"].ToString()), "DocumentVesrion", "Success", dsversion.Tables[0].Rows[k]["Doc_FileName"].ToString());
                                                }
                                                else
                                                {
                                                    VersionError += 1;
                                                    int log = getdt.DocumenttoBlobLog_Insert(Guid.NewGuid(), new Guid(dsversion.Tables[0].Rows[k]["DocVersion_UID"].ToString()), "DocumentVesrion", "Error", dsversion.Tables[0].Rows[k]["Doc_FileName"].ToString());
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                VersionError += 1;
                                                int log = getdt.DocumenttoBlobLog_Insert(Guid.NewGuid(), new Guid(dsversion.Tables[0].Rows[k]["DocVersion_UID"].ToString()), "DocumentVesrion", "Error", dsversion.Tables[0].Rows[k]["Doc_FileName"].ToString());
                                            }
                                        }
                                    }
                                }


                            }
                            else
                            {
                                Errored += 1;
                                int elog = getdt.DocumenttoBlobLog_Insert(Guid.NewGuid(), ActualDocumentUID, "ActualDocuments", "Failure", path);
                            }
                        }
                        else
                        {
                            FileNotFound += 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Errored += 1;

                        int log = getdt.DocumenttoBlobLog_Insert(Guid.NewGuid(), ActualDocumentUID, "ActualDocuments", "Failure", path);
                    }
                }

                
            }
            LblMessage.Text = "Total Documents : " + TotalDocuments + ", FileNotFound : " + FileNotFound + " Converted Documents : " + SuccessFullyConverted + ", Errored Documents : " + Errored;
            LblProgress.Visible = false;
            LblMessage.Visible = true;
        }
    }
}